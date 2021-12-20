using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private List<string> Urls = new List<string> { "https://www.google.com/" };
        private IHttpClientFactory httpClientFactory;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollUrls();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while polling urls");
                }

                finally
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async Task PollUrls()
        {
            List<Task> tasks = new List<Task>();
            foreach (var url in Urls)
            {
                tasks.Add(PollUrl(url));
            }

            Task.WhenAll(tasks);
        }

        private async Task PollUrl(string url)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("{Url} is online.", url);
                }

                else
                {
                    _logger.LogInformation("{Url} is offline.", url);
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Url} is offline", url);
                throw;
            }
        }
    }
}
