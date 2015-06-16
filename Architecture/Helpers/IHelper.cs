using System.Collections.Generic;
using Architecture.Data;
using Architecture.Objects;

namespace Architecture.Helpers
{
	public interface IHelper<T> where T : IReadOnlyObject
	{
		Dictionary<long, T> Items { get; }

		void Load(IReadOnlyAdapter<T> adapter);
	}
}