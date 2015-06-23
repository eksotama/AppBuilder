﻿using System;
using System.Collections.Generic;
using Core.Data;
using Core.Objects;

namespace Core.Helpers
{
	public abstract class Helper<T> : IHelper<T> where T : IReadOnlyObject
	{
		private readonly Dictionary<long, T> _items = new Dictionary<long, T>();

		public Dictionary<long, T> Items
		{
			get { return _items; }
		}

		public void Load(IReadOnlyAdapter<T> adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.Items);
		}
	}
}