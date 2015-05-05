using System;

namespace AppBuilder
{
	public abstract class AdapterBase
	{
		private readonly QueryHelper _queryHelper;

		protected QueryHelper QueryHelper
		{
			get { return _queryHelper; }
		}

		protected AdapterBase(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException("queryHelper");

			_queryHelper = queryHelper;
		}
	}
}