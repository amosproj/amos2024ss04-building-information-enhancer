using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using BIE.Core.BaseRepository;
using BIE.Core.DBRepository;
using BIE.Core.Services;
using BIE.Data;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Parbat.Core.Services;

namespace BIE.Core.API
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// /Configuration property
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            InjectInstances(services);

            services.AddControllers()
                .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(Global.API_VER, new OpenApiInfo
                {
                    Title = "BIE.Core.API",
                    Version = Global.API_VER,
                    Contact = new OpenApiContact
                    {
                    },
                    License = new OpenApiLicense
                    {
                        Name = "The MIT License (MIT)",
                        Url = new Uri("http://opensource.org/licenses/mit-license.php"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(baseDirectory, xmlFile);
                Console.WriteLine(xmlPath);
                c.IncludeXmlComments(xmlPath);
            });



            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));


        }

        private static void InjectInstances(IServiceCollection services)
        {
            // Repository Factor
            services.AddTransient<IRepositoryFactory, DBRepositoryFactory>();
            //services.AddTransient<CourseService>();


            // Inject Services: get all classes implementing IService interface
            var assemb = typeof(InfrastructureService).Assembly;
            var iservices = assemb.GetTypes().
                Where(x => !x.IsAbstract && x.IsClass
                                    && x.GetInterfaces().Contains(typeof(IService)));

            foreach (var s in iservices)
                services.Add(new ServiceDescriptor(s, s, ServiceLifetime.Transient));

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("MyPolicy");
            app.MaintainCorsHeadersOnError();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/" + Global.API_VER + "/swagger.json",
                    "BIE.Core.API " + Global.API_VER));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            ConfigureEnviornmentVaraiables();
        }

        private static void ConfigureEnviornmentVaraiables()
        {
            var dbServer = "sql-db"; //Environment.GetEnvironmentVariable("DB_SERVER");
            var dbName = "BIEDB"; //Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = "sa";//Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = "MyPass@1234"; //Environment.GetEnvironmentVariable("DB_PASSWORD");
            bool dbTrusted = false; //Environment.GetEnvironmentVariable("TRUSTED").ToLower() == "true";
            DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),
                "SQL");//Environment.GetEnvironmentVariable("DB_TYPE"));

            Database.Instance.SetConnectionString(dbType, dbServer,
                dbName, dbUser, dbPassword, dbTrusted);
        }
    }
}
