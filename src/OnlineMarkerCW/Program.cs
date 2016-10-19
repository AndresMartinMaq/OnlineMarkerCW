using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OnlineMarkerCW
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //this allows to pass env setttings from the command line as dotnet run --environment "Development"
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            var config = builder.Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config) //enables the use of commnand line vars
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
