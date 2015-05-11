using System.Windows;
using AppBuilder.UI.ViewModels;

namespace AppBuilder.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly DbViewModel _viewModel = new DbViewModel();

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = _viewModel;
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			_viewModel.Load();
		}
	}
}
