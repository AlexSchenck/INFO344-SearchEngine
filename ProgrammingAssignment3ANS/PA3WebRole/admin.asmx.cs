using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using StorageLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace PA3WebRole
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class admin : System.Web.Services.WebService
    {
        private static StorageManager manager;
        private static WebCrawler crawler;

        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        public admin()
        {
            manager = new StorageManager();
            crawler = WebCrawler.getInstance();

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        [WebMethod]
        public void StartCrawling(string url) 
        {
            manager.getCommandQueue().AddMessage(
                new CloudQueueMessage(StorageManager.START_MESSAGE
                    + " " + url));
        }

        [WebMethod]
        public void StopCrawling() 
        {
            manager.getCommandQueue().AddMessage(
                new CloudQueueMessage(StorageManager.STOP_MESSAGE));
        }

        [WebMethod]
        public void ClearIndex() { }

        [WebMethod]
        public string GetPageTitle(string url) { return null; }

        [WebMethod]
        public List<string> GetStatus() 
        {
            List<string> results = new List<string>();
            
            // State of worker role
            results.Add(crawler.getStatus());

            // CPU utilization %
            results.Add(cpuCounter.NextValue().ToString());

            // RAM available
            results.Add(ramCounter.NextValue().ToString());

            // # URL's crawled
            results.Add(crawler.getNumberUrlsCrawled().ToString());

            // Last 10 URL's crawled
            Queue<String> recent = crawler.getRecentUrls();
            for (int i = 0; i < recent.Count; i++)
            {
                String temp = recent.Dequeue();
                results.Add(temp);
                recent.Enqueue(temp);
            }

            // Size of queue (number of URL's to be crawled)
            results.Add(manager.getQueueSize(manager.getUrlQueue()).ToString());

            // Size of index (table storage with crawled data)
            results.Add(manager.getTableSize(manager.getUrlTable()).ToString());

            // Any error URL's

            return null; 
        }
    }
}
