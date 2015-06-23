using System;

namespace Core.Objects
{
	public sealed class ObjectEventArgs<T> : EventArgs where T : IModifiableObject
	{
		public T Item { get; private set; }

		public ObjectEventArgs(T item)
		{
			this.Item = item;
		}
	}
}