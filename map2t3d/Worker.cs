using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace map2t3d
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly T3dWriter _t3DWriter;
        private readonly ArgsConfig _args;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime, T3dWriter t3DWriter, IOptions<ArgsConfig> options)
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _t3DWriter = t3DWriter;

            _args = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            using var writer = new StreamWriter(Path.ChangeExtension(_args.FileName, "t3d"));
            _t3DWriter.Write(writer);

            _hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }
    }
}
