using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    public class IndexURL : TableEntity
    {
        public IndexURL(string url, string keyword, string title, string date, int index)
        {
            this.PartitionKey = keyword;
            this.RowKey = url;

            this.Title = title;
            this.Date = date;
            this.Index = index;
        }

        public IndexURL() { }

        public string Title { get; set; }
        
        public string Date { get; set; }

        public int Index { get; set; }
    }
}
 