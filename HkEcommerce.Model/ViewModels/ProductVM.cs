using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.Model.ViewModels
{
	public class ProductVM
	{
		public Product Product { get; set; }
		public IEnumerable<SelectListItem> CategoryList { get; set; }
		//The SelectLIstItems enables to directly use the dropdown of models in view.
		public IEnumerable<SelectListItem> CoverTypeList { get; set; }
	}
}
