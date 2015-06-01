using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using StorageLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
        private static Trie trie;
        private static PerformanceCounter memProcess;
        private string filePath;
        private static Dictionary<string, List<ResultTuple>> cachedResults = new Dictionary<string, List<ResultTuple>>();
        private static List<string> suggestionStats = new List<string>{ "0", "" }; // index 0 : Total titles, 1 : Last title

        public admin()
        {
            manager = new StorageManager();
            memProcess = new PerformanceCounter("Memory", "Available Mbytes");
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

            // Last 10 URL's crawled
            string sumString = "";
            foreach (string s in manager.GetRecentUrls())
                sumString += s + " ";
            results.Add(sumString);

            // Any error URL's
            string errorString = "";
            foreach (string e in manager.GetErrors())
            {
                errorString += e + " ";
            }
            results.Add(errorString);

            // Number of suggestion titles
            results.Add(suggestionStats[0]);

            // Last suggestion title
            results.Add(suggestionStats[1]);

            return results; 
        }

        [WebMethod]
        public List<ResultTuple> SearchQuery(string query)
        {
            query = query.ToLower();

            // If query is cached
            if (cachedResults.ContainsKey(query))
            {
                Debug.WriteLine(query + " is a cached query.");

                return cachedResults[query];
            }
            else
            {
                // Split query into lowercase words
                string[] keyWords = query.Split(new char[] { ' ' });

                // Convert each word
                for (int i = 0; i < keyWords.Length; i++)
                {
                    keyWords[i] = Convert.ToBase64String(Encoding.UTF8.GetBytes(keyWords[i]));
                }

                List<ResultTuple> result = manager.SearchIndex(keyWords);

                // Add to cache, clear cache if count will be over 100
                if (cachedResults.Count == 100)
                {
                    cachedResults.Clear();
                }

                cachedResults.Add(query, result);

                return result;
            }
        }

        [WebMethod]
        public List<String> searchTrie(string query)
        {
            if (trie != null)
            {
                List<string> results = trie.searchPrefix(query);

                return results;
            }
            else
            {
                return new List<String>();
            }
        }

        [WebMethod]
        private void buildTrie(string fileName)
        {
            trie = new Trie();

            using (StreamReader reader = new StreamReader(filePath + fileName))
            {
                String line;
                int check = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    //Checks memory every 1000 adds
                    //If under 50MB, stops building Trie
                    if (check % 1000 == 0)
                    {
                        if (memProcess.NextValue() < 50)
                        {
                            break;
                        }
                    }
                    
                    trie.addTitle(line);

                    long suggestionCount = long.Parse(suggestionStats[0]);
                    suggestionCount++;
                    suggestionStats[0] = "" + suggestionCount;
                    suggestionStats[1] = line;

                    check++;
                }
            }
        }

        [WebMethod]
        public void downloadWiki()
        {
            Debug.WriteLine("downloadWiki started!");
            string blobConnectionString = ConfigurationManager.AppSettings["BlobConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(StorageManager.BLOB_NAME);
            filePath = System.IO.Path.GetTempFileName();

            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    if (blob.Name.Equals(StorageManager.BLOB_FILE_NAME))
                    {
                        using (var fileStream = System.IO.File.OpenWrite(filePath + blob.Name))
                        {
                            blob.DownloadToStream(fileStream);
                            fileStream.Close();
                            Debug.WriteLine("downloadWiki finished!");
                            buildTrie(StorageManager.BLOB_FILE_NAME);
                        }
                    }
                }
            }
        }
    }
}
