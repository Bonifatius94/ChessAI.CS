using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chess.WebApi.Server
{
    /// <summary>
    /// Program containing the main function for running a self-hosted web api server.
    /// </summary>
    public class Program
    {
        #region Main

        /// <summary>
        /// Main function starting the web service.
        /// </summary>
        /// <param name="args">Commandline arguments put by the user via CLI.</param>
        public static void Main(string[] args)
        {
            // create a new webhost builder instance
            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

            // build a new web app context and start it
            builder.Build().Run();
        }

        #endregion Main
    }
}
