using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    public sealed class WebCrawler
    {
        public static string IDLE = "Idle";
        public static string LOADING = "Loading";
        public static string CRAWLING = "Crawling";

        private static WebCrawler instance;

        private string status;

        // WebCrawler -- singleton instance
        private WebCrawler()
        {
            status = IDLE;
        }

        public static WebCrawler getInstance()
        {
            if (instance == null)
            {
                instance = new WebCrawler();
            }

            return instance;
        }

        public string getStatus()
        {
            return status;
        }

        public void setStatus(string newStatus)
        {
            status = newStatus;
        }
    }
}
