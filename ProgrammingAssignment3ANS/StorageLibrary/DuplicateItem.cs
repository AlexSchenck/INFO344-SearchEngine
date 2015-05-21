using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLibrary
{
    public class DuplicateItem : TableEntity
    {
        public DuplicateItem(int count)
        {
            this.PartitionKey = "" + 0;
            this.RowKey = "" + 0;

            this.Count = count;
        }

        public DuplicateItem() { }

        public int Count { get; set; }
    }
}
