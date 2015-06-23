using Core.Objects;

namespace Core.Data
{
	public interface IModifiableAdapter<in T> where T : IModifiableObject
	{
		void Insert(T item);
		void Update(T item);
		void Delete(T item);
	}
}