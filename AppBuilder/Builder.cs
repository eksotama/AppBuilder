using System;
using System.IO;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public class Builder
	{
		private readonly DirectoryInfo _rootDirectory;

		public Builder(DirectoryInfo rootDirectory)
		{
			if (rootDirectory == null) throw new ArgumentNullException("rootDirectory");

			_rootDirectory = rootDirectory;
		}

		public void BuilderApp(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			if (_rootDirectory.Exists)
			{
				_rootDirectory.Delete(true);
			}
			_rootDirectory.Create();

			var dbScript = DbSchemaParser.GenerateScript(schema);
			File.WriteAllText(Path.Combine(_rootDirectory.FullName, @"script.sql"), dbScript);

			// Objects
			var objectsPath = Path.Combine(_rootDirectory.FullName, @"Objects");
			Directory.CreateDirectory(objectsPath);
			// Adapters
			var adaptersPath = Path.Combine(_rootDirectory.FullName, @"Adapters");
			Directory.CreateDirectory(adaptersPath);
			// Helpers
			var helpersPath = Path.Combine(_rootDirectory.FullName, @"Helpers");
			Directory.CreateDirectory(helpersPath);
			// Data
			var dataPath = Path.Combine(_rootDirectory.FullName, @"Data");
			Directory.CreateDirectory(dataPath);

			var buffer = new StringBuilder();

			var tables = schema.Tables;
			foreach (var table in tables)
			{
				var clrClass = DbTableConverter.ToClrClass(table, tables);

				buffer.Clear();
				buffer.AppendLine(GetObjectUsings(clrClass));
				buffer.AppendLine(string.Format(@"namespace {0}.{1}", schema.Name, Path.GetFileName(objectsPath)));
				buffer.AppendLine(@"{");
				buffer.AppendLine(ObjectGenerator.GenerateCode(clrClass, table.IsReadOnly));
				buffer.AppendLine(@"}");
				File.WriteAllText(Path.Combine(objectsPath, clrClass.Name) + @".cs", buffer.ToString());

				buffer.Clear();
				buffer.AppendLine(GetAdapterUsings(schema.Name));
				buffer.AppendLine(string.Format(@"namespace {0}.{1}", schema.Name, Path.GetFileName(adaptersPath)));
				buffer.AppendLine(@"{");
				buffer.AppendLine(AdapterGenerator.GenerateCode(clrClass, table, schema));
				buffer.AppendLine(@"}");
				File.WriteAllText(Path.Combine(adaptersPath, table.Name + @"Adapter") + @".cs", buffer.ToString());

				if (table.IsReadOnly)
				{
					buffer.Clear();
					buffer.AppendLine(GetHelperUsings(schema.Name));
					buffer.AppendLine(string.Format(@"namespace {0}.{1}", schema.Name, Path.GetFileName(helpersPath)));
					buffer.AppendLine(@"{");
					buffer.AppendLine(HelperGenerator.GenerateCode(table));
					buffer.AppendLine(@"}");

					File.WriteAllText(Path.Combine(helpersPath, clrClass.Name + @"Helper") + @".cs", buffer.ToString());
				}
			}

			buffer.Clear();
			buffer.AppendLine(GetAppUsings(schema.Name));
			buffer.AppendLine(string.Format(@"namespace {0}", schema.Name));
			buffer.AppendLine(@"{");
			buffer.AppendLine(AppGenerator.GenerateCode(schema));
			buffer.AppendLine(@"}");
			File.WriteAllText(Path.Combine(_rootDirectory.FullName, string.Format(@"{0}App.cs", schema.Name)), buffer.ToString());

			buffer.Clear();
			buffer.AppendLine(GetSystemUsings());
			buffer.AppendLine(string.Format(@"namespace {0}", schema.Name));
			buffer.AppendLine(@"{");
			buffer.AppendLine(AppGenerator.GenerateEventArgsClass());
			buffer.AppendLine(@"}");
			File.WriteAllText(Path.Combine(_rootDirectory.FullName, @"HelperLoadedEventArgs.cs"), buffer.ToString());

			buffer.Clear();
			buffer.AppendLine(GetDataUsings());
			buffer.AppendLine(string.Format(@"namespace {0}.{1}", schema.Name, Path.GetFileName(dataPath)));
			buffer.AppendLine(@"{");
			buffer.AppendLine(QueryHelperGenerator.GetCode());
			buffer.AppendLine(@"}");
			File.WriteAllText(Path.Combine(dataPath, @"QueryHelper.cs"), buffer.ToString());
		}

		private string GetAppUsings(string name)
		{
			var buffer = new StringBuilder();

			buffer.AppendLine(@"using System;");
			buffer.AppendLine(string.Format(@"using {0}.Helpers;", name));
			buffer.AppendLine(string.Format(@"using {0}.Adapters;", name));

			return buffer.ToString();
		}

		private string GetSystemUsings()
		{
			return @"using System;";
		}

		private string GetObjectUsings(ClrClass clrClass)
		{
			var buffer = new StringBuilder();

			var addSystem = false;
			var addGenerics = false;
			foreach (var property in clrClass.Properties)
			{
				var type = property.Type;
				if (type.CheckValue)
				{
					addSystem = true;
				}
				if (type.IsCollection)
				{
					addGenerics = true;
				}
			}

			if (addSystem)
			{
				buffer.AppendLine(@"using System;");
			}
			if (addGenerics)
			{
				buffer.AppendLine(@"using System.Collections.Generic;");
			}

			return buffer.ToString();
		}

		private string GetAdapterUsings(string name)
		{
			var buffer = new StringBuilder();

			buffer.AppendLine(@"using System;");
			buffer.AppendLine(@"using System.Collections.Generic;");
			buffer.AppendLine(@"using System.Data;");
			buffer.AppendLine(string.Format(@"using {0}.Data;", name));
			buffer.AppendLine(string.Format(@"using {0}.Objects;", name));

			return buffer.ToString();
		}

		private string GetDataUsings()
		{
			var buffer = new StringBuilder();

			buffer.AppendLine(@"using System;");
			buffer.AppendLine(@"using System.Collections.Generic;");
			buffer.AppendLine(@"using System.Data;");

			return buffer.ToString();
		}

		private string GetHelperUsings(string name)
		{
			var buffer = new StringBuilder();

			buffer.AppendLine(@"using System;");
			buffer.AppendLine(@"using System.Collections.Generic;");
			buffer.AppendLine(string.Format(@"using {0}.Objects;", name));
			buffer.AppendLine(string.Format(@"using {0}.Adapters;", name));

			return buffer.ToString();
		}
	}
}

