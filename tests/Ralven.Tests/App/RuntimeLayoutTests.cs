using Ralven.App.Services;
using Xunit;

namespace Ralven.Tests.App;

public sealed class RuntimeLayoutTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "RalvenRuntimeLayout", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void Resolve_RecognizesTheRealInstallerLayout()
    {
        // {installRoot}/Runtime/versions/{version}/Ralven.exe -- exactly
        // what Ralven.iss and RuntimePackageStager produce.
        var baseDirectory = Path.Combine(root, "Ralven", "Runtime", "versions", "1.2.3");
        Directory.CreateDirectory(baseDirectory);

        var result = RuntimeLayout.Resolve(baseDirectory + Path.DirectorySeparatorChar);

        Assert.Equal(Path.Combine(root, "Ralven"), result.InstallRoot);
        Assert.Equal(Path.Combine(root, "Ralven", "Runtime"), result.RuntimeRoot);
    }

    [Theory]
    [InlineData("Ralven", "Runtime", "notversions")]
    [InlineData("Ralven", "NotRuntime", "versions")]
    public void Resolve_RejectsALayoutThatDoesNotMatchExactly(string installRootName, string runtimeName, string versionsName)
    {
        var baseDirectory = Path.Combine(root, installRootName, runtimeName, versionsName, "1.2.3");
        Directory.CreateDirectory(baseDirectory);

        var result = RuntimeLayout.Resolve(baseDirectory);

        Assert.Null(result.InstallRoot);
        Assert.Null(result.RuntimeRoot);
    }

    [Fact]
    public void Resolve_RejectsAStandaloneOrDevRunWithNoRuntimeAncestors()
    {
        var baseDirectory = Path.Combine(root, "SomeDevOutput", "bin", "Release");
        Directory.CreateDirectory(baseDirectory);

        var result = RuntimeLayout.Resolve(baseDirectory);

        Assert.Null(result.InstallRoot);
        Assert.Null(result.RuntimeRoot);
    }
}
