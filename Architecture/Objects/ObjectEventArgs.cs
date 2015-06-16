using System;

namespace Architecture.Objects
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