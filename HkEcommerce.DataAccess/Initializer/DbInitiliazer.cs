using HkEcommerce.Data;
using HkEcommerce.Model;
using HkEcommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HkEcommerce.DataAccess.Initializer
{
	public class DbInitiliazer : IDbInitializer
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public DbInitiliazer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public void Initialize()
		{
			try
			{
				if(_db.Database.GetPendingMigrations().Count() > 0)
				{
					_db.Database.Migrate();
				}
			}
			catch (Exception)
			{
				throw;
			}

			if (_db.Roles.Any(r => r.Name == SD.Role_Admin)) return;

			_roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
			_roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
			_roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();
			_roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();

			_userManager.CreateAsync(new ApplicationUser
			{
				UserName = "gozmegatron@gmail.com",
				Email = "gozmegatron@gmail.com",
				EmailConfirmed = true,
				Name = "Nwosu"
			}, "LookingForMe22$").GetAwaiter().GetResult();
			ApplicationUser user = _db.ApplicationUsers.Where(u => u.Email == "gozmegatron@gmail.com").FirstOrDefault();
			_userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
		}
	}
}
