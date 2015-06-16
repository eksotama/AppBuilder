using Architecture.Objects;

namespace Architecture.Data
{
	public interface IModifiableAdapter<in T> where T : IModifiableObject
	{
		void Insert(T item);
		void Update(T item);
		void Delete(T item);
	}
}