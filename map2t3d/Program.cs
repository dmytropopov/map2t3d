using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace map2t3d
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configure =>
                {
                    //configure.
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ObjReader>();
                    services.AddSingleton<T3dWriter>();
                    services.AddHostedService<Worker>();

                    services.Configure<ArgsConfig>((conf) => conf.FileName = args[0] );
                });
    }
}
