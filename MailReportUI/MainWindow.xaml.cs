using System.Windows;
using System.Windows.Controls;

namespace MailReportUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly ReportGeneratorViewModel _viewModel = new ReportGeneratorViewModel();

		public ReportGeneratorViewModel ViewModel
		{
			get { return _viewModel; }
		}

		public MainWindow()
		{
			this.InitializeComponent();
			this.DataContext = this.ViewModel;
		}

		private void PbPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			this.ViewModel.PasswordChanged((sender as PasswordBox).Password);
		}
	}
}
