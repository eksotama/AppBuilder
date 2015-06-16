using System.Collections.Generic;
using Architecture.Objects;

namespace Architecture.Data
{
	public interface IReadOnlyAdapter<T> where T : IReadOnlyObject
	{
		void Fill(Dictionary<long, T> items);
	}
}