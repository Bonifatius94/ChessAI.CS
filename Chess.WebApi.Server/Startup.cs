using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chess.WebApi.Server
{
    /// <summary>
    /// Contains a startup configuration for a self-hosted .net core web api service.
    /// </summary>
    public class Startup
    {
        #region Constructor

        /// <summary>
        /// Create a new startup configuration instance with the given configuration parameters.
        /// </summary>
        /// <param name="configuration">The configuration parameters for the new instance.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The configuration parameters.
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion Members

        #region Methods

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// Apply configuration settings to the given services.
        /// </summary>
        /// <param name="services">The services to be configured.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()

                // the fallback .net core version
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)

                // required for using app.UseMvc()
                .AddMvcOptions(x => x.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Apply configuration settings to the given app and its hosting environment.
        /// </summary>
        /// <param name="app">The app to be configured.</param>
        /// <param name="env">The hosting environment to be configured.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // handle error responses on server exception
            #if DEBUG
                app.UseDeveloperExceptionPage();
            #else
                app.UseHsts();
            #endif

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        #endregion Methods
    }
}
