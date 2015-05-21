using System;

namespace AppBuilder.Db.DML
{
	public sealed class DbQueryParameter
	{
		public string Name { get; private set; }

		public DbQueryParameter(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
		}
	}
}