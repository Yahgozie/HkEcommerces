using HkEcommerce.Data;
using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.DataAccess.Repository
{
	public class CategoryRepository : RepositoryAsync<Category>, ICategoryRepository
	{
		private readonly ApplicationDbContext _db;
		public CategoryRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}
		public void Update(Category category)
		{
			_db.Update(category);
		}
	}
}
