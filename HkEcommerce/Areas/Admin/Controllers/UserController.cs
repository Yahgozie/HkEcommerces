using HkEcommerce.Data;
using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Model;
using HkEcommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HkEcommerce.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	public class UserController : Controller
	{
		//Below we use a logic to work directly with the ApplicationDbContext instead of using Interface IUnit as it is used in other controllers
		private readonly ApplicationDbContext _db;

		public UserController(ApplicationDbContext db)
		{
			_db = db;
		}

		public IActionResult Index()
		{
			return View();
		}

		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			var userList = _db.ApplicationUsers.Include(u => u.Company).ToList();//displays all the list of users
			var userRole = _db.UserRoles.ToList();//displays the list of users and their roles
			var roles = _db.Roles.ToList();//displays all the list of roles available
			foreach (var user in userList)
			{
				var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
				user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
				//This will initialize the name of an empty company rather than displaying error
				if (user.Company == null)
				{
					user.Company = new Company()
					{
						Name = ""
					};
				}
			}
			return Json(new { data = userList });
		}

		[HttpPost]
		public IActionResult LockUnlock([FromBody] string id)
		{
			var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
			if (objFromDb == null)
			{
				return Json(new { success = false, message = "Error while Locking/Unlocking" });
			}
			if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
			{
				//User is currently locked we will unlock them
				objFromDb.LockoutEnd = DateTime.Now;
			}
			else
			{
				//This will be used to lock the user
				objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
			}
			_db.SaveChanges();
			return Json(new { success = true, message = "Operation Successful." });
		}

		//[HttpDelete]
		//public IActionResult Delete(int id)
		//{
		//	var objFromDb = _unitOfWork.Category.Get(id);
		//	if (objFromDb == null)
		//	{
		//		return Json(new { success = false, message = "Error while deleting" });
		//	}
		//	_unitOfWork.Category.Remove(objFromDb);
		//	_unitOfWork.Save();
		//	return Json(new { success = true, message = "Delete Successful" });

		//}

		#endregion
	}
}
