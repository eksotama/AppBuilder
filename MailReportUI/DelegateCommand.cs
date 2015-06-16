using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MailReportUI
{
	public sealed class DelegateCommand<T> : ICommand where T : class
	{
		private readonly Func<T, bool> _canExecuteMethod;
		private readonly Func<T, Task> _executeMethod;

		#region Constructors

		public DelegateCommand(Func<T, Task> executeMethod)
			: this(executeMethod, null)
		{
		}

		public DelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
		{
			_executeMethod = executeMethod;
			_canExecuteMethod = canExecuteMethod;
		}

		#endregion Constructors

		#region ICommand Members

		public event EventHandler CanExecuteChanged;

		bool ICommand.CanExecute(object parameter)
		{
			var value = parameter as T;
			if (value != null)
			{
				return this.CanExecute(value);
			}
			return false;
		}

		void ICommand.Execute(object parameter)
		{
			Execute((T)parameter);
		}

		#endregion ICommand Members

		#region Public Methods

		public bool CanExecute(T parameter)
		{
			return ((_canExecuteMethod == null) || _canExecuteMethod(parameter));
		}

		public async void Execute(T parameter)
		{
			if (_executeMethod != null)
			{
				await _executeMethod(parameter);
			}
		}

		public void RaiseCanExecuteChanged()
		{
			OnCanExecuteChanged(EventArgs.Empty);
		}

		#endregion Public Methods

		#region Protected Methods

		public void OnCanExecuteChanged(EventArgs e)
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion Protected Methods
	}
}