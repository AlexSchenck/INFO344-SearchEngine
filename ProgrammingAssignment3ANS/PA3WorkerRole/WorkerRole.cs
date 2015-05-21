using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using StorageLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PA3WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static StorageManager manager;
        private static WebCrawler crawler;

        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private int waitIteration;
        private static int performanceCheckInterval = 5;

        public override void Run()
        {
            Trace.TraceInformation("PA3WorkerRole is running");

            manager = new StorageManager();
            crawler = new WebCrawler();

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            waitIteration = 0;

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("PA3WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("PA3WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("PA3WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Store performance information every 500ms
                if (waitIteration == 0)
                {
                    string cpu = cpuCounter.NextValue().ToString();
                    string ram = ramCounter.NextValue().ToString();

                    PerformanceItem perf = new PerformanceItem(StorageManager.CPU_COUNTER, cpu);
                    TableOperation to = TableOperation.InsertOrReplace(perf);
                    manager.GetPerformanceTable().Execute(to);

                    PerformanceItem perf2 = new PerformanceItem(StorageManager.RAM_COUNTER, ram);
                    TableOperation to2 = TableOperation.InsertOrReplace(perf2);
                    manager.GetPerformanceTable().Execute(to2);
                }
                else if (waitIteration == performanceCheckInterval)
                {
                    waitIteration = 0;
                }
                else
                {
                    waitIteration++;
                }

                // Read from command queue every 50ms
                CloudQueueMessage commandMessage = manager.GetCommandQueue().GetMessage();

                // Non-empty message
                if (commandMessage != null)
                {
                    // Start message
                    if (commandMessage.AsString.Equals(StorageManager.START_MESSAGE))
                    {
                        // Always remove from queue
                        manager.GetCommandQueue().DeleteMessage(commandMessage);

                        // Crawler is idling, start crawl
                        if (manager.GetStatus() == StorageManager.STATUS_IDLE)
                        {
                            crawler.Load(manager, StorageManager.CNN_ROBOTS);
                            crawler.Load(manager, StorageManager.BLEACHER_REPORT_ROBOTS);
                        }
                    }
                    // Stop message
                    else if (commandMessage.AsString.Equals(StorageManager.STOP_MESSAGE))
                    {
                        // Crawler is still loading, keep stop message pending
                        if (manager.GetStatus() != StorageManager.STATUS_LOADING)
                        {
                            manager.GetCommandQueue().DeleteMessage(commandMessage);

                            // Crawler is currently crawling
                            // Stop all crawling and clear url queue
                            manager.GetUrlQueue().Clear();
                            manager.SetStatus(StorageManager.STATUS_IDLE);
                        }
                    }
                }
                // No pending command, crawler is not loading and url queue is not empty
                // Take one url and crawl
                else if (manager.GetQueueSize(manager.GetUrlQueue()) != 0)
                {
                    CloudQueue q = manager.GetUrlQueue();
                    CloudQueueMessage url = q.GetMessage();
                    q.DeleteMessage(url);
                    crawler.crawlURL(manager, url.AsString);
                }

                await Task.Delay(50);
            }
        }
    }
}
