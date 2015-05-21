using System;

namespace AppBuilder.Db.DML
{
	public sealed class DbQuery
	{
		public string Statement { get; private set; }
		public DbQueryParameter[] Parameters { get; private set; }

		public DbQuery(string statement)
			: this(statement, new DbQueryParameter[0])
		{
		}

		public DbQuery(string statement, DbQueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException("statement");
			if (parameters == null) throw new ArgumentNullException("parameters");

			this.Statement = statement;
			this.Parameters = parameters;
		}
	}
}