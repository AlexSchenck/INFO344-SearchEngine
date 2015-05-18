using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StorageLibrary
{
    public class WebCrawler
    {
        public static string IDLE = "Idle";
        public static string LOADING = "Loading";
        public static string CRAWLING = "Crawling";

        public static WebCrawler instance;

        private string status;
        private DateTime minimumDate;
        private int urlsCrawled;

        private Queue<string> recentUrls;

        // WebCrawler -- singleton instance
        private WebCrawler()
        {
            status = IDLE;
            minimumDate = DateTime.Today.AddMonths(-2);
            urlsCrawled = 0;
            recentUrls = new Queue<string>();
        }

        public static WebCrawler GetInstance()
        {
            if (instance == null)
            {
                instance = new WebCrawler();
            }

            return instance;
        }

        public String GetStatus()
        {
            return status;
        }

        public int GetNumberUrlsCrawled()
        {
            return urlsCrawled;
        }

        public Queue<String> GetRecentUrls()
        {
            return recentUrls;
        }

        public void Load(StorageManager manager, string robotTxt)
        {
            status = LOADING;

            // Read robots.txt file
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(robotTxt);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            // Local list of robot.txt's XML files containing URL's to add to queue
            List<string> xmlList = new List<string>();

            using (StreamReader reader = new StreamReader(resStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Separate all line elements by spaces
                    string[] elements = line.Split(new char[] { ' ' });
                    for (int i = 0; i < elements.Length; i++)
                    {
                        if (elements[i].EndsWith(".xml"))
                            xmlList.Add(elements[i]);
                    }
                }
            }

            // Finish loading
            // If xml document is found (within last 2 months), add to xmlList
            // If html page is found, add to queue
            while (xmlList.Count != 0)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlList[0]);
                String xml = xDoc.InnerXml;

                DateTime urlDate;

                using (XmlReader xreader = XmlReader.Create(new StringReader(xml)))
                {
                    while (xreader.ReadToFollowing("loc"))
                    {
                        string content = xreader.ReadElementContentAsString();

                        //if (validDate >= 0)
                        //{
                        // XML -- add to load list
                        if (content.EndsWith(".xml"))
                        {
                            int validDate = 0;

                            if (xreader.ReadToFollowing("lastmod"))
                            {
                                urlDate = DateTime.Parse(xreader.ReadElementContentAsString());
                                validDate = DateTime.Compare(urlDate, minimumDate);
                            }

                            if (validDate >= 0)
                            {
                                xmlList.Add(content);
                                Debug.WriteLine("Added " + content + " to XML list");
                            }
                        }
                        // HTML -- add to crawl list
                        else if (content.Contains("cnn.com"))
                        {
                            //int validDate = 0;

                            // Check if link is more recent than 2 months
                            // If no last mod attribute exists, assume link is recent
                            if (xreader.ReadToFollowing("lastmod"))
                            //{
                                //urlDate = DateTime.Parse(xreader.ReadElementContentAsString());
                              //  validDate = DateTime.Compare(urlDate, minimumDate);
                            //}

                            //if (validDate >= 0)
                            //{
                                manager.getUrlQueue().AddMessage(new CloudQueueMessage(content));
                                Debug.WriteLine("Added " + content + " to HTML queue");
                            //}
                        }
                        //}
                    }
                }

                xmlList.RemoveAt(0);
            }

            Debug.WriteLine("Loading complete!");
            status = WebCrawler.IDLE;
        }

        public void crawlURL(string url)
        {
            status = WebCrawler.CRAWLING;
            urlsCrawled++;
            Debug.WriteLine(urlsCrawled + " " + url);
            status = WebCrawler.IDLE;
        }
    }
}
