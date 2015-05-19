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

        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        public admin()
        {
            manager = new StorageManager();

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        [WebMethod]
        public void StartCrawling(string url) 
        {
            manager.GetCommandQueue().AddMessage(
                new CloudQueueMessage(StorageManager.START_MESSAGE));
        }

        [WebMethod]
        public void StopCrawling() 
        {
            manager.GetCommandQueue().AddMessage(
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
            results.Add(manager.GetStatus());

            // CPU utilization %
            results.Add(cpuCounter.NextValue().ToString());

            // RAM available
            results.Add(ramCounter.NextValue().ToString());

            // # URL's crawled
            results.Add("" + manager.GetTotalUrlsCrawled());

            // Size of queue (number of URL's to be crawled)
            results.Add(manager.GetQueueSize(manager.GetUrlQueue()).ToString());

            // Size of index (table storage with crawled data)
            results.Add(manager.GetIndexSize().ToString());

            // Last 10 URL's crawled
            List<String> recent = manager.GetRecentUrls();
            foreach (string s in recent)
                results.Add(s);

            // Any error URL's

            return results; 
        }
    }
}
