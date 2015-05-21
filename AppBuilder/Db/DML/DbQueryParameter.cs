using System;

namespace AppBuilder.Db.DML
{
	public sealed class DbQueryParameter
	{
		public string Name { get; private set; }
		public object Value { get; private set; }

		public DbQueryParameter(string name, object value)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (value == null) throw new ArgumentNullException("value");

			this.Name = name;
			this.Value = value;
		}
	}
}