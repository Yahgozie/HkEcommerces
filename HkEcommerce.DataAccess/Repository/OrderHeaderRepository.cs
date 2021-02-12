using HkEcommerce.Data;
using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.DataAccess.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _db;
		public OrderHeaderRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}
		public void Update(OrderHeader obj)
		{
			_db.Update(obj);
		}
	}
}
