using HkEcommerce.Data;
using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.DataAccess.Repository
{
	public class CompanyRepository : Repository<Company>, ICompanyRepository
	{
		private readonly ApplicationDbContext _db;
		public CompanyRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}
		public void Update(Company company)
		{
			_db.Update(company);
		}
	}
}
