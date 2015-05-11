using System;
using System.Collections.Generic;
using System.Data;

namespace Demo
{
	public static class QueryHelper
	{
		public static IDbConnection Connection { get; set; }

		public static int ExecuteQuery(string query)
		{
			return ExecuteQuery(query, new IDbDataParameter[0]);
		}

		public static int ExecuteQuery(string query, IEnumerable<IDbDataParameter> parameters)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (parameters == null) throw new ArgumentNullException("parameters");

			int total;

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;

				foreach (var p in parameters)
				{
					cmd.Parameters.Add(p);
				}

				total = cmd.ExecuteNonQuery();
			}

			return total;
		}

		public static List<T> Get<T>(string query, Func<IDataReader, T> creator)
		{
			return Get(query, creator, new IDbDataParameter[0]);
		}

		public static List<T> Get<T>(string query, Func<IDataReader, T> creator, IEnumerable<IDbDataParameter> parameters)
		{
			return Get(query, creator, parameters, 4);
		}

		public static List<T> Get<T>(string query, Func<IDataReader, T> creator, int capacity)
		{
			return Get(query, creator, new IDbDataParameter[0], capacity);
		}

		public static List<T> Get<T>(string query, Func<IDataReader, T> creator, IEnumerable<IDbDataParameter> parameters, int capacity)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (creator == null) throw new ArgumentNullException("creator");
			if (parameters == null) throw new ArgumentNullException("parameters");

			var items = new List<T>(capacity);
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;

				foreach (var p in parameters)
				{
					cmd.Parameters.Add(p);
				}

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var v = creator(reader);
						items.Add(v);
					}
				}
			}

			return items;
		}

		public static void Fill<T>(List<T> items, string query, Func<IDataReader, T> creator)
		{
			if (items == null) throw new ArgumentNullException("items");
			if (query == null) throw new ArgumentNullException("query");
			if (creator == null) throw new ArgumentNullException("creator");

			items.Clear();

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;

				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						items.Add(creator(r));
					}
				}
			}
		}

		public static void Fill<T>(Dictionary<long, T> items, string query, Func<IDataReader, T> creator, Func<T, long> selector)
		{
			if (items == null) throw new ArgumentNullException("items");
			if (query == null) throw new ArgumentNullException("query");
			if (creator == null) throw new ArgumentNullException("creator");
			if (selector == null) throw new ArgumentNullException("selector");

			items.Clear();

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;

				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						var item = creator(r);
						items.Add(selector(item), item);
					}
				}
			}
		}
	}
}