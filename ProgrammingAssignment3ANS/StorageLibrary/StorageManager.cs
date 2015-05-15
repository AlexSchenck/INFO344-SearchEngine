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

        private static CloudStorageAccount storageAccount;
        private static CloudTable urlTable; // Table with indexed urls with page titles
        private static CloudQueue urlQueue; // Queue with urls yet to be indexed
        private static CloudQueue commandQueue; // Queue with commands for worker role

        public StorageManager()
        {
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            urlTable = tableClient.GetTableReference("urltable");
            urlTable.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            urlQueue = queueClient.GetQueueReference("urlqueue");
            urlQueue.CreateIfNotExists();

            commandQueue = queueClient.GetQueueReference("commandqueue");
            commandQueue.CreateIfNotExists();
        }

        public CloudTable getUrlTable()
        {
            return urlTable;
        }

        public CloudQueue getUrlQueue()
        {
            return urlQueue;
        }

        public CloudQueue getCommandQueue()
        {
            return commandQueue;
        }

        public int getTableSize(CloudTable table)
        {
            return -1;
        }

        public int getQueueSize(CloudQueue queue)
        {
            queue.FetchAttributes();
            return (int) queue.ApproximateMessageCount;
        }
    }
}
