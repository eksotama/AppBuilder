using System;

namespace AppBuilder.Db
{
	public sealed class DbTable
	{
		public string Name { get; private set; }

		public DbTable(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
		}
	}
}