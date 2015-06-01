using System;
using System.Collections.Generic;
using System.Data;
using AppBuilder.Db.DDL;

namespace Demo
{
	public sealed class IFsa
	{
		public DbSchema Create()
		{
			var articleTypes = DbTable.ReadOnly(@"ArticleTypes", new[]
			                                                     {
				                                                     DbColumn.PrimaryKey(),
				                                                     DbColumn.String(@"Name"),
			                                                     });
			var brands = DbTable.ReadOnly(@"Brands", new[]
			                                         {
				                                         DbColumn.PrimaryKey(),
				                                         DbColumn.String(@"Name"),
			                                         });

			var flavours = DbTable.ReadOnly(@"Flavours", new[]
			                                             {
				                                             DbColumn.PrimaryKey(),
				                                             DbColumn.String(@"Name"),
			                                             });

			var articles = DbTable.ReadOnly(@"Articles", new[]
			                                             {
				                                             DbColumn.PrimaryKey(),
				                                             DbColumn.String(@"Name"),
				                                             DbColumn.ForeignKey(articleTypes),
				                                             DbColumn.ForeignKey(brands),
				                                             DbColumn.ForeignKey(flavours),
				                                             DbColumn.Decimal(@"Price"),
			                                             });

			var deliveryLocation = DbTable.ReadOnly(@"DeliveryLocations", new[]
			                                                              {
				                                                              DbColumn.PrimaryKey(),
				                                                              DbColumn.String(@"Name"),
			                                                              });
			var outlets = DbTable.ReadOnly(@"Outlets", new[]
			                                           {
				                                           DbColumn.PrimaryKey(),
				                                           DbColumn.String(@"Name"),
				                                           DbColumn.String(@"Address"),
				                                           DbColumn.String(@"City"),
				                                           DbColumn.String(@"Street"),
				                                           DbColumn.ForeignKey(deliveryLocation),
			                                           });

			var users = DbTable.ReadOnly(@"Users", new[]
			                                       {
				                                       DbColumn.PrimaryKey(),
				                                       DbColumn.String(@"LoginName"),
				                                       DbColumn.String(@"FullName"),
			                                       });

			var visits = DbTable.Normal(@"Visits", new[]
			                                       {
													   DbColumn.PrimaryKey(),
													   DbColumn.DateTime(@"Date"),
													   DbColumn.ForeignKey(outlets),
													   DbColumn.ForeignKey(users),
			                                       });

			var activityTypes = DbTable.ReadOnly(@"ActivityTypes", new[]
			                                                        {
				                                                        DbColumn.PrimaryKey(),
				                                                        DbColumn.String(@"Name"),
				                                                        DbColumn.String(@"Code"),
			                                                        });

			var activities = DbTable.Normal(@"Activities", new[]
			                                               {
															   DbColumn.PrimaryKey(),
															   DbColumn.ForeignKey(activityTypes),
															   DbColumn.ForeignKey(visits),
															   DbColumn.DateTime(@"ValidFrom"),
															   DbColumn.DateTime(@"ValidTo"),
			                                               }, @"Activity");

			var calendarDays = DbTable.Normal(@"CalendarDays", new[]
			                                                        {
				                                                        DbColumn.PrimaryKey(),
				                                                        DbColumn.DateTime(@"VisitDate"),
				                                                        DbColumn.Integer(@"Status"),
				                                                        DbColumn.ForeignKey(users),
			                                                        });

			var logs = DbTable.Normal(@"LogMessages", new[]
			                                                        {
				                                                        DbColumn.PrimaryKey(),
				                                                        DbColumn.DateTime(@"Time"),
				                                                        DbColumn.String(@"Type"),
				                                                        DbColumn.String(@"Message"),
			                                                        });

			return new DbSchema(@"iFSA", new[]
			           {
						   articleTypes,
						   brands,
						   flavours,
						   articles,
						   deliveryLocation,
						   outlets,
						   users,						   
						   activityTypes,
						   activities,
						   visits,
						   calendarDays,
						   logs
			           });
		}
	}

	public sealed class ArticleType
	{
		public long Id { get; private set; }
		public string Name { get; private set; }

		public ArticleType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Brand
	{
		public long Id { get; private set; }
		public string Name { get; private set; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Flavour
	{
		public long Id { get; private set; }
		public string Name { get; private set; }

		public Flavour(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Article
	{
		public long Id { get; private set; }
		public string Name { get; private set; }
		public ArticleType ArticleType { get; private set; }
		public Brand Brand { get; private set; }
		public Flavour Flavour { get; private set; }
		public decimal Price { get; private set; }

		public Article(long id, string name, ArticleType articleType, Brand brand, Flavour flavour, decimal price)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (articleType == null) throw new ArgumentNullException("articleType");
			if (brand == null) throw new ArgumentNullException("brand");
			if (flavour == null) throw new ArgumentNullException("flavour");

			this.Id = id;
			this.Name = name;
			this.ArticleType = articleType;
			this.Brand = brand;
			this.Flavour = flavour;
			this.Price = price;
		}
	}

	public sealed class DeliveryLocation
	{
		public long Id { get; private set; }
		public string Name { get; private set; }

		public DeliveryLocation(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Outlet
	{
		public long Id { get; private set; }
		public string Name { get; private set; }
		public string Address { get; private set; }
		public string City { get; private set; }
		public string Street { get; private set; }
		public DeliveryLocation DeliveryLocation { get; private set; }

		public Outlet(long id, string name, string address, string city, string street, DeliveryLocation deliveryLocation)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (address == null) throw new ArgumentNullException("address");
			if (city == null) throw new ArgumentNullException("city");
			if (street == null) throw new ArgumentNullException("street");
			if (deliveryLocation == null) throw new ArgumentNullException("deliveryLocation");

			this.Id = id;
			this.Name = name;
			this.Address = address;
			this.City = city;
			this.Street = street;
			this.DeliveryLocation = deliveryLocation;
		}
	}

	public sealed class User
	{
		public long Id { get; private set; }
		public string LoginName { get; private set; }
		public string FullName { get; private set; }

		public User(long id, string loginName, string fullName)
		{
			if (loginName == null) throw new ArgumentNullException("loginName");
			if (fullName == null) throw new ArgumentNullException("fullName");

			this.Id = id;
			this.LoginName = loginName;
			this.FullName = fullName;
		}
	}

	public sealed class ActivityType
	{
		public long Id { get; private set; }
		public string Name { get; private set; }
		public string Code { get; private set; }

		public ActivityType(long id, string name, string code)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (code == null) throw new ArgumentNullException("code");

			this.Id = id;
			this.Name = name;
			this.Code = code;
		}
	}

	public sealed class Activity
	{
		public long Id { get; set; }
		public ActivityType ActivityType { get; private set; }
		public Visit Visit { get; private set; }
		public DateTime ValidFrom { get; private set; }
		public DateTime ValidTo { get; private set; }

		public Activity(long id, ActivityType activityType, Visit visit, DateTime validFrom, DateTime validTo)
		{
			if (activityType == null) throw new ArgumentNullException("activityType");
			if (visit == null) throw new ArgumentNullException("visit");

			this.Id = id;
			this.ActivityType = activityType;
			this.Visit = visit;
			this.ValidFrom = validFrom;
			this.ValidTo = validTo;
		}
	}

	public sealed class Visit
	{
		public long Id { get; set; }
		public DateTime Date { get; private set; }
		public Outlet Outlet { get; private set; }
		public User User { get; private set; }
		public List<Activity> Activities { get; private set; }

		public Visit(long id, DateTime date, Outlet outlet, User user, List<Activity> activities)
		{
			if (outlet == null) throw new ArgumentNullException("outlet");
			if (user == null) throw new ArgumentNullException("user");
			if (activities == null) throw new ArgumentNullException("activities");

			this.Id = id;
			this.Date = date;
			this.Outlet = outlet;
			this.User = user;
			this.Activities = activities;
		}
	}

	public sealed class CalendarDay
	{
		public long Id { get; set; }
		public DateTime VisitDate { get; private set; }
		public long Status { get; private set; }
		public User User { get; private set; }

		public CalendarDay(long id, DateTime visitDate, long status, User user)
		{
			if (user == null) throw new ArgumentNullException("user");

			this.Id = id;
			this.VisitDate = visitDate;
			this.Status = status;
			this.User = user;
		}
	}

	public sealed class LogMessage
	{
		public long Id { get; set; }
		public DateTime Time { get; private set; }
		public string Type { get; private set; }
		public string Message { get; private set; }

		public LogMessage(long id, DateTime time, string type, string message)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (message == null) throw new ArgumentNullException("message");

			this.Id = id;
			this.Time = time;
			this.Type = type;
			this.Message = message;
		}
	}

	public sealed class ArticleTypesAdapter
	{
		public void Fill(Dictionary<long, ArticleType> articleTypes)
		{
			if (articleTypes == null) throw new ArgumentNullException("articleTypes");

			var query = @"SELECT Id, Name FROM ArticleTypes";

			QueryHelper.Fill(articleTypes, query, this.Creator, this.Selector);
		}

		private ArticleType Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}

			return new ArticleType(id, name);
		}

		private long Selector(ArticleType a) { return a.Id; }
	}

	public sealed class BrandsAdapter
	{
		public void Fill(Dictionary<long, Brand> brands)
		{
			if (brands == null) throw new ArgumentNullException("brands");

			var query = @"SELECT Id, Name FROM Brands";

			QueryHelper.Fill(brands, query, this.Creator, this.Selector);
		}

		private Brand Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}

			return new Brand(id, name);
		}

		private long Selector(Brand b) { return b.Id; }
	}


	public sealed class FlavoursAdapter
	{
		public void Fill(Dictionary<long, Flavour> flavours)
		{
			if (flavours == null) throw new ArgumentNullException("flavours");

			var query = @"SELECT Id, Name FROM Flavours";

			QueryHelper.Fill(flavours, query, this.Creator, this.Selector);
		}

		private Flavour Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}

			return new Flavour(id, name);
		}

		private long Selector(Flavour f) { return f.Id; }
	}


	public sealed class ArticlesAdapter
	{
		private readonly Dictionary<long, ArticleType> _articleTypes;
		private readonly Dictionary<long, Brand> _brands;
		private readonly Dictionary<long, Flavour> _flavours;

		public ArticlesAdapter(Dictionary<long, ArticleType> articleTypes, Dictionary<long, Brand> brands, Dictionary<long, Flavour> flavours)
		{
			if (articleTypes == null) throw new ArgumentNullException("articleTypes");
			if (brands == null) throw new ArgumentNullException("brands");
			if (flavours == null) throw new ArgumentNullException("flavours");

			_articleTypes = articleTypes;
			_brands = brands;
			_flavours = flavours;
		}

		public void Fill(Dictionary<long, Article> articles)
		{
			if (articles == null) throw new ArgumentNullException("articles");

			var query = @"SELECT Id, Name, ArticleTypeId, BrandId, FlavourId, Price FROM Articles";

			QueryHelper.Fill(articles, query, this.Creator, this.Selector);
		}

		private Article Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}
			var articleType = default(ArticleType);
			if (!r.IsDBNull(2))
			{
				articleType = _articleTypes[r.GetInt64(2)];
			}
			var brand = default(Brand);
			if (!r.IsDBNull(3))
			{
				brand = _brands[r.GetInt64(3)];
			}
			var flavour = default(Flavour);
			if (!r.IsDBNull(4))
			{
				flavour = _flavours[r.GetInt64(4)];
			}
			var price = 0M;
			if (!r.IsDBNull(5))
			{
				price = r.GetDecimal(5);
			}

			return new Article(id, name, articleType, brand, flavour, price);
		}

		private long Selector(Article a) { return a.Id; }
	}


	public sealed class DeliveryLocationsAdapter
	{
		public void Fill(Dictionary<long, DeliveryLocation> deliveryLocations)
		{
			if (deliveryLocations == null) throw new ArgumentNullException("deliveryLocations");

			var query = @"SELECT Id, Name FROM DeliveryLocations";

			QueryHelper.Fill(deliveryLocations, query, this.Creator, this.Selector);
		}

		private DeliveryLocation Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}

			return new DeliveryLocation(id, name);
		}

		private long Selector(DeliveryLocation d) { return d.Id; }
	}


	public sealed class OutletsAdapter
	{
		private readonly Dictionary<long, DeliveryLocation> _deliveryLocations;

		public OutletsAdapter(Dictionary<long, DeliveryLocation> deliveryLocations)
		{
			if (deliveryLocations == null) throw new ArgumentNullException("deliveryLocations");

			_deliveryLocations = deliveryLocations;
		}

		public void Fill(Dictionary<long, Outlet> outlets)
		{
			if (outlets == null) throw new ArgumentNullException("outlets");

			var query = @"SELECT Id, Name, Address, City, Street, DeliveryLocationId FROM Outlets";

			QueryHelper.Fill(outlets, query, this.Creator, this.Selector);
		}

		private Outlet Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}
			var address = string.Empty;
			if (!r.IsDBNull(2))
			{
				address = r.GetString(2);
			}
			var city = string.Empty;
			if (!r.IsDBNull(3))
			{
				city = r.GetString(3);
			}
			var street = string.Empty;
			if (!r.IsDBNull(4))
			{
				street = r.GetString(4);
			}
			var deliveryLocation = default(DeliveryLocation);
			if (!r.IsDBNull(5))
			{
				deliveryLocation = _deliveryLocations[r.GetInt64(5)];
			}

			return new Outlet(id, name, address, city, street, deliveryLocation);
		}

		private long Selector(Outlet o) { return o.Id; }
	}


	public sealed class UsersAdapter
	{
		public void Fill(Dictionary<long, User> users)
		{
			if (users == null) throw new ArgumentNullException("users");

			var query = @"SELECT Id, LoginName, FullName FROM Users";

			QueryHelper.Fill(users, query, this.Creator, this.Selector);
		}

		private User Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var loginName = string.Empty;
			if (!r.IsDBNull(1))
			{
				loginName = r.GetString(1);
			}
			var fullName = string.Empty;
			if (!r.IsDBNull(2))
			{
				fullName = r.GetString(2);
			}

			return new User(id, loginName, fullName);
		}

		private long Selector(User u) { return u.Id; }
	}


	public sealed class ActivityTypesAdapter
	{
		public void Fill(Dictionary<long, ActivityType> activityTypes)
		{
			if (activityTypes == null) throw new ArgumentNullException("activityTypes");

			var query = @"SELECT Id, Name, Code FROM ActivityTypes";

			QueryHelper.Fill(activityTypes, query, this.Creator, this.Selector);
		}

		private ActivityType Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var name = string.Empty;
			if (!r.IsDBNull(1))
			{
				name = r.GetString(1);
			}
			var code = string.Empty;
			if (!r.IsDBNull(2))
			{
				code = r.GetString(2);
			}

			return new ActivityType(id, name, code);
		}

		private long Selector(ActivityType a) { return a.Id; }
	}


	public sealed class ActivitiesAdapter
	{
		private readonly Dictionary<long, ActivityType> _activityTypes;

		public ActivitiesAdapter(Dictionary<long, ActivityType> activityTypes)
		{
			if (activityTypes == null) throw new ArgumentNullException("activityTypes");

			_activityTypes = activityTypes;
		}

		public Activity Creator(IDataReader r, Visit visit)
		{
			if (r == null) throw new ArgumentNullException("r");
			if (visit == null) throw new ArgumentNullException("visit");

			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var activityType = default(ActivityType);
			if (!r.IsDBNull(1))
			{
				activityType = _activityTypes[r.GetInt64(1)];
			}
			var validFrom = DateTime.MinValue;
			if (!r.IsDBNull(2))
			{
				validFrom = r.GetDateTime(2);
			}
			var validTo = DateTime.MinValue;
			if (!r.IsDBNull(3))
			{
				validTo = r.GetDateTime(3);
			}

			return new Activity(id, activityType, visit, validFrom, validTo);
		}

		public void Insert(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException("activity");

			var query = @"INSERT INTO Activities(ActivityTypeId, VisitId, ValidFrom, ValidTo) VALUES (@activityTypeId, @visitId, @validFrom, @validTo)";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@activityTypeId", activity.ActivityType.Id),
			QueryHelper.Parameter(@"@visitId", activity.Visit.Id),
			QueryHelper.Parameter(@"@validFrom", activity.ValidFrom),
			QueryHelper.Parameter(@"@validTo", activity.ValidTo),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
			activity.Id = Convert.ToInt64(QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()"));
		}

		public void Update(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException("activity");

			var query = @"UPDATE Activities SET ActivityTypeId = @activityTypeId, VisitId = @visitId, ValidFrom = @validFrom, ValidTo = @validTo WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@id", activity.Id),
			QueryHelper.Parameter(@"@activityTypeId", activity.ActivityType.Id),
			QueryHelper.Parameter(@"@visitId", activity.Visit.Id),
			QueryHelper.Parameter(@"@validFrom", activity.ValidFrom),
			QueryHelper.Parameter(@"@validTo", activity.ValidTo),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}

		public void Delete(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException("activity");

			var query = @"DELETE FROM Activities WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"Id", activity.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}
	}


	public sealed class VisitsAdapter
	{
		private readonly Dictionary<long, Outlet> _outlets;
		private readonly Dictionary<long, User> _users;
		private readonly ActivitiesAdapter _adapter;

		public VisitsAdapter(Dictionary<long, Outlet> outlets, Dictionary<long, User> users, ActivitiesAdapter adapter)
		{
			if (outlets == null) throw new ArgumentNullException("outlets");
			if (users == null) throw new ArgumentNullException("users");
			if (adapter == null) throw new ArgumentNullException("adapter");

			_outlets = outlets;
			_users = users;
			_adapter = adapter;
		}

		public List<Visit> GetAll()
		{
			var query = @"SELECT a.Id, a.ActivityTypeId, a.ValidFrom, a.ValidTo, v.Id, v.Date, v.OutletId, v.UserId FROM Visits v INNER JOIN Activities a ON v.Id = a.VisitId";

			return QueryHelper.Get(query, this.IdReader, this.Creator, _adapter.Creator, this.Attach);
		}

		private long IdReader(IDataReader r) { return r.GetInt64(0); }

		private void Attach(Visit v, Activity a) { v.Activities.Add(a); }

		private Visit Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(4))
			{
				id = r.GetInt64(4);
			}
			var date = DateTime.MinValue;
			if (!r.IsDBNull(5))
			{
				date = r.GetDateTime(5);
			}
			var outlet = default(Outlet);
			if (!r.IsDBNull(6))
			{
				outlet = _outlets[r.GetInt64(6)];
			}
			var user = default(User);
			if (!r.IsDBNull(7))
			{
				user = _users[r.GetInt64(7)];
			}

			return new Visit(id, date, outlet, user, new List<Activity>());
		}

		public void Insert(Visit visit)
		{
			if (visit == null) throw new ArgumentNullException("visit");

			var query = @"INSERT INTO Visits(Date, OutletId, UserId) VALUES (@date, @outletId, @userId)";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@date", visit.Date),
			QueryHelper.Parameter(@"@outletId", visit.Outlet.Id),
			QueryHelper.Parameter(@"@userId", visit.User.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
			visit.Id = Convert.ToInt64(QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()"));
		}

		public void Update(Visit visit)
		{
			if (visit == null) throw new ArgumentNullException("visit");

			var query = @"UPDATE Visits SET Date = @date, OutletId = @outletId, UserId = @userId WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@id", visit.Id),
			QueryHelper.Parameter(@"@date", visit.Date),
			QueryHelper.Parameter(@"@outletId", visit.Outlet.Id),
			QueryHelper.Parameter(@"@userId", visit.User.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}

		public void Delete(Visit visit)
		{
			if (visit == null) throw new ArgumentNullException("visit");

			var query = @"DELETE FROM Visits WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"Id", visit.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}
	}


	public sealed class CalendarDaysAdapter
	{
		private readonly Dictionary<long, User> _users;

		public CalendarDaysAdapter(Dictionary<long, User> users)
		{
			if (users == null) throw new ArgumentNullException("users");

			_users = users;
		}

		public List<CalendarDay> GetAll()
		{
			var query = @"SELECT Id, VisitDate, Status, UserId FROM CalendarDays";

			return QueryHelper.Get(query, this.Creator);
		}

		private CalendarDay Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var visitDate = DateTime.MinValue;
			if (!r.IsDBNull(1))
			{
				visitDate = r.GetDateTime(1);
			}
			var status = 0L;
			if (!r.IsDBNull(2))
			{
				status = r.GetInt64(2);
			}
			var user = default(User);
			if (!r.IsDBNull(3))
			{
				user = _users[r.GetInt64(3)];
			}

			return new CalendarDay(id, visitDate, status, user);
		}

		public void Insert(CalendarDay calendarDay)
		{
			if (calendarDay == null) throw new ArgumentNullException("calendarDay");

			var query = @"INSERT INTO CalendarDays(VisitDate, Status, UserId) VALUES (@visitDate, @status, @userId)";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@visitDate", calendarDay.VisitDate),
			QueryHelper.Parameter(@"@status", calendarDay.Status),
			QueryHelper.Parameter(@"@userId", calendarDay.User.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
			calendarDay.Id = Convert.ToInt64(QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()"));
		}

		public void Update(CalendarDay calendarDay)
		{
			if (calendarDay == null) throw new ArgumentNullException("calendarDay");

			var query = @"UPDATE CalendarDays SET VisitDate = @visitDate, Status = @status, UserId = @userId WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@id", calendarDay.Id),
			QueryHelper.Parameter(@"@visitDate", calendarDay.VisitDate),
			QueryHelper.Parameter(@"@status", calendarDay.Status),
			QueryHelper.Parameter(@"@userId", calendarDay.User.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}

		public void Delete(CalendarDay calendarDay)
		{
			if (calendarDay == null) throw new ArgumentNullException("calendarDay");

			var query = @"DELETE FROM CalendarDays WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"Id", calendarDay.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}
	}


	public sealed class LogMessagesAdapter
	{
		public List<LogMessage> GetAll()
		{
			var query = @"SELECT Id, Time, Type, Message FROM LogMessages";

			return QueryHelper.Get(query, this.Creator);
		}

		private LogMessage Creator(IDataReader r)
		{
			var id = 0L;
			if (!r.IsDBNull(0))
			{
				id = r.GetInt64(0);
			}
			var time = DateTime.MinValue;
			if (!r.IsDBNull(1))
			{
				time = r.GetDateTime(1);
			}
			var type = string.Empty;
			if (!r.IsDBNull(2))
			{
				type = r.GetString(2);
			}
			var message = string.Empty;
			if (!r.IsDBNull(3))
			{
				message = r.GetString(3);
			}

			return new LogMessage(id, time, type, message);
		}

		public void Insert(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			var query = @"INSERT INTO LogMessages(Time, Type, Message) VALUES (@time, @type, @message)";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@time", logMessage.Time),
			QueryHelper.Parameter(@"@type", logMessage.Type),
			QueryHelper.Parameter(@"@message", logMessage.Message),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
			logMessage.Id = Convert.ToInt64(QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()"));
		}

		public void Update(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			var query = @"UPDATE LogMessages SET Time = @time, Type = @type, Message = @message WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"@id", logMessage.Id),
			QueryHelper.Parameter(@"@time", logMessage.Time),
			QueryHelper.Parameter(@"@type", logMessage.Type),
			QueryHelper.Parameter(@"@message", logMessage.Message),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}

		public void Delete(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			var query = @"DELETE FROM LogMessages WHERE Id = @id";

			var sqlParams = new[]
		{
			QueryHelper.Parameter(@"Id", logMessage.Id),
		};

			QueryHelper.ExecuteQuery(query, sqlParams);
		}
	}


	public sealed class ArticleTypesHelper
	{
		private readonly Dictionary<long, ArticleType> _articleTypes = new Dictionary<long, ArticleType>();

		public Dictionary<long, ArticleType> ArticleTypes { get { return _articleTypes; } }

		public void Load(ArticleTypesAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.ArticleTypes);
		}
	}


	public sealed class BrandsHelper
	{
		private readonly Dictionary<long, Brand> _brands = new Dictionary<long, Brand>();

		public Dictionary<long, Brand> Brands { get { return _brands; } }

		public void Load(BrandsAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.Brands);
		}
	}


	public sealed class FlavoursHelper
	{
		private readonly Dictionary<long, Flavour> _flavours = new Dictionary<long, Flavour>();

		public Dictionary<long, Flavour> Flavours { get { return _flavours; } }

		public void Load(FlavoursAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.Flavours);
		}
	}


	public sealed class ArticlesHelper
	{
		private readonly Dictionary<long, Article> _articles = new Dictionary<long, Article>();

		public Dictionary<long, Article> Articles { get { return _articles; } }

		public void Load(ArticlesAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.Articles);
		}
	}


	public sealed class DeliveryLocationsHelper
	{
		private readonly Dictionary<long, DeliveryLocation> _deliveryLocations = new Dictionary<long, DeliveryLocation>();

		public Dictionary<long, DeliveryLocation> DeliveryLocations { get { return _deliveryLocations; } }

		public void Load(DeliveryLocationsAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.DeliveryLocations);
		}
	}


	public sealed class OutletsHelper
	{
		private readonly Dictionary<long, Outlet> _outlets = new Dictionary<long, Outlet>();

		public Dictionary<long, Outlet> Outlets { get { return _outlets; } }

		public void Load(OutletsAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.Outlets);
		}
	}


	public sealed class UsersHelper
	{
		private readonly Dictionary<long, User> _users = new Dictionary<long, User>();

		public Dictionary<long, User> Users { get { return _users; } }

		public void Load(UsersAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.Users);
		}
	}


	public sealed class ActivityTypesHelper
	{
		private readonly Dictionary<long, ActivityType> _activityTypes = new Dictionary<long, ActivityType>();

		public Dictionary<long, ActivityType> ActivityTypes { get { return _activityTypes; } }

		public void Load(ActivityTypesAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			adapter.Fill(this.ActivityTypes);
		}
	}



}