using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetCoreTableStorageSample.Services
{
    public interface ITableStorageService<T> where T : TableEntity, new()
    {
        Task<List<T>> GetList();
        Task<List<T>> GetList(TableQuery<T> query);
        Task<List<T>> GetList(string partitionKey);
        Task<T> GetItem(string partitionKey, string rowKey);
        Task Insert(T item);
        Task Update(T item);
        Task Delete(string partitionKey, string rowKey);
    }

    public class TableStorageService<T> : ITableStorageService<T> where T : TableEntity, new()
    {
        public TableStorageService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<List<T>> GetList()
        {
            var table = await GetTableAsync();
            var query = new TableQuery<T>();
            var results = new List<T>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResults = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }

        public async Task<List<T>> GetList(TableQuery<T> query)
        {
            var table = await GetTableAsync();
            var results = new List<T>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResults = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }


        public async Task<List<T>> GetList(string partitionKey)
        {
            var table = await GetTableAsync();
            var query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var results = new List<T>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResults = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);
            } while (continuationToken != null);

            return results;
        }

        public async Task<T> GetItem(string partitionKey, string rowKey)
        {
            var table = await GetTableAsync();
            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);
            return (T)(dynamic)result.Result;
        }

        public async Task Insert(T item)
        {
            var table = await GetTableAsync();
            var operation = TableOperation.Insert(item);
            await table.ExecuteAsync(operation);
        }

        public async Task Update(T item)
        {
            var table = await GetTableAsync();
            var operation = TableOperation.InsertOrReplace(item);
            await table.ExecuteAsync(operation);
        }

        public async Task Delete(string partitionKey, string rowKey)
        {
            T item = await GetItem(partitionKey, rowKey);
            var table = await GetTableAsync();
            var operation = TableOperation.Delete(item);
            await table.ExecuteAsync(operation);
        }

        private readonly string connectionString;

        private async Task<CloudTable> GetTableAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(typeof(T).Name);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
