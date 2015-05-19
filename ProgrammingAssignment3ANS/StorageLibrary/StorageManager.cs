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
        public static string CNN_ROBOTS = "http://www.cnn.com/robots.txt";
        public static string BLEACHER_REPORT_ROBOTS = "http://bleacherreport.com/robots.txt";

        private static CloudStorageAccount storageAccount;
        private static CloudTable urlTable; // Table with indexed urls with page titles
        private static CloudTable statusTable; // Table with current status of crawler(s)
        private static CloudTable counterTable; // Table with current number of url's crawled
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

            counterTable = tableClient.GetTableReference("counterTable");
            counterTable.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            urlQueue = queueClient.GetQueueReference("urlqueue");
            urlQueue.CreateIfNotExists();

            commandQueue = queueClient.GetQueueReference("commandqueue");
            commandQueue.CreateIfNotExists();

            TableQuery<StatusItem> query = new TableQuery<StatusItem>();
            if (statusTable.ExecuteQuery(query).Count() == 0)
                SetStatus(StorageManager.STATUS_IDLE);

            TableQuery<UrlCounter> query2 = new TableQuery<UrlCounter>();
            if (counterTable.ExecuteQuery(query2).Count() == 0)
                AddOneToUrlCounter();
        }

        public CloudTable GetUrlTable()
        {
            return urlTable;
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

        public int GetTotalUrlsCrawled()
        {
            TableQuery<UrlCounter> query = new TableQuery<UrlCounter>().Take(1);

            int result = -1;

            foreach (UrlCounter uc in statusTable.ExecuteQuery(query))
            {
                result = uc.Counter;
            }

            return result;
        }

        public void AddOneToUrlCounter()
        {
            int n = GetTotalUrlsCrawled() + 1;
            UrlCounter newCounter = new UrlCounter(0, n);
            TableOperation to = TableOperation.InsertOrReplace(newCounter);
            counterTable.Execute(to);
        }

        public int GetQueueSize(CloudQueue queue)
        {
            queue.FetchAttributes();
            return (int) queue.ApproximateMessageCount;
        }

        public List<String> GetRecentUrls()
        {
            List<string> result = new List<string>();
            TableQuery<IndexURL> query = new TableQuery<IndexURL>().Take(10);

            foreach (IndexURL iu in urlTable.ExecuteQuery(query))
            {
                result.Add(iu.PartitionKey);
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
    }
}
