using System;
using System.Collections.Generic;
using Architecture.Objects;

namespace Architecture
{
	public interface IManager<T> where T : IModifiableObject
	{
		List<T> Items { get; }

		event EventHandler<ObjectEventArgs<T>> ItemInserted;
		event EventHandler<ObjectEventArgs<T>> ItemUpdated;
		event EventHandler<ObjectEventArgs<T>> ItemDeleted;

		//void InsertAsync(T item, IDialog dialog);
		//void UpdateAsync(T item, IDialog dialog);
		//void DeleteAsync(T item, IDialog dialog);
	}
}