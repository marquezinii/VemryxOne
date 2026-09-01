namespace Ralven.Contracts;

public static class ProductIdentity
{
    public const string DisplayName = "Ralven";
    public const string Name = "Ralven";
    public const string Subtitle = "optimization, performance and practicality";
    public const string RepositoryUrl = "https://github.com/marquezinii/Ralven";
    public const string ReleasesUrl = RepositoryUrl + "/releases";
    public const string DiscordInviteUrl = "https://discord.gg/bazcuQB9n6";
    public const string AdministratorReceiptRegistryPath = @"SOFTWARE\Ralven\BrokerReceipts";

    /// <summary>Bump when the plan stops being readable by an older broker, which rejects any other value.</summary>
    public const int PlanSchemaVersion = 2;
}
