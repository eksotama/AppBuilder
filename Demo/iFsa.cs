using System;
using System.Collections.Generic;
using System.Data;

namespace Demo
{
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

	public sealed class Visit
	{
		public long Id { get; private set; }
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
		public long Id { get; private set; }
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

	public sealed class VisitsAdapter
	{
		//SELECT Id, Date, OutletId, UserId FROM Visits
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
		//SELECT Id, ActivityTypeId, VisitId, ValidFrom, ValidTo FROM Activities
	}









}