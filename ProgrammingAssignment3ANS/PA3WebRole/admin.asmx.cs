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

        public admin()
        {
            manager = new StorageManager();
            crawler = WebCrawler.getInstance();
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
            Debug.WriteLine(crawler.getStatus());

            // CPU utilization %
            // RAM available
            // # URL's crawled
            // Last 10 URL's crawled

            // Size of queue (number of URL's to be crawled)
            manager.getQueueSize(manager.getUrlQueue());

            // Size of index (table storage with crawled data)
            // Any error URL's

            return null; 
        }
    }
}
