using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    class UrlCounter : TableEntity
    {
        public UrlCounter(int counterID, int counter)
        {
            this.PartitionKey = "" + counterID;
            this.RowKey = "" + 0;

            this.Counter = counter;
        }

        public UrlCounter() { }

        public int Counter { get; set; }
    }
}
