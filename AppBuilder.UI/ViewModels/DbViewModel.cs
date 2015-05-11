using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using AppBuilder.Db;
using AppBuilder.UI.Data;

namespace AppBuilder.UI.ViewModels
{
	public abstract class ViewModel
	{

	}

	public sealed class DbViewModel : ViewModel
	{
		private readonly ObservableCollection<DbTableViewModel> _tables = new ObservableCollection<DbTableViewModel>();

		public ObservableCollection<DbTableViewModel> Tables
		{
			get { return _tables; }
		}

		public DbViewModel()
		{
			QueryHelper.Connection = new SqlConnection();
		}

		public void Load()
		{
			this.Tables.Clear();

			
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
			this.Tables.Add(new DbTableViewModel(new DbTable(@"A", new[] { new DbColumn("Name", DbColumnType.String, 40, false, true), })));
		}
	}

	public sealed class DbTableViewModel : ViewModel
	{
		public DbTable Table { get; private set; }

		public string Name { get { return this.Table.Name; } }

		public DbTableViewModel(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			this.Table = table;
		}
	}
}