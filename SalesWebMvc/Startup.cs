﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SalesWebMvc.Models;
using SalesWebMvc.Services;
using SalesWebMvc.Data;

namespace SalesWebMvc
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
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});


			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddDbContext<SalesWebMvcContext>(options =>
					options.UseMySql(Configuration.GetConnectionString("SalesWebMvcContext"), builder =>
						builder.MigrationsAssembly("SalesWebMvc")));
			
			//Registro dos novos serviços no sistema de injenção de dependencia da aplicação
			//Escopo transacional

			services.AddScoped<SeedingService>();
			services.AddScoped<SellerService>();
			services.AddScoped<DepartmentService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, SeedingService seedingService)
		{
			#region Configuração de localização
			var enUS = new CultureInfo("en-US");
			var localizationOption = new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture(enUS),
				SupportedCultures = new List<CultureInfo> { enUS },
				SupportedUICultures = new List<CultureInfo> { enUS}
			};
			#endregion

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			//Se estiver no perfil de dev, popula o bd
				seedingService.Seed();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
