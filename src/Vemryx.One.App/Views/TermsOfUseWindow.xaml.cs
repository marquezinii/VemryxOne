using Vemryx.One.App.Services;

namespace Vemryx.One.App.Views;

public partial class TermsOfUseWindow : Wpf.Ui.Controls.FluentWindow
{
    public TermsOfUseWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string VersionLabel => LocalizationService.Current.Format("Terms.VersionLabel", AccountTerms.CurrentVersion);
}
