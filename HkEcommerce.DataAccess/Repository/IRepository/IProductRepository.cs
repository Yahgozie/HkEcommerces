using HkEcommerce.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.DataAccess.Repository.IRepository
{
	public interface IProductRepository : IRepository<Product>
	{
		void Update(Product product);
	}
}
