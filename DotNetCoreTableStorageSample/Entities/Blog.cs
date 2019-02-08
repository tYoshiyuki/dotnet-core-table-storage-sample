using Microsoft.WindowsAzure.Storage.Table;

namespace DotNetCoreTableStorageSample.Entities
{
    public class Blog : TableEntity
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
    }
}
