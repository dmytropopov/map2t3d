using map2t3d.Config;
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
                    services.AddTransient<ObjReader>();
                    services.AddTransient<T3dWriter>();
                    services.AddHostedService<Worker>();

                    services.Configure<ArgsConfig>(conf => conf.FileName = args[0] );
                    services.Configure<FoldersOptions>(hostContext.Configuration.GetSection("FoldersConfig"));
                    services.Configure<TexturesConversionOptions>(hostContext.Configuration.GetSection("TexturesConversion"));
                });
    }
}
