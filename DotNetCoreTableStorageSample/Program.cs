using DotNetCoreTableStorageSample.Entities;
using DotNetCoreTableStorageSample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTableStorageSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 設定ファイルの読み込み
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();

            // サービスの設定
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITableStorageService<Blog>>(factory => new TableStorageService<Blog>(configuration["ConnectionStrings:StorageConnection"]));
            var service = serviceCollection.BuildServiceProvider().GetService<ITableStorageService<Blog>>();

            // サンプル実装
            var blogs = await service.GetList();
            blogs.ForEach(async _ => await service.Delete(_.PartitionKey, _.RowKey));

            await service.Insert(new Blog { BlogId = 1, Author = "Tanaka", Name = "Taro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "1", Timestamp = DateTime.Now });
            await service.Insert(new Blog { BlogId = 2, Author = "Suzuki", Name = "Jiro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "2", Timestamp = DateTime.Now });
            await service.Insert(new Blog { BlogId = 3, Author = "Sato", Name = "Saburo", RowKey = Guid.NewGuid().ToString(), PartitionKey = "3", Timestamp = DateTime.Now });

            var query = new TableQuery<Blog>().Where(TableQuery.GenerateFilterCondition("Author", QueryComparisons.Equal, "Tanaka"));
            var blog = await service.GetList(query);

            Console.WriteLine($"Hello {blog.FirstOrDefault().Author}!");
        }
    }
}
