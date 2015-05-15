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
        public IndexURL(string url, string title)
        {
            this.PartitionKey = url;
            this.RowKey = Guid.NewGuid().ToString();

            this.Title = title;
        }

        public IndexURL() { }

        public string Title { get; set; }
    }
}
