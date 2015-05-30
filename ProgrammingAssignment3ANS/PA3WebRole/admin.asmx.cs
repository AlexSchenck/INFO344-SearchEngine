using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using StorageLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public admin()
        {
            manager = new StorageManager();
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
        public void ClearIndex() 
        {
            manager.ClearIndex();
        }

        [WebMethod]
        public List<string> GetStatus() 
        {
            List<string> results = new List<string>();
            
            // State of worker role
            results.Add(manager.GetStatus());

            // CPU utilization %
            results.Add(manager.GetPerformanceCounter(StorageManager.CPU_COUNTER));

            // RAM available
            results.Add(manager.GetPerformanceCounter(StorageManager.RAM_COUNTER));

            // # URL's crawled
            results.Add("" + manager.GetTotalUrlsCrawled());

            // Size of queue (number of URL's to be crawled)
            results.Add(manager.GetQueueSize(manager.GetUrlQueue()).ToString());

            // Size of index (table storage with crawled data)
            results.Add(manager.GetIndexSize().ToString());

            return results; 
        }

        [WebMethod]
        public List<string> SearchQuery(string query)
        {
            // Split query into lowercase words
            string[] keyWords = query.ToLower().Split(new char[] { ' ' });

            // Convert each word
            for (int i = 0; i < keyWords.Length; i++)
            {
                keyWords[i] = Convert.ToBase64String(Encoding.UTF8.GetBytes(keyWords[i]));
            }

            return manager.SearchIndex(keyWords);
        }
    }
}
