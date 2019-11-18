using Autofac;
using InternationalizationService.Config;
using InternationalizationService.Core;
using InternationalizationService.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using VPT.Shared.Poco.Enum.Accounts;
using VPT.Shared.Rest;
using AuthorizationMiddleware = InternationalizationService.Filters.AuthorizationMiddleware;

namespace InternationalizationService
{
    /// <summary>
    /// The <see cref="Startup"/> class.
    /// </summary>
    public class Startup
    {
        private static readonly ILogger Logger = Log.ForContext<Startup>();

        /// <summary>
        /// The application configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The constructor of <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterConfigurations(Configuration);

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddAuthentication("BearerToken")
                   .AddScheme<AuthenticationSchemeOptions, AuthorizationMiddleware>("BearerToken", null);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("EmployeeLogin", policy =>
                {
                    policy.AuthenticationSchemes.Add("BearerToken");
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new EmployeeLoginSystemRequirement(LoginSystem.EmployeeLogin));
                });
            });

            services.AddSingleton<IAuthorizationHandler, EmployeeLoginSystemHandler>();

            services.AddControllers()
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.InvalidModelStateResponseFactory = context =>
                     {
                         var requestPath = context.HttpContext.Request.Path;

                         var problemDetails = new ValidationProblemDetails(context.ModelState)
                         {
                             Title = "One or more model validation errors occurred.",
                             Status = StatusCodes.Status400BadRequest,
                             Detail = "See the errors property for details.",
                             Instance = requestPath
                         };

                         var errorData = JsonConvert.SerializeObject(problemDetails.Errors);
                         Logger.Error($"Error occurred validating model state on { requestPath} : { errorData}");

                         return new BadRequestObjectResult(problemDetails)
                         {
                             ContentTypes = { "application/problem+json" }
                         };
                     };
                 });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Internationalization Service",
                    Description = "A Web API Microservice for Internationalization",
                    Contact = new Contact
                    {
                        Name = "Vant4ge Engineering",
                        Email = "engineering@vant4ge.com",
                        Url = "https://www.vant4ge.com"
                    }
                });
                options.IncludeXmlComments($"{Program.WorkingDirectory}/RulesService.xml", true);
            });
        }

        /// <summary>
        /// Register dependency with Autofac
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<VTPSharedRestModule>();
            builder.RegisterModule(new RulesServiceCoreModule(Configuration));
            Logger.Debug("Autofac configuration complete");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The reference of <see cref="IApplicationBuilder"/> to cofigure the application's request pipeline.</param>
        /// <param name="env">The web hosting environment that application is running in.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Logger.Debug("Configure the HTTP request pipeline.");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Internationalization API");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Logger.Debug("Request pipeline configuration complete");
        }
    }
}
