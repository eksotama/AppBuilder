using System;
using System.IO;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class Builder
	{
		public static void Generate(DbSchema schema, NameProvider nameProvider, DirectoryInfo outputDirectory)
		{
			if (schema == null) throw new ArgumentNullException("schema");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (outputDirectory == null) throw new ArgumentNullException("outputDirectory");

			var directory = outputDirectory.FullName;
			GenerateObjects(schema, nameProvider, new DirectoryInfo(Path.Combine(directory, @"Objects")));
			GenerateAdapters(schema, nameProvider, new DirectoryInfo(Path.Combine(directory, @"Adapters")));
		}

		private static void GenerateAdapters(DbSchema schema, NameProvider nameProvider, DirectoryInfo outputDirectory)
		{
			Delete(outputDirectory);

			// TODO  :!!!
		}

		private static void GenerateObjects(DbSchema schema, NameProvider nameProvider, DirectoryInfo outputDirectory)
		{
			Delete(outputDirectory);

			foreach (var table in schema.Tables)
			{
				var @class = DbTableConverter.ToClrClass(table, nameProvider, schema.Tables);
				var code = ClrCodeGenerator.GetClassCode(@class);
				var path = Path.Combine(outputDirectory.FullName, @class.Name) + @".cs";
				File.WriteAllText(path, code);
			}
		}

		private static void Delete(DirectoryInfo outputDirectory)
		{
			if (outputDirectory.Exists)
			{
				outputDirectory.Delete(true);
			}
			outputDirectory.Create();
		}
	}
}