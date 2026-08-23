using FiveMCleaner.App.Services;

namespace FiveMCleaner.App.Views;

public partial class TermsOfUseWindow : Wpf.Ui.Controls.FluentWindow
{
    public TermsOfUseWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string VersionLabel => LocalizationService.Current.Format("Terms.VersionLabel", AccountTerms.CurrentVersion);
}
