using System.Collections.Generic;
using Core.Data;
using Core.Objects;

namespace Core.Helpers
{
	public interface IHelper<T> where T : IReadOnlyObject
	{
		Dictionary<long, T> Items { get; }

		void Load(IReadOnlyAdapter<T> adapter);
	}
}