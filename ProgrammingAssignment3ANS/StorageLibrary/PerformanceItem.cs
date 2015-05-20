using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    public class PerformanceItem : TableEntity
    {
        public PerformanceItem(int counterId, string value)
        {
            this.PartitionKey = "" + counterId;
            this.RowKey = "" + 0;

            this.Value = value;
        }

        public PerformanceItem() { }

        public string Value { get; set; }
    }
}
