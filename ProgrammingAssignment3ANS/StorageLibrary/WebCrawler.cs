using HtmlAgilityPack;
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
        private DateTime minimumDate;

        // WebCrawler -- singleton instance
        public WebCrawler()
        {
            minimumDate = new DateTime(2015, 4, 1);
        }

        public void Load(StorageManager manager, string robotTxt)
        {
            manager.SetStatus(StorageManager.STATUS_LOADING);

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
                        //CNN
                        if (robotTxt == StorageManager.CNN_ROBOTS)
                        {
                            if (elements[i].EndsWith(".xml"))
                            {
                                xmlList.Add(elements[i]);
                            }
                        }
                        //Bleacher Report
                        else if (robotTxt == StorageManager.BLEACHER_REPORT_ROBOTS)
                        {
                            if (elements[i].EndsWith("nba.xml"))
                            {
                                xmlList.Add(elements[i]);
                            }
                        }
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

                using (XmlReader xreader = XmlReader.Create(new StringReader(xml)))
                {
                    while (xreader.ReadToFollowing("loc"))
                    {
                        string content = xreader.ReadElementContentAsString();

                        DateTime urlDate;
                        int validDate = 0;

                        // Link has an associated last modified date
                        if (robotTxt != StorageManager.BLEACHER_REPORT_ROBOTS && xreader.ReadToFollowing("lastmod"))
                        {
                            urlDate = DateTime.Parse(xreader.ReadElementContentAsString());
                            validDate = DateTime.Compare(urlDate, minimumDate);
                        }

                        if (validDate >= 0)
                        {
                            // XML -- add to load list
                            if (content.EndsWith(".xml"))
                            {
                                xmlList.Add(content);
                                Debug.WriteLine("Loaded " + content);
                            }
                            // HTML -- add to crawl list
                            else if (content.Contains("cnn") || content.Contains("bleacherreport"))
                            {
                                manager.GetUrlQueue().AddMessage(new CloudQueueMessage(content));
                                Debug.WriteLine("Loaded " + content);
                            }
                        }
                        else
                        {
                            Debug.WriteLine(content + " did not pass date check");
                        }
                    }
                }

                xmlList.RemoveAt(0);
            }
            
            Debug.WriteLine("Loading complete!");
            manager.SetStatus(StorageManager.STATUS_IDLE);
        }

        public void crawlURL(StorageManager manager, string url)
        {
            manager.SetStatus(StorageManager.STATUS_CRAWLING);

            if (!url.Equals("/"))
            {
                if (!manager.ContainsIndexUrl(url))
                {
                    HtmlDocument htmlPage = new HtmlWeb().Load(url);
                    var title = htmlPage.DocumentNode.SelectSingleNode("//head/title");
                    string pageTitle = title.InnerText;
                    if (pageTitle.Equals("Error"))
                    {
                        ErrorItem error = new ErrorItem(url, StorageManager.ERROR_404);
                        TableOperation to = TableOperation.Insert(error);
                        manager.GetErrorTable().Execute(to);
                    }
                    else
                    {
                        string date = "";

                        try
                        {
                            var mod = htmlPage.DocumentNode.SelectSingleNode(".//meta[@name='lastmod']");
                            string modDateNode = mod.OuterHtml;
                            date = modDateNode.Split(new char[] { '"' })[1];
                        }
                        catch (System.NullReferenceException) { }

                        int index = manager.GetIndexSize();
                        IndexURL newUrl = new IndexURL(url, pageTitle, date, index);
                        TableOperation to = TableOperation.Insert(newUrl);
                        manager.GetUrlTable().Execute(to);

                        foreach (HtmlNode hn in htmlPage.DocumentNode.SelectNodes("//a"))
                        {
                            string href = hn.GetAttributeValue("href", string.Empty);
                            if (!String.IsNullOrEmpty(href) && (href.Contains("cnn.com") || href.Contains("bleacherreport")))
                            {
                                CloudQueueMessage cqm = new CloudQueueMessage(href);
                                manager.GetUrlQueue().AddMessage(cqm);
                            }
                        }
                    }
                }
                else
                {
                    // Duplicate URL, add to error table
                    Debug.WriteLine("URL Error! " + url + " is a duplicate.");
                    ErrorItem newError = new ErrorItem(url, StorageManager.ERROR_DUPLICATE);
                    TableOperation to = TableOperation.Insert(newError);
                    manager.GetErrorTable().Execute(to);
                }
            }

            manager.SetStatus(StorageManager.STATUS_IDLE);
        }
    }
}
