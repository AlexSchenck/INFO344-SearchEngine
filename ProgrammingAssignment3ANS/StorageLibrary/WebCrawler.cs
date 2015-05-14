using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA3WorkerRole
{
    class WebCrawler
    {
        private static string IDLE = "Idle";
        private static string LOADING = "Loading";
        private static string CRAWLING = "Crawling";

        private string status;

        public WebCrawler()
        {
            status = IDLE;
        }

        public string getStatus()
        {
            return status;
        }
    }
}
