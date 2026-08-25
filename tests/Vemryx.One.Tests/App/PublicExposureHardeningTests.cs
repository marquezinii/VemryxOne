using Xunit;

namespace Vemryx.One.Tests.App;

public sealed class PublicExposureHardeningTests
{
    [Fact]
    public void StableRelease_RequiresExactOriginMainBeforeSecretBearingSteps()
    {
        var root = FindRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "release.yml"));

        var provenanceGuard = workflow.IndexOf(
            "Stable release tag must point to the current origin/main commit.",
            StringComparison.Ordinal);
        var firstSecretReference = workflow.IndexOf("${{ secrets.", StringComparison.Ordinal);

        Assert.True(provenanceGuard >= 0, "Stable releases must validate the trusted origin/main commit.");
        Assert.True(
            firstSecretReference > provenanceGuard,
            "The provenance guard must run before any workflow secret is referenced.");
        Assert.Contains("refs/remotes/origin/main", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseSigning_IsIsolatedFromBuildAndProductionPublishing()
    {
        var root = FindRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "release.yml"));
        var buildEnd = workflow.IndexOf("  sign_release:", StringComparison.Ordinal);
        var publishStart = workflow.IndexOf("  publish:", buildEnd, StringComparison.Ordinal);

        Assert.True(buildEnd > 0, "The unsigned build and signing jobs must remain separate.");
        Assert.True(publishStart > buildEnd, "The signing job must complete before publishing.");
        Assert.DoesNotContain("SIGNING_PRIVATE_KEY", workflow[..buildEnd], StringComparison.Ordinal);
        Assert.Contains("environment: release-signing", workflow, StringComparison.Ordinal);
        Assert.Contains("environment: production", workflow, StringComparison.Ordinal);
        Assert.Contains("BROKER_INTEGRITY_SIGNING_PRIVATE_KEY", workflow, StringComparison.Ordinal);
        Assert.Contains("RELEASE_SIGNING_PRIVATE_KEY", workflow[buildEnd..publishStart], StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeTrustAnchors_AreSeparatedByPurpose()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "Vemryx.One.App", "Vemryx.One.App.csproj"));
        var updater = File.ReadAllText(Path.Combine(root, "src", "Vemryx.One.App", "Services", "SignedManifestUpdateService.cs"));
        var broker = File.ReadAllText(Path.Combine(root, "src", "Vemryx.One.App", "Services", "BrokerIntegrityVerifier.cs"));

        Assert.Contains("Assets/update-manifest-public-key.pem", project, StringComparison.Ordinal);
        Assert.Contains("Assets/broker-integrity-public-key.pem", project, StringComparison.Ordinal);
        Assert.Contains("Assets.update-manifest-public-key.pem", updater, StringComparison.Ordinal);
        Assert.Contains("Assets.broker-integrity-public-key.pem", broker, StringComparison.Ordinal);

        var updateKey = File.ReadAllText(Path.Combine(root, "src", "Vemryx.One.App", "Assets", "update-manifest-public-key.pem"));
        var brokerKey = File.ReadAllText(Path.Combine(root, "src", "Vemryx.One.App", "Assets", "broker-integrity-public-key.pem"));
        Assert.NotEqual(updateKey, brokerKey);
    }

    [Fact]
    public void Startup_DoesNotWriteRawExceptionsToDeveloperSpecificPath()
    {
        var root = FindRepositoryRoot();
        var appSource = File.ReadAllText(Path.Combine(root, "src", "Vemryx.One.App", "App.xaml.cs"));

        Assert.DoesNotContain(@"C:\Projetos\fivemcleaner-debug.log", appSource, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Vemryx.One.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
