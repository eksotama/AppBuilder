using System;
using System.Collections.Generic;
using System.Data;

namespace AppBuilder
{
	public sealed class QueryHelper
	{
		public static IDbConnection Connection { get; set; }

		public int ExecuteQuery(string query)
		{
			return ExecuteQuery(query, new IDbDataParameter[0]);
		}

		public int ExecuteQuery(string query, IEnumerable<IDbDataParameter> parameters)
		{
			if (parameters == null) throw new ArgumentNullException("parameters");

			int total;

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;
				cmd.Parameters.Clear();

				foreach (IDbDataParameter p in parameters)
				{
					cmd.Parameters.Add(p);
				}

				total = cmd.ExecuteNonQuery();
			}

			return total;
		}

		public List<T> ExecuteReader<T>(string query, Func<IDataReader, T> creator)
		{
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}

			return ExecuteReader(query, creator, new IDbDataParameter[0]);
		}

		public List<T> ExecuteReader<T>(string query, Func<IDataReader, T> creator, IEnumerable<IDbDataParameter> parameters)
		{
			return ExecuteReader(query, creator, parameters, 4);
		}

		public List<T> ExecuteReader<T>(string query, Func<IDataReader, T> creator, int capacity)
		{
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}

			return ExecuteReader(query, creator, new IDbDataParameter[0], capacity);
		}

		public List<T> ExecuteReader<T>(string query, Func<IDataReader, T> creator, IEnumerable<IDbDataParameter> parameters, int capacity)
		{
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}

			var items = new List<T>(capacity);
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;

				foreach (IDbDataParameter p in parameters)
				{
					cmd.Parameters.Add(p);
				}

				using (IDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						T v = creator(reader);
						items.Add(v);
					}
				}
			}

			return items;
		}

		public List<T> ExecuteReader<T, TC>(string query, Func<IDataReader, TC, T> creator, TC contructorValue)
		{
			return ExecuteReader(query, creator, contructorValue, 4);
		}

		public List<T> ExecuteReader<T, TC>(string query, Func<IDataReader, TC, T> creator, TC contructorValue, int capacity)
		{
			return ExecuteReader(query, creator, contructorValue, new IDbDataParameter[0], capacity);
		}

		public List<T> ExecuteReader<T, TC>(string query, Func<IDataReader, TC, T> creator, TC contructorValue,
			IEnumerable<IDbDataParameter> parameters)
		{
			return ExecuteReader(query, creator, contructorValue, parameters, 4);
		}

		public List<T> ExecuteReader<T, TC>(string query, Func<IDataReader, TC, T> creator, TC contructorValue,
			IEnumerable<IDbDataParameter> parameters, int capacity)
		{
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}

			var items = new List<T>(capacity);
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query;

				foreach (var p in parameters)
				{
					cmd.Parameters.Add(p);
				}

				using (IDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var v = creator(reader, contructorValue);
						items.Add(v);
					}
				}
			}

			return items;
		}

		public void Fill<T>(List<T> items, string query, Func<IDataReader, T> creator)
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

		public void Fill<T>(Dictionary<long, T> items, string query, Func<IDataReader, T> creator, Func<T, long> selector)
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