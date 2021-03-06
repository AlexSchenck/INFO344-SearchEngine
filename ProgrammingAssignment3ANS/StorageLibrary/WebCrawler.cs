﻿using HtmlAgilityPack;
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

                try
                {
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
                                else if (content.Contains("cnn.com") || content.Contains("bleacherreport"))
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
                }
                catch(System.Net.WebException)
                {
                    Debug.WriteLine("URL Error! " + xmlList[0] + " 404'd.");
                    ErrorItem error = new ErrorItem(xmlList[0], StorageManager.ERROR_404);
                    TableOperation to = TableOperation.Insert(error);
                    manager.GetErrorTable().Execute(to);
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
                // Remove any leading slashes
                while (url.StartsWith("/"))
                {
                    url = url.Substring(1);
                }
            
                try
                {
                    HtmlDocument htmlPage = new HtmlWeb().Load(url);

                    // Current page's title
                    var title = htmlPage.DocumentNode.SelectSingleNode("//head/title");
                    string pageTitle = title.InnerText;

                    if (pageTitle.Equals("Error"))
                    {
                        Debug.WriteLine("URL Error! " + url + " 404'd.");
                        ErrorItem error = new ErrorItem(url, StorageManager.ERROR_404);
                        TableOperation to = TableOperation.Insert(error);
                        manager.GetErrorTable().Execute(to);
                    }
                    else
                    {
                        string date = "";

                        // Tries getting the date, if not found, return blank string
                        try
                        {
                            var mod = htmlPage.DocumentNode.SelectSingleNode(".//meta[@name='lastmod']");
                            string modDateNode = mod.OuterHtml;
                            date = modDateNode.Split(new char[] { '"' })[1];
                        }
                        catch (System.NullReferenceException) { }

                        int index = manager.GetIndexSize();

                        // Removes " - Cnn.com"
                        pageTitle = pageTitle.Split(new string[] {" - CNN.com"}, StringSplitOptions.None)[0];

                        // Removes " | Bleacher Report"
                        pageTitle = pageTitle.Split(new string[] {" | Bleacher Report"}, StringSplitOptions.None)[0];

                        // Separates into keywords
                        String[] keywords = pageTitle.Split(new char[] { ' ' });

                        string convertedURL = Convert.ToBase64String(Encoding.UTF8.GetBytes(url));

                        // Index the URL for every word in the title
                        foreach (string s in keywords)
                        {
                            try
                            {
                                // Remove punctuation
                                String temp = new string(s.ToCharArray().Where(c => !char.IsPunctuation(c)).ToArray());
                                    
                                string convertedS = Convert.ToBase64String(Encoding.UTF8.GetBytes(temp.ToLower()));
                                IndexURL newUrl = new IndexURL(convertedURL, convertedS, pageTitle, date, index);
                                TableOperation to = TableOperation.Insert(newUrl);
                                manager.GetUrlTable().Execute(to);
                            }
                            catch (Microsoft.WindowsAzure.Storage.StorageException) { }
                        }  

                        foreach (HtmlNode hn in htmlPage.DocumentNode.SelectNodes("//a"))
                        {
                            string href = hn.GetAttributeValue("href", string.Empty);
                            if (!String.IsNullOrEmpty(href))
                            {
                                if (href.Contains("cnn.com") || href.Contains("bleacherreport"))
                                {
                                    CloudQueueMessage cqm = new CloudQueueMessage(href);
                                    manager.GetUrlQueue().AddMessage(cqm);
                                }
                            }
                        }
                    }
                }
                catch(System.UriFormatException)
                {
                    Debug.WriteLine("URL Error! " + url + " is invalid.");
                    ErrorItem error = new ErrorItem(url, StorageManager.ERROR_NON_VALID_WEBSITE);
                    TableOperation to = TableOperation.Insert(error);
                    manager.GetErrorTable().Execute(to);
                }
            }
         
            manager.SetStatus(StorageManager.STATUS_IDLE);
        }
    }
}
