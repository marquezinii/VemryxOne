using Ralven.UpdateRuntime;
using Xunit;

namespace Ralven.Tests.UpdateRuntime;

public sealed class RuntimeActivationStoreTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "RalvenRuntime", Guid.NewGuid().ToString("N"));
    public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root, true); }

    [Fact]
    public void Activate_SwapsOnlyThePointerBetweenStagedVersions()
    {
        var store = new RuntimeActivationStore(root);
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "1.0.0"));
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "1.1.0"));
        store.Activate("1.0.0");
        store.Activate("1.1.0");
        Assert.Equal("1.1.0", store.ReadActiveVersion());
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "1.0.0")));
    }

    [Fact]
    public void ReadActiveVersion_RetriesThroughATransientLockInsteadOfThrowing()
    {
        var store = new RuntimeActivationStore(root);
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "1.0.0"));
        store.Activate("1.0.0");
        var pointerPath = Path.Combine(root, "active.json");

        // A escrita atômica concorrente de outro launcher, ou um antivírus
        // segurando active.json por poucos milissegundos, não pode derrubar a
        // abertura do app: ReadActiveVersion deve tentar de novo em vez de
        // propagar o IOException do lock transitório. Uma thread dedicada
        // (não o thread pool, que fica sob contenção real com o resto da
        // suíte rodando em paralelo em runners de CI) segura o lock até
        // ReadActiveVersion já estar dentro do laço de retry, comprovado por
        // um ManualResetEventSlim, e só então libera -- sem depender de
        // nenhum tempo fixo de espera.
        using var lockAcquired = new ManualResetEventSlim(false);
        using var releaseLock = new ManualResetEventSlim(false);
        Exception? lockThreadException = null;
        var lockThread = new Thread(() =>
        {
            try
            {
                using var handle = new FileStream(pointerPath, FileMode.Open, FileAccess.Read, FileShare.None);
                lockAcquired.Set();
                releaseLock.Wait();
            }
            catch (Exception exception)
            {
                lockThreadException = exception;
                lockAcquired.Set();
            }
        })
        {
            IsBackground = true
        };
        lockThread.Start();
        lockAcquired.Wait(cancellationToken: global::Xunit.TestContext.Current.CancellationToken);
        Assert.Null(lockThreadException);

        // ReadActiveVersion já está tentando contra o arquivo travado; libera
        // o lock em paralelo enquanto ele retenta, para que a primeira
        // tentativa falhe e uma tentativa seguinte pegue o arquivo livre.
        var releaseThread = new Thread(() =>
        {
            Thread.Sleep(50);
            releaseLock.Set();
        })
        {
            IsBackground = true
        };
        releaseThread.Start();

        try
        {
            Assert.Equal("1.0.0", store.ReadActiveVersion());
        }
        finally
        {
            releaseLock.Set();
            lockThread.Join();
            releaseThread.Join();
        }
    }

    [Fact]
    public void PruneVersionsExcept_RemovesOnlyRecognizedUnpreservedVersionsAndKeepsTheActiveVersion()
    {
        var store = new RuntimeActivationStore(root);
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "0.9.0"));
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "1.0.0"));
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "1.1.0"));
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "manual-recovery"));
        store.Activate("1.1.0");

        store.PruneVersionsExcept("1.0.0");

        Assert.False(Directory.Exists(Path.Combine(store.VersionsRoot, "0.9.0")));
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "1.0.0")));
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "1.1.0")));
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "manual-recovery")));
    }

    [Fact]
    public void PruneVersionsExcept_RejectsAnInvalidPreservationSetBeforeDeletingAnything()
    {
        var store = new RuntimeActivationStore(root);
        var oldVersion = Path.Combine(store.VersionsRoot, "1.0.0");
        Directory.CreateDirectory(oldVersion);

        Assert.Throws<ArgumentException>(() => store.PruneVersionsExcept(".."));

        Assert.True(Directory.Exists(oldVersion));
    }

    [Fact]
    public void PruneVersionsExcept_DoesNotFailTheUpdateWhenAnOldVersionIsInUse()
    {
        var store = new RuntimeActivationStore(root);
        var oldVersion = Path.Combine(store.VersionsRoot, "1.0.0");
        Directory.CreateDirectory(oldVersion);
        Directory.CreateDirectory(Path.Combine(store.VersionsRoot, "2.0.0"));
        store.Activate("2.0.0");
        var lockedFile = Path.Combine(oldVersion, "Ralven.dll");
        File.WriteAllText(lockedFile, "old");

        using (File.Open(lockedFile, FileMode.Open, FileAccess.Read, FileShare.None))
            store.PruneVersionsExcept("2.0.0");

        Assert.True(Directory.Exists(oldVersion));
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "2.0.0")));
    }

    [Fact]
    public void PruneInactiveVersions_KeepsOnlyTheActiveVersionAndItsClosestPredecessor()
    {
        var store = new RuntimeActivationStore(root);
        foreach (var version in new[] { "0.9.0", "1.0.0", "1.1.0", "2.0.0" })
            Directory.CreateDirectory(Path.Combine(store.VersionsRoot, version));
        store.Activate("1.1.0");

        store.PruneInactiveVersions();

        Assert.False(Directory.Exists(Path.Combine(store.VersionsRoot, "0.9.0")));
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "1.0.0")));
        Assert.True(Directory.Exists(Path.Combine(store.VersionsRoot, "1.1.0")));
        Assert.False(Directory.Exists(Path.Combine(store.VersionsRoot, "2.0.0")));
    }
}
