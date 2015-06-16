using System.Threading.Tasks;

namespace Architecture.Dialog
{
	public interface IDialog
	{
		Task<ConfirmationResult> ConfirmAsync(string message);
		Task<ConfirmationResult> DisplayAsync(string message);
	}
}