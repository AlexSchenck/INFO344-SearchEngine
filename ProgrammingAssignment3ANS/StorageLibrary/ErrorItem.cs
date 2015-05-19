using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    public class ErrorItem : TableEntity
    {
        public ErrorItem(string url, string error)
        {
            this.PartitionKey = Guid.NewGuid().ToString();
            this.RowKey = Guid.NewGuid().ToString();

            this.URL = url;
            this.Error = error;
        }

        public ErrorItem() { }

        public string URL { get; set; }

        public string Error {get; set; }
    }
}
