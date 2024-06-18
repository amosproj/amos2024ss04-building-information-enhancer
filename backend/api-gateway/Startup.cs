using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpClient();

        // Add CORS services to allow specific origins
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                builder =>
                {
                    builder.WithOrigins(
                        "http://localhost",
                        "http://localhost:80", // Common HTTP frontend port
                        "http://localhost:443", // Common HTTPS frontend port 
                        "http://localhost:3000", // Common React development server port
                        "http://localhost:5173", // Vite development server port
                        "http://localhost:8080", // Other common development port
                        "http://localhost:8081", // Another common port
                        "http://localhost:4200", // Angular default development port
                        "http://localhost:5000", // Common port for some setups
                        "http://localhost:5500", // Common port for live server
                        "http://test.amos.b-ci.de",
                        "http://prod.amos.b-ci.de",
                        "http://test.amos.b-ci.de:80",
                        "http://test.amos.b-ci.de:443",
                        "http://prod.amos.b-ci.de:80",
                        "http://prod.amos.b-ci.de:443"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
        });

        // Add Swagger services
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "BCI - API Gateway",
                Description = "An API Gateway for the BCI project."
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Enable middleware to serve generated Swagger as a JSON endpoint
        app.UseSwagger();

        // Enable middleware to serve Swagger UI
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
            c.RoutePrefix = string.Empty; // Set Swagger UI at the root
        });

        // Enable CORS
        app.UseCors("AllowSpecificOrigins");

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
