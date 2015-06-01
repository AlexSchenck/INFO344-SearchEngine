using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorageLibrary;

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
        public static string ERROR_NON_VALID_WEBSITE = "Non-Valid Website";
        public static int CPU_COUNTER = 1;
        public static int RAM_COUNTER = 2;
        public static string CNN_ROBOTS = "http://www.cnn.com/robots.txt";
        public static string BLEACHER_REPORT_ROBOTS = "http://bleacherreport.com/robots.txt";
        public static string BLOB_NAME = "ansblob";
        public static string BLOB_FILE_NAME = "WikipediaTitles";

        private static CloudStorageAccount storageAccount;
        private static CloudTable urlTable; // Table with indexed urls with page titles
        private static CloudTable statusTable; // Table with current status of crawler(s)
        private static CloudTable errorTable; // Table containing list of error urls with specific reasons
        private static CloudTable duplicatesTable; // Table containing current number of duplicate url's crawled
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

            duplicatesTable = tableClient.GetTableReference("duplicatestable");
            duplicatesTable.CreateIfNotExists();

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

        public CloudTable GetDuplicatesTable()
        {
            return duplicatesTable;
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

            List<IndexURL> entities = new List<IndexURL>();

            foreach (IndexURL iu in urlTable.ExecuteQuery(query))
            {
                entities.Add(iu);
            }

            entities.OrderBy(x => x.Index);

            if (entities.Count == 0)
                return 0;
            else
                return entities[0].Index;
        }

        public int GetNumberOfDuplicates()
        {
            TableQuery<DuplicateItem> query = new TableQuery<DuplicateItem>();
            int result = 0;

            foreach (DuplicateItem di in duplicatesTable.ExecuteQuery(query))
            {
                result =  di.Count;
            }

            return result;
        }

        public void IncrementDuplicates()
        {
            DuplicateItem newDup = new DuplicateItem(GetNumberOfDuplicates() + 1);
            TableOperation to = TableOperation.InsertOrReplace(newDup);
            duplicatesTable.Execute(to);
        }

        public void ClearIndex()
        {
            urlTable.DeleteIfExists();
            errorTable.DeleteIfExists();
        }

        public int GetTotalUrlsCrawled()
        {
            int indexNum = GetIndexSize();
            TableQuery<ErrorItem> query = new TableQuery<ErrorItem>();
            int errorNum = errorTable.ExecuteQuery(query).Count();
            int dupNum = GetNumberOfDuplicates();

            return indexNum + errorNum + dupNum;
        }

        public int GetQueueSize(CloudQueue queue)
        {
            queue.FetchAttributes();
            return (int) queue.ApproximateMessageCount;
        }

        public List<string> GetRecentUrls()
        {
            List<string> result = new List<string>();
            int indexSize = GetIndexSize();
            TableQuery<IndexURL> query = new TableQuery<IndexURL>()
                .Where(TableQuery.GenerateFilterConditionForInt("Index", QueryComparisons.GreaterThan, indexSize - 10));

            foreach (IndexURL iu in urlTable.ExecuteQuery(query))
            {
                result.Add(Encoding.UTF8.GetString(Convert.FromBase64String(iu.RowKey)));
            }

            return result;
        }

        public List<string> GetErrors()
        {
            List<string> result = new List<string>();
            TableQuery<ErrorItem> query = new TableQuery<ErrorItem>().Take(30);

            foreach (ErrorItem ei in errorTable.ExecuteQuery(query))
            {
                result.Add(ei.URL);
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
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, url));

            return urlTable.ExecuteQuery(query).Count() > 0;
        }

        public List<ResultTuple> SearchIndex(string[] query)
        {
            List<IndexURL> entities = new List<IndexURL>();

            for (int i = 0; i < query.Length; i++)
            {
                TableQuery<IndexURL> tableQuery = new TableQuery<IndexURL>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, query[i]));

                foreach (IndexURL iu in urlTable.ExecuteQuery(tableQuery))
                {
                    entities.Add(iu);
                }
            }

            var result =  entities
                .GroupBy(x => x.RowKey)
                .Select(x => new Tuple<string, int, string, string>(Encoding.UTF8.GetString(Convert.FromBase64String(x.Key)), x.ToList().Count, x.ToList()[0].Title, x.ToList()[0].Date))
                .OrderByDescending(x => x.Item2)
                .ThenByDescending(x => x.Item4)
                .ToList();
                //.Take(100);

            List<ResultTuple> resultTuples = new List<ResultTuple>();

            for (int i = 0; i < result.Count; i++)
            {
                resultTuples.Add(new ResultTuple(result[i]));
            }

            return resultTuples;
        }
    }
}
