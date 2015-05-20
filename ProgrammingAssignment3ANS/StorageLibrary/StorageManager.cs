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
    public class StorageManager
    {
        public static string START_MESSAGE = "start";
        public static string STOP_MESSAGE = "stop";
        public static string STATUS_IDLE = "Idle";
        public static string STATUS_LOADING = "Loading";
        public static string STATUS_CRAWLING = "Crawling";
        public static string ERROR_DUPLICATE = "Duplicate URL";
        public static string ERROR_404 = "URL 404";
        public static int CPU_COUNTER = 1;
        public static int RAM_COUNTER = 2;
        public static string CNN_ROBOTS = "http://www.cnn.com/robots.txt";
        public static string BLEACHER_REPORT_ROBOTS = "http://bleacherreport.com/robots.txt";

        private static CloudStorageAccount storageAccount;
        private static CloudTable urlTable; // Table with indexed urls with page titles
        private static CloudTable statusTable; // Table with current status of crawler(s)
        private static CloudTable errorTable; // Table containing list of error urls with specific reasons
        private static CloudTable performanceTable; // Table containing worker role performance information
        private static CloudQueue urlQueue; // Queue with urls yet to be indexed
        private static CloudQueue commandQueue; // Queue with commands for worker role

        public StorageManager()
        {
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            urlTable = tableClient.GetTableReference("urltable");
            urlTable.CreateIfNotExists();

            statusTable = tableClient.GetTableReference("statustable");
            statusTable.CreateIfNotExists();

            errorTable = tableClient.GetTableReference("errortable");
            errorTable.CreateIfNotExists();

            performanceTable = tableClient.GetTableReference("performancetable");
            performanceTable.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            urlQueue = queueClient.GetQueueReference("urlqueue");
            urlQueue.CreateIfNotExists();

            commandQueue = queueClient.GetQueueReference("commandqueue");
            commandQueue.CreateIfNotExists();

            TableQuery<StatusItem> query = new TableQuery<StatusItem>();
            if (statusTable.ExecuteQuery(query).Count() == 0)
                SetStatus(StorageManager.STATUS_IDLE);
        }

        public CloudTable GetUrlTable()
        {
            return urlTable;
        }

        public CloudTable GetErrorTable()
        {
            return errorTable;
        }

        public CloudTable GetPerformanceTable()
        {
            return performanceTable;
        }

        public CloudQueue GetUrlQueue()
        {
            return urlQueue;
        }

        public CloudQueue GetCommandQueue()
        {
            return commandQueue;
        }

        public int GetIndexSize()
        {
            TableQuery<IndexURL> query = new TableQuery<IndexURL>();
            return urlTable.ExecuteQuery(query).Count();
        }

        public void ClearIndex()
        {
            urlTable.DeleteIfExists();
        }

        public string GetPageTitle(string url)
        {
            string result = "";

            TableQuery<IndexURL> tq = new TableQuery<IndexURL>()
                .Where(TableQuery.GenerateFilterCondition("URL", QueryComparisons.Equal, url));

            foreach (IndexURL iu in urlTable.ExecuteQuery(tq))
            {
                result = iu.Title;
            }

            return result;
        }

        public int GetTotalUrlsCrawled()
        {
            int indexNum = GetIndexSize();
            TableQuery<ErrorItem> query = new TableQuery<ErrorItem>();
            int errorNum = errorTable.ExecuteQuery(query).Count();

            return indexNum + errorNum;
        }

        public int GetQueueSize(CloudQueue queue)
        {
            queue.FetchAttributes();
            return (int) queue.ApproximateMessageCount;
        }

        public List<String> GetRecentUrls()
        {
            List<string> result = new List<string>();
            int indexSize = GetIndexSize();
            TableQuery<IndexURL> query = new TableQuery<IndexURL>()
                .Where(TableQuery.GenerateFilterConditionForInt("Index", QueryComparisons.GreaterThan, indexSize - 10));

            foreach (IndexURL iu in urlTable.ExecuteQuery(query))
            {
                result.Add(iu.URL);
            }

            return result;
        }

        public string GetStatus()
        {
            TableQuery<StatusItem> query = new TableQuery<StatusItem>().Take(1);
            
            String result = "";

            foreach (StatusItem si in statusTable.ExecuteQuery(query))
            {
                result = si.Status;
            }

            return result;
        }

        public void SetStatus(string newStatus)
        {
            StatusItem newStatusItem = new StatusItem(0, newStatus);
            TableOperation to = TableOperation.InsertOrReplace(newStatusItem);
            statusTable.Execute(to);
        }

        public string GetPerformanceCounter(int counterId)
        {
            TableQuery<PerformanceItem> query = new TableQuery<PerformanceItem>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "" + counterId));

            String result = "";

            foreach (PerformanceItem pi in performanceTable.ExecuteQuery(query))
            {
                result = pi.Value;
            }

            return result;
        }

        public bool ContainsIndexUrl(string url)
        {
            TableQuery<IndexURL> query = new TableQuery<IndexURL>()
                .Where(TableQuery.GenerateFilterCondition("URL", QueryComparisons.Equal, url));

            return urlTable.ExecuteQuery(query).Count() > 0;
        }
    }
}
