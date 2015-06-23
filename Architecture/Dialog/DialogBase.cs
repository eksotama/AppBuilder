using System;
using System.Threading.Tasks;

namespace Core.Dialog
{
	public abstract class DialogBase
	{
		public string Message { get; set; }
		public Action AcceptAction { get; set; }
		public Action DeclineAction { get; set; }
		public Action CancelAction { get; set; }

		private readonly Action _emptyAction = () => { };

		protected DialogBase()
		{
			this.Message = string.Empty;
			this.AcceptAction = _emptyAction;
			this.DeclineAction = _emptyAction;
			this.CancelAction = _emptyAction;
		}

		public Task DisplayAsync(string message)
		{
			if (message == null) throw new ArgumentNullException("message");

			this.Message = message;

			return this.ShowAsync();
		}

		public abstract Task ShowAsync();
	}
}