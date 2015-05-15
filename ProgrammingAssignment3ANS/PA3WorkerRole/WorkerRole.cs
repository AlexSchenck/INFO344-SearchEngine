using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using System.IO;
using StorageLibrary;
using System.Xml;

namespace PA3WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static StorageManager manager;
        private static WebCrawler crawler;

        public override void Run()
        {
            Trace.TraceInformation("PA3WorkerRole is running");

            manager = new StorageManager();
            crawler = WebCrawler.getInstance();

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
                // Read from command queue every 50ms
                CloudQueueMessage commandMessage = manager.getCommandQueue().GetMessage();

                // Non-empty message
                if (commandMessage != null)
                {
                    // If message is in an unrecognized format, do nothing
                    string[] fullCommand = commandMessage.AsString.Split(new char[]{' '});

                    // Start message
                    if (fullCommand[0].Equals(StorageManager.START_MESSAGE))
                    {
                        // Always remove from queue
                        manager.getCommandQueue().DeleteMessage(commandMessage);

                        // Crawler is idling, start crawl
                        if (crawler.getStatus().Equals(WebCrawler.IDLE))
                        {
                            crawler.setStatus(WebCrawler.LOADING);

                            // Read robots.txt file
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullCommand[1]);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            Stream resStream = response.GetResponseStream();

                            // Local list of robot.txt's XML files containing URL's to add to queue
                            List<string> xmlList = new List<string>();

                            using (StreamReader reader = new StreamReader(resStream))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    // Separate all line elements by spaces
                                    string[] elements = line.Split(new char[] {' '});
                                    for (int i = 0; i < elements.Length; i++)
                                    {
                                        if (elements[i].EndsWith(".xml"))
                                            xmlList.Add(elements[i]);
                                    }
                                }
                            }

                            // Finish loading
                            // If xml document is found (within last 2 months), add to xmlList
                            // If html page is found, add to queue
                            while (xmlList.Count != 0)
                            {
                                using (XmlReader xreader = XmlReader.Create(new StringReader(xmlList[0])))
                                {
                                    xreader.ReadToFollowing("loc");
                                    Debug.WriteLine("ANSANS " + xreader.ReadElementContentAsString());
                                }

                                xmlList.RemoveAt(0);
                            }
                        }
                    }
                    // Stop message
                    else if (fullCommand[0].Equals(StorageManager.STOP_MESSAGE))
                    {
                        String status = crawler.getStatus();

                        // Crawler is still loading, keep stop message pending
                        if (!status.Equals(WebCrawler.LOADING))
                        {
                            manager.getCommandQueue().DeleteMessage(commandMessage);

                            // Crawler is currently crawling
                            // Stop all crawling and clear url queue
                            if (status.Equals(WebCrawler.CRAWLING))
                            {

                            }
                        }
                    }
                }

                // No command message, continue crawling/loading/idling
                await Task.Delay(50);
            }
        }
    }
}
