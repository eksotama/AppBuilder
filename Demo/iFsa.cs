using System;
using System.Collections.Generic;

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




}