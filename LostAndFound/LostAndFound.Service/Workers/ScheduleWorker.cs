using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LostAndFound.Service.Workers
{
    public class ScheduleWorker : BackgroundService
    {
        private readonly IImageMatchingWorker _imageMatchingWorker;
        private readonly ILogger<ScheduleWorker> _logger;

        public ScheduleWorker(IImageMatchingWorker imageMatchingWorker, ILogger<ScheduleWorker> logger)
        {
            _imageMatchingWorker = imageMatchingWorker;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(60 * 1000);

            _logger.LogInformation("ScheduleWorker Started");

            await _imageMatchingWorker.DoWork(stoppingToken);
        }
    }
}
