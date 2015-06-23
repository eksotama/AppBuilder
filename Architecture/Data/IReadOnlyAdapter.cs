using System.Collections.Generic;
using Core.Objects;

namespace Core.Data
{
	public interface IReadOnlyAdapter<T> where T : IReadOnlyObject
	{
		void Fill(Dictionary<long, T> items);
	}
}