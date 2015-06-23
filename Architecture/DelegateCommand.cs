using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Core
{
	public sealed class DelegateCommand<T> : ICommand where T : class
	{
		private readonly Func<T, bool> _canExecuteMethod;
		private readonly Func<T, Task> _executeMethod;

		public DelegateCommand(Func<T, Task> executeMethod)
			: this(executeMethod, null)
		{
		}

		public DelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
		{
			_executeMethod = executeMethod;
			_canExecuteMethod = canExecuteMethod;
		}

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
			this.OnCanExecuteChanged(EventArgs.Empty);
		}

		private void OnCanExecuteChanged(EventArgs e)
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}