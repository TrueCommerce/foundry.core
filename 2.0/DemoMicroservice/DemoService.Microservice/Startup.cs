using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using DemoService.Core;
using DemoService.Core.Models;
using DemoService.Microservice.Controllers;
using DemoService.SQL;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Models;
using Foundry.Core.Shared.Security;
using Foundry.Core.Shared.Services.Exceptions;
using Foundry.Core.Shared.Services.OData;
using Foundry.Core.Shared.Services.OData.Help;
using Foundry.Shared.Crypt;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using AuthenticationTicket = Accellos.Platform.Security.Authentication.AuthenticationTicket;

namespace DemoService.Microservice
{
	/// <summary>Startup class</summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Startup
	{
		#region Properties
		/// <summary>Container configuration</summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public IConfiguration Configuration { get; }
		#endregion


		#region Constructor
		/// <summary>Startup container</summary>
		/// <param name="configuration">Container configuration</param>
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}
		#endregion


		#region public ConfigureServices
		/// <summary>Configures services</summary>
		/// <param name="services">Container services</param>
		// This method gets called by the runtime. Use this method to add services to the container.
		// ReSharper disable once UnusedMember.Global
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddAutoMapper(delegate (IMapperConfigurationExpression e)
			{
				//e.AddProfile<SQL.Mapping.MappingProfile>();
				e.CreateMissingTypeMaps = false;
			});

			DemoServiceDbContext.ConnectionString = Configuration[ConfigurationKeys.SQLConnection];

			services.AddDbContext<DemoServiceDbContext>(optionsBuilder => optionsBuilder
				.UseLazyLoadingProxies()
				.UseSqlServer(
					DemoServiceDbContext.ConnectionString,
					x => x.MigrationsHistoryTable(DemoServiceDbContext.SchemaTableName)));

			//services.AddCors();

			services.AddMvc(options =>
			{
				//loop on each OData formatter to find the one without a supported media type
				foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>()
					.Where(_ => _.SupportedMediaTypes.Count == 0))
				{
					// to comply with the media type specifications, I'm using the prs prefix, for personal usage
					outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
				}

				foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>()
					.Where(_ => _.SupportedMediaTypes.Count == 0))
				{
					// to comply with the media type specifications, I'm using the prs prefix, for personal usage
					inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
				}
			}).AddJsonOptions(opt =>
			{
				//				opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
				opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
			})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


			services.AddOData();

			#region Register OData help
			foreach (KeyValuePair<int, string> pair in ApiVersion.ApiVersions)
			{
				CoreODataDocumentFilter.ODataApiHelpVersions[pair.Key] =
					new ODataApiHelpVersion
					{
						ApiVersion = pair.Key,
						ODataRouteBase = pair.Value,
						ODataAssemblies = new List<Assembly>
						{
							Assembly.GetAssembly(typeof(Order)),
							Assembly.GetAssembly(typeof(ExportParameters)),
							Assembly.GetAssembly(typeof(OrdersController))
						}
					};
			}

			services.AddSwaggerGen(c =>
			{
				c.DocumentFilter<CoreODataDocumentFilter>();
				c.CustomSchemaIds(type => type.FullName);

				foreach (KeyValuePair<int, string> pair in ApiVersion.ApiVersions)
					c.SwaggerDoc($"v{pair.Key}",
						new Info
						{
							Version = $"v{pair.Key}",
							Title = "Demo Service API",
							Description = "TrueCommerce Demo Service API",
							TermsOfService = "None",
							Contact = new Contact { Name = "None", Email = "", Url = "http://truecommerce.com" }
						});

				// Set the comments path for the Swagger JSON and UI.
				var basePath = PlatformServices.Default.Application.ApplicationBasePath;
				var xmlPath = Path.Combine(basePath, Assembly.GetAssembly(typeof(ApiVersion)).GetName().Name + ".xml");
				c.IncludeXmlComments(xmlPath);
				//xmlPath = Path.Combine(basePath, Assembly.GetAssembly(typeof(ODataUtils)).GetName().Name + ".xml");
				//c.IncludeXmlComments(xmlPath);

				AuthenticationTicket ticket = new AuthenticationTicket
				{
					UniqueId = Guid.NewGuid(),
					UserId = SystemDefaults.SystemAdministratorUserId,
					UserPasswordHash = HashCrypt.ComputeHash("sa")
				};

				c.AddSecurityDefinition("Bearer", new ApiKeyScheme
				{
					Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
					Name = "Authorization",
					In = "header",
					Type = "apiKey"
				});

				c.AddSecurityDefinition("AuthenticationTicket", new ApiKeyScheme
				{
					Description = $"Serialized AuthenticationTicket (XML or JSon). Example: {JsonConvert.SerializeObject(ticket)}",
					Name = "AuthenticationTicket",
					In = "header",
					Type = "apiKey"
				});

				var security = new Dictionary<string, IEnumerable<string>>
				{
					{"AuthenticationTicket", new string[] { }},
					{"Bearer", new string[] { }}
				};
				c.AddSecurityRequirement(security);
			});
			#endregion
		}
		#endregion

		#region public Configure
		/// <summary>Configure container</summary>
		/// <param name="app">Application builder</param>
		/// <param name="env">Hosting environment</param>
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		// ReSharper disable once UnusedMember.Global
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
				.CreateScope())
			{
				DemoServiceDbContext context = serviceScope.ServiceProvider.GetService<DemoServiceDbContext>();

				context.Database.Migrate();
				context.CheckBasicIntegrity(Configuration).Wait();
			}

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

			//app.UseCors(e =>
			//{
			//	e.AllowAnyOrigin();
			//	e.AllowAnyHeader();
			//	e.AllowAnyMethod();
			//	e.AllowCredentials();
			//	e.Build();
			//});

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				foreach (int version in ApiVersion.ApiVersions.Keys)
					c.SwaggerEndpoint($"/swagger/v{version}/swagger.json", $"Security API V{version}");

				c.DocExpansion(DocExpansion.None);
				c.EnableFilter();
			});

			app.UseMiddleware<ErrorHandlingMiddleware>();
			app.UseMiddleware<AuthenticationHandler>();

			app.UseMvc(routeBuilder =>
			{
				routeBuilder.Count().Filter().OrderBy().Expand().Select().MaxTop(null);

				ODataUtils.ConfigureOData(
					"DemoServiceODataContext",
					routeBuilder,
					app.ApplicationServices,
					new List<ODataAssembly>
					{
						new ODataAssembly
						{
							Assembly = Assembly.GetAssembly(typeof(Order)),
							ServiceIds = new List<string> {"global"}
						},
						new ODataAssembly
						{
							Assembly = Assembly.GetAssembly(typeof(ExportParameters)),
							ServiceIds = new List<string> {"global"}
						},
						new ODataAssembly
						{
							Assembly = Assembly.GetAssembly(typeof(OrdersController)),
							ServiceIds = new List<string> {"global"}
						}
					},
					ApiVersion.ApiVersions,
					(version, builder) => { });

				routeBuilder.EnableDependencyInjection();
			});
		}
		#endregion
	}
}
