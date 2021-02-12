using HkEcommerce.Data;
using HkEcommerce.DataAccess.Initializer;
using HkEcommerce.DataAccess.Repository;
using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HkEcommerce
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					Configuration.GetConnectionString("DefaultConnection")));
			services.AddDatabaseDeveloperPageExceptionFilter();

			services.AddIdentity<IdentityUser, IdentityRole>()
				.AddDefaultUI()
				.AddDefaultTokenProviders()
				.AddEntityFrameworkStores<ApplicationDbContext>();
			services.AddSingleton<IEmailSender, EmailSender>();
			services.Configure<EmailOptions>(Configuration);
			services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
			services.Configure<TwilioSettings>(Configuration.GetSection("Twilio"));
			services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();			
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IDbInitializer, DbInitiliazer>();

			services.AddControllersWithViews();

			//Configuration facebook login
			services.AddAuthentication().AddFacebook(options =>
			{
				options.AppId = "400317041072815";
				options.AppSecret = "9828a33dd37de3c7e069f8431066dc07";
			});

			//Configuration google login
			services.AddAuthentication().AddGoogle(options =>
			{
				options.ClientId = "440420426041-nmup8fq5c16c4lmbuv4tvgovje910nhp.apps.googleusercontent.com";
				options.ClientSecret = "TxVrhyvPuC3PcEWKUuIfrqAL";
			});

			//Configuring the session method 
			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["Secretkey"];
			app.UseSession();

			app.UseAuthentication();
			app.UseAuthorization();
			dbInitializer.Initialize();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
				endpoints.MapRazorPages();
			});
		}
	}
}
