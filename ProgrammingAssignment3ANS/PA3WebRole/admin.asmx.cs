using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private static string cnnRobots = "http://www.cnn.com/robots.txt";

        private static CloudTable table;
        private static CloudQueue queue;

        public admin()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.AppSettings["StorageConnectionString"]);

            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            //table = tableClient.GetTableReference("urlTable");
            //table.CreateIfNotExists();

            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            //queue = queueClient.GetQueueReference("urlQueue");
            //queue.CreateIfNotExists();
        }

        [WebMethod]
        public void StartCrawling() { }

        [WebMethod]
        public void StopCrawling() { }

        [WebMethod]
        public void ClearIndex() { }

        [WebMethod]
        public string GetPageTitle(string url) { return null; }

        [WebMethod]
        public List<string> GetStatus() { return null; }
    }
}
