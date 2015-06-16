using System;
using System.Collections.Generic;
using System.Data;
using AppBuilder.Db.DDL;

namespace Demo
{
	//public sealed class GitHub
	//{
	//	public DbSchema Create()
	//	{
	//		var severities = DbTable.ReadOnly(@"Severities", new[]
	//													   {
	//														   DbColumn.PrimaryKey(),
	//														   DbColumn.String(@"Name"),
	//														   DbColumn.Integer(@"Code"),
	//													   }, @"Severity");

	//		var statuses = DbTable.ReadOnly(@"Statuses", new[]
	//													 {
	//														 DbColumn.PrimaryKey(),
	//														 DbColumn.String(@"Name"),
	//													 }, @"Status");

	//		var users = DbTable.ReadOnly(@"Users", new[]
	//											 {
	//												 DbColumn.PrimaryKey(),
	//												 DbColumn.String(@"Name"),
	//											 });



	//		var projects = DbTable.Normal(@"Projects", new[]
	//												   {
	//													   DbColumn.PrimaryKey(),
	//													   DbColumn.String(@"Name"),
	//													   DbColumn.ForeignKey(users),
	//												   });

	//		var bugs = DbTable.Normal(@"Bugs", new[]
	//										   {
	//											   DbColumn.PrimaryKey(),
	//											   DbColumn.String(@"Title"),
	//											   DbColumn.String(@"Description"),
	//											   DbColumn.ForeignKey(projects),
	//											   DbColumn.ForeignKey(severities),
	//											   DbColumn.ForeignKey(statuses),
	//											   DbColumn.DateTime(@"CreatedAt"),
	//											   DbColumn.ForeignKey(users, false, @"CreatedById"),
	//											   DbColumn.DateTime(@"ResolvedAt", true),
	//											   DbColumn.ForeignKey(users, true, @"ResolvedById"),
	//										   });




	//		var tables = new[]
	//					 {
	//						 severities,
	//						 statuses,
	//						 users,
	//						 bugs,
	//						 projects,
	//					 };

	//		var schema = new DbSchema(@"GitHub", tables);
	//		return schema;
	//	}
	//}

	//public sealed class Severity
	//{
	//	public long Id { get; private set; }
	//	public string Name { get; private set; }
	//	public long Code { get; private set; }

	//	public Severity(long id, string name, long code)
	//	{
	//		if (name == null) throw new ArgumentNullException("name");

	//		this.Id = id;
	//		this.Name = name;
	//		this.Code = code;
	//	}
	//}

	//public sealed class Status
	//{
	//	public long Id { get; private set; }
	//	public string Name { get; private set; }

	//	public Status(long id, string name)
	//	{
	//		if (name == null) throw new ArgumentNullException("name");

	//		this.Id = id;
	//		this.Name = name;
	//	}
	//}

	//public sealed class GitHubUser
	//{
	//	public long Id { get; private set; }
	//	public string Name { get; private set; }

	//	public GitHubUser(long id, string name)
	//	{
	//		if (name == null) throw new ArgumentNullException("name");

	//		this.Id = id;
	//		this.Name = name;
	//	}
	//}

	//public sealed class Bug
	//{
	//	public long Id { get; set; }
	//	public string Title { get; private set; }
	//	public string Description { get; private set; }
	//	public Project Project { get; private set; }
	//	public Severity Severity { get; private set; }
	//	public Status Status { get; private set; }
	//	public DateTime CreatedAt { get; private set; }
	//	public GitHubUser CreatedBy { get; private set; }
	//	public DateTime ResolvedAt { get; private set; }
	//	public GitHubUser ResolvedBy { get; private set; }

	//	public Bug(long id, string title, string description, Project project, Severity severity, Status status, DateTime createdAt, GitHubUser createdBy, DateTime resolvedAt, GitHubUser resolvedBy)
	//	{
	//		if (title == null) throw new ArgumentNullException("title");
	//		if (description == null) throw new ArgumentNullException("description");
	//		if (project == null) throw new ArgumentNullException("project");
	//		if (severity == null) throw new ArgumentNullException("severity");
	//		if (status == null) throw new ArgumentNullException("status");
	//		if (createdBy == null) throw new ArgumentNullException("createdBy");

	//		this.Id = id;
	//		this.Title = title;
	//		this.Description = description;
	//		this.Project = project;
	//		this.Severity = severity;
	//		this.Status = status;
	//		this.CreatedAt = createdAt;
	//		this.CreatedBy = createdBy;
	//		this.ResolvedAt = resolvedAt;
	//		this.ResolvedBy = resolvedBy;
	//	}
	//}

	//public sealed class Project
	//{
	//	public long Id { get; set; }
	//	public string Name { get; private set; }
	//	public User User { get; private set; }
	//	public List<Bug> Bugs { get; private set; }

	//	public Project(long id, string name, User user, List<Bug> bugs)
	//	{
	//		if (name == null) throw new ArgumentNullException("name");
	//		if (user == null) throw new ArgumentNullException("user");
	//		if (bugs == null) throw new ArgumentNullException("bugs");

	//		this.Id = id;
	//		this.Name = name;
	//		this.User = user;
	//		this.Bugs = bugs;
	//	}
	//}


	//public sealed class SeveritiesAdapter
	//{
	//	public void Fill(Dictionary<long, Severity> severities)
	//	{
	//		if (severities == null) throw new ArgumentNullException("severities");

	//		var query = @"SELECT Id, Name, Code FROM Severities";

	//		QueryHelper.Fill(severities, query, this.Creator, this.Selector);
	//	}

	//	private Severity Creator(IDataReader r)
	//	{
	//		var id = 0L;
	//		if (!r.IsDBNull(0))
	//		{
	//			id = r.GetInt64(0);
	//		}
	//		var name = string.Empty;
	//		if (!r.IsDBNull(1))
	//		{
	//			name = r.GetString(1);
	//		}
	//		var code = 0L;
	//		if (!r.IsDBNull(2))
	//		{
	//			code = r.GetInt64(2);
	//		}

	//		return new Severity(id, name, code);
	//	}

	//	private long Selector(Severity s) { return s.Id; }
	//}


	//public sealed class StatusesAdapter
	//{
	//	public void Fill(Dictionary<long, Status> statuses)
	//	{
	//		if (statuses == null) throw new ArgumentNullException("statuses");

	//		var query = @"SELECT Id, Name FROM Statuses";

	//		QueryHelper.Fill(statuses, query, this.Creator, this.Selector);
	//	}

	//	private Status Creator(IDataReader r)
	//	{
	//		var id = 0L;
	//		if (!r.IsDBNull(0))
	//		{
	//			id = r.GetInt64(0);
	//		}
	//		var name = string.Empty;
	//		if (!r.IsDBNull(1))
	//		{
	//			name = r.GetString(1);
	//		}

	//		return new Status(id, name);
	//	}

	//	private long Selector(Status s) { return s.Id; }
	//}


	//public sealed class GitHubUsersAdapter
	//{
	//	public void Fill(Dictionary<long, GitHubUser> users)
	//	{
	//		if (users == null) throw new ArgumentNullException("users");

	//		var query = @"SELECT Id, Name FROM Users";

	//		QueryHelper.Fill(users, query, this.Creator, this.Selector);
	//	}

	//	private GitHubUser Creator(IDataReader r)
	//	{
	//		var id = 0L;
	//		if (!r.IsDBNull(0))
	//		{
	//			id = r.GetInt64(0);
	//		}
	//		var name = string.Empty;
	//		if (!r.IsDBNull(1))
	//		{
	//			name = r.GetString(1);
	//		}

	//		return new GitHubUser(id, name);
	//	}

	//	private long Selector(GitHubUser u) { return u.Id; }
	//}


	//public sealed class BugsAdapter
	//{
	//	private readonly Dictionary<long, Severity> _severities;
	//	private readonly Dictionary<long, Status> _statuses;
	//	private readonly Dictionary<long, GitHubUser> _users;

	//	public BugsAdapter(Dictionary<long, Severity> severities, Dictionary<long, Status> statuses, Dictionary<long, GitHubUser> users)
	//	{
	//		if (severities == null) throw new ArgumentNullException("severities");
	//		if (statuses == null) throw new ArgumentNullException("statuses");
	//		if (users == null) throw new ArgumentNullException("users");

	//		_severities = severities;
	//		_statuses = statuses;
	//		_users = users;
	//	}

	//	public Bug Creator(IDataReader r, Project project)
	//	{
	//		if (r == null) throw new ArgumentNullException("r");
	//		if (project == null) throw new ArgumentNullException("project");

	//		var id = 0L;
	//		if (!r.IsDBNull(0))
	//		{
	//			id = r.GetInt64(0);
	//		}
	//		var title = string.Empty;
	//		if (!r.IsDBNull(1))
	//		{
	//			title = r.GetString(1);
	//		}
	//		var description = string.Empty;
	//		if (!r.IsDBNull(2))
	//		{
	//			description = r.GetString(2);
	//		}
	//		var severity = default(Severity);
	//		if (!r.IsDBNull(3))
	//		{
	//			severity = _severities[r.GetInt64(3)];
	//		}
	//		var status = default(Status);
	//		if (!r.IsDBNull(4))
	//		{
	//			status = _statuses[r.GetInt64(4)];
	//		}
	//		var createdAt = DateTime.MinValue;
	//		if (!r.IsDBNull(5))
	//		{
	//			createdAt = r.GetDateTime(5);
	//		}
	//		var createdBy = default(GitHubUser);
	//		if (!r.IsDBNull(6))
	//		{
	//			createdBy = _users[r.GetInt64(6)];
	//		}
	//		var resolvedAt = DateTime.MinValue;
	//		if (!r.IsDBNull(7))
	//		{
	//			resolvedAt = r.GetDateTime(7);
	//		}
	//		var resolvedBy = default(GitHubUser);
	//		if (!r.IsDBNull(8))
	//		{
	//			resolvedBy = _users[r.GetInt64(8)];
	//		}

	//		return new Bug(id, title, description, project, severity, status, createdAt, createdBy, resolvedAt, resolvedBy);
	//	}

	//	public void Insert(Bug bug)
	//	{
	//		if (bug == null) throw new ArgumentNullException("bug");

	//		var query = @"INSERT INTO Bugs(Title, Description, ProjectId, SeverityId, StatusId, CreatedAt, CreatedById, ResolvedAt, ResolvedById) VALUES (@title, @description, @projectId, @severityId, @statusId, @createdAt, @createdById, @resolvedAt, @resolvedById)";

	//		var sqlParams = new[]
	//	{
	//		QueryHelper.Parameter(@"@title", bug.Title),
	//		QueryHelper.Parameter(@"@description", bug.Description),
	//		QueryHelper.Parameter(@"@projectId", bug.Project.Id),
	//		QueryHelper.Parameter(@"@severityId", bug.Severity.Id),
	//		QueryHelper.Parameter(@"@statusId", bug.Status.Id),
	//		QueryHelper.Parameter(@"@createdAt", bug.CreatedAt),
	//		QueryHelper.Parameter(@"@createdById", bug.CreatedBy.Id),
	//		QueryHelper.Parameter(@"@resolvedAt", bug.ResolvedAt),
	//		QueryHelper.Parameter(@"@resolvedById", bug.ResolvedBy.Id),
	//	};

	//		QueryHelper.ExecuteQuery(query, sqlParams);
	//		bug.Id = Convert.ToInt64(QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()"));
	//	}

	//	public void Update(Bug bug)
	//	{
	//		if (bug == null) throw new ArgumentNullException("bug");

	//		var query = @"UPDATE Bugs SET Title = @title, Description = @description, ProjectId = @projectId, SeverityId = @severityId, StatusId = @statusId, CreatedAt = @createdAt, CreatedById = @createdById, ResolvedAt = @resolvedAt, ResolvedById = @resolvedById WHERE Id = @id";

	//		var sqlParams = new[]
	//	{
	//		QueryHelper.Parameter(@"@id", bug.Id),
	//		QueryHelper.Parameter(@"@title", bug.Title),
	//		QueryHelper.Parameter(@"@description", bug.Description),
	//		QueryHelper.Parameter(@"@projectId", bug.Project.Id),
	//		QueryHelper.Parameter(@"@severityId", bug.Severity.Id),
	//		QueryHelper.Parameter(@"@statusId", bug.Status.Id),
	//		QueryHelper.Parameter(@"@createdAt", bug.CreatedAt),
	//		QueryHelper.Parameter(@"@createdById", bug.CreatedBy.Id),
	//		QueryHelper.Parameter(@"@resolvedAt", bug.ResolvedAt),
	//		QueryHelper.Parameter(@"@resolvedById", bug.ResolvedBy.Id),
	//	};

	//		QueryHelper.ExecuteQuery(query, sqlParams);
	//	}

	//	public void Delete(Bug bug)
	//	{
	//		if (bug == null) throw new ArgumentNullException("bug");

	//		var query = @"DELETE FROM Bugs WHERE Id = @id";

	//		var sqlParams = new[]
	//	{
	//		QueryHelper.Parameter(@"Id", bug.Id),
	//	};

	//		QueryHelper.ExecuteQuery(query, sqlParams);
	//	}
	//}


	//public sealed class ProjectsAdapter
	//{
	//	private readonly Dictionary<long, User> _users;
	//	private readonly BugsAdapter _adapter;

	//	public ProjectsAdapter(Dictionary<long, User> users, BugsAdapter adapter)
	//	{
	//		if (users == null) throw new ArgumentNullException("users");
	//		if (adapter == null) throw new ArgumentNullException("adapter");

	//		_users = users;
	//		_adapter = adapter;
	//	}

	//	public List<Project> GetAll()
	//	{
	//		var query = @"SELECT b.Id, b.Title, b.Description, b.SeverityId, b.StatusId, b.CreatedAt, b.CreatedById, b.ResolvedAt, b.ResolvedById, p.Id, p.Name, p.UserId FROM Projects p INNER JOIN Bugs b ON p.Id = b.ProjectId";

	//		return QueryHelper.Get(query, this.IdReader, this.Creator, _adapter.Creator, this.Attach);
	//	}

	//	private long IdReader(IDataReader r) { return r.GetInt64(0); }

	//	private void Attach(Project p, Bug b) { p.Bugs.Add(b); }

	//	private Project Creator(IDataReader r)
	//	{
	//		var id = 0L;
	//		if (!r.IsDBNull(9))
	//		{
	//			id = r.GetInt64(9);
	//		}
	//		var name = string.Empty;
	//		if (!r.IsDBNull(10))
	//		{
	//			name = r.GetString(10);
	//		}
	//		var user = default(User);
	//		if (!r.IsDBNull(11))
	//		{
	//			user = _users[r.GetInt64(11)];
	//		}

	//		return new Project(id, name, user, new List<Bug>());
	//	}

	//	public void Insert(Project project)
	//	{
	//		if (project == null) throw new ArgumentNullException("project");

	//		var query = @"INSERT INTO Projects(Name, UserId) VALUES (@name, @userId)";

	//		var sqlParams = new[]
	//	{
	//		QueryHelper.Parameter(@"@name", project.Name),
	//		QueryHelper.Parameter(@"@userId", project.User.Id),
	//	};

	//		QueryHelper.ExecuteQuery(query, sqlParams);
	//		project.Id = Convert.ToInt64(QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()"));
	//	}

	//	public void Update(Project project)
	//	{
	//		if (project == null) throw new ArgumentNullException("project");

	//		var query = @"UPDATE Projects SET Name = @name, UserId = @userId WHERE Id = @id";

	//		var sqlParams = new[]
	//	{
	//		QueryHelper.Parameter(@"@id", project.Id),
	//		QueryHelper.Parameter(@"@name", project.Name),
	//		QueryHelper.Parameter(@"@userId", project.User.Id),
	//	};

	//		QueryHelper.ExecuteQuery(query, sqlParams);
	//	}

	//	public void Delete(Project project)
	//	{
	//		if (project == null) throw new ArgumentNullException("project");

	//		var query = @"DELETE FROM Projects WHERE Id = @id";

	//		var sqlParams = new[]
	//	{
	//		QueryHelper.Parameter(@"Id", project.Id),
	//	};

	//		QueryHelper.ExecuteQuery(query, sqlParams);
	//	}
	//}







}