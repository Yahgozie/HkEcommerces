using HkEcommerce.Data;
using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.DataAccess.Repository
{
	public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
	{
		private readonly ApplicationDbContext _db;
		public OrderDetailsRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}
		public void Update(OrderDetails obj)
		{
			_db.Update(obj);
		}
	}
}
