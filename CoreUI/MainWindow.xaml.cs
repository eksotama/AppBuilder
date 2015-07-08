using System;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.Dialog;

namespace CoreUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly LogMessageViewManager _viewManager = new LogMessageViewManager(new LoginManager(new LoginAdapter(), new LoginSettings()), new WpfDialog());

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = _viewManager;
		}
	}

	public sealed class WpfDialog : DialogBase
	{
		public override Task ShowAsync(string message, DialogType? type = null)
		{
			if (message == null) throw new ArgumentNullException("message");

			var boxButton = MessageBoxButton.OK;
			if (type.HasValue)
			{
				var value = type.Value;
				if (value == DialogType.YesNo)
				{
					boxButton = MessageBoxButton.YesNo;
				}
				if (value == DialogType.YesNoCancel)
				{
					boxButton = MessageBoxButton.YesNoCancel;
				}
			}
			var result = MessageBox.Show(message, @"Core", boxButton);
			switch (result)
			{
				case MessageBoxResult.OK:
				case MessageBoxResult.Yes:
					this.AcceptAction();
					break;
				case MessageBoxResult.Cancel:
					this.CancelAction();
					break;
				case MessageBoxResult.No:
					this.DeclineAction();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return Task.FromResult(true);
		}
	}
}
