using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace practice
{
    public class BackgroundWorkerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                System.Console.WriteLine(DateTime.Now);
                await Task.Delay(1000);
            }
        }
    }
}