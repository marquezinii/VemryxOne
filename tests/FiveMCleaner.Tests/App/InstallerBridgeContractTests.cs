using Xunit;

namespace FiveMCleaner.Tests.App;

public sealed class InstallerBridgeContractTests
{
    [Fact]
    public void ReleaseBridge_UsesThePublicInstallerWhileKeepingTheLegacyUpdateAlias()
    {
        var root = TestHelpers.FindRepositoryRoot();
        var installer = File.ReadAllText(Path.Combine(root, "installer", "FiveMCleaner.iss"));
        var build = File.ReadAllText(Path.Combine(root, "scripts", "Build-Installer.ps1"));
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "release.yml"));

        Assert.Contains("#define AppName \"Vemryx One\"", installer, StringComparison.Ordinal);
        Assert.Contains("#define InstallerBaseName \"VemryxOne-Setup-\"", installer, StringComparison.Ordinal);
        Assert.Contains("#define AppExeName \"FiveMCleaner.Launcher.exe\"", installer, StringComparison.Ordinal);
        Assert.Contains("#define StableAppId \"{{49338651-127F-4FD3-BEAD-88D8C9377672}\"", installer, StringComparison.Ordinal);

        Assert.Contains("$baseName = \"VemryxOne-Setup-$Version-win-x64\"", build, StringComparison.Ordinal);
        Assert.Contains("$legacyBaseName = \"FiveMCleaner-Setup-$Version-win-x64\"", build, StringComparison.Ordinal);
        Assert.Contains("Copy-Item -LiteralPath $finalInstaller -Destination $legacyInstaller", build, StringComparison.Ordinal);

        Assert.Contains("VemryxOne-Setup-$env:ASSET_VERSION-win-x64.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("FiveMCleaner-Setup-$env:ASSET_VERSION-win-x64.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("VemryxOne-Setup-latest-win-x64.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("FiveMCleaner-Setup-latest-win-x64.exe", workflow, StringComparison.Ordinal);
    }
}
