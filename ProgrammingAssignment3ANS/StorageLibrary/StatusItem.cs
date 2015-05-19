using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    class StatusItem : TableEntity
    {
        public StatusItem(int crawlerID, string status)
        {
            this.PartitionKey = "" + crawlerID;
            this.RowKey = "" + 0;

            this.Status = status;
        }

        public StatusItem() { }

        public string Status { get; set; }
    }
}
