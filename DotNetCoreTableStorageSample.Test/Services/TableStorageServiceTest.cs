using ChainingAssertion;
using DotNetCoreTableStorageSample.Entities;
using DotNetCoreTableStorageSample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using RimDev.Automation.StorageEmulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotNetCoreTableStorageSample.Test.Services
{
    public class TableStorageServiceTest : IDisposable
    {
        private AzureStorageEmulatorAutomation _emulator { get; }
        private ITableStorageService<Blog> _service { get; }

        /// <summary>
        /// Setup
        /// </summary>
        public TableStorageServiceTest()
        {
            _emulator = new AzureStorageEmulatorAutomation();
            _emulator.Start();

            if (!AzureStorageEmulatorAutomation.IsEmulatorRunning()) throw new Exception("Azure Storage Emulatorの起動に失敗しました");

            // 設定ファイルの読み込み
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // サービスの構成
            var serviceCollection = new ServiceCollection();
            var connectionStrings = configuration["ConnectionStrings:StorageConnection"];
            serviceCollection.AddSingleton<ITableStorageService<Blog>>(factory => new TableStorageService<Blog>(connectionStrings));
            _service = serviceCollection.BuildServiceProvider().GetService<ITableStorageService<Blog>>();
        }

        /// <summary>
        /// Teardown
        /// </summary>
        public void Dispose()
        {
            _emulator.Stop();
        }

        [Fact]
        public async Task GetList_取得件数が想定通り()
        {
            // Arrange
            await _service.DeleteTable();
            await _service.Insert(new Blog { BlogId = 1, Author = "Tanaka", Name = "Taro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "1", Timestamp = DateTime.Now });
            await _service.Insert(new Blog { BlogId = 2, Author = "Suzuki", Name = "Jiro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "2", Timestamp = DateTime.Now });
            await _service.Insert(new Blog { BlogId = 3, Author = "Sato", Name = "Saburo", RowKey = Guid.NewGuid().ToString(), PartitionKey = "3", Timestamp = DateTime.Now });

            // Act
            var blogs = await _service.GetList();

            // Assert
            blogs.Count().Is(3);
        }

        [Fact]
        public async Task GetList_TableQueryを指定して取得()
        {
            // Arrange
            await _service.DeleteTable();
            await _service.Insert(new Blog { BlogId = 1, Author = "Tanaka", Name = "Taro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "1", Timestamp = DateTime.Now });
            await _service.Insert(new Blog { BlogId = 2, Author = "Suzuki", Name = "Jiro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "2", Timestamp = DateTime.Now });
            await _service.Insert(new Blog { BlogId = 3, Author = "Sato", Name = "Saburo", RowKey = Guid.NewGuid().ToString(), PartitionKey = "3", Timestamp = DateTime.Now });
            var query = new TableQuery<Blog>().Where(TableQuery.GenerateFilterCondition("Author", QueryComparisons.Equal, "Tanaka"));

            // Act
            var blogs = await _service.GetList(query);

            // Assert
            blogs.Count().Is(1);
            blogs.First().Author.Is("Tanaka");
        }

        [Fact]
        public async Task GetItem_正常取得()
        {
            // Arrange
            await _service.DeleteTable();
            var guid = Guid.NewGuid().ToString();
            await _service.Insert(new Blog { BlogId = 1, Author = "Tanaka", Name = "Taro", RowKey = Guid.NewGuid().ToString(), PartitionKey = "1", Timestamp = DateTime.Now });
            await _service.Insert(new Blog { BlogId = 2, Author = "Suzuki", Name = "Jiro", RowKey = guid, PartitionKey = "2", Timestamp = DateTime.Now });
            await _service.Insert(new Blog { BlogId = 3, Author = "Sato", Name = "Saburo", RowKey = Guid.NewGuid().ToString(), PartitionKey = "3", Timestamp = DateTime.Now });

            // Act
            var blog = await _service.GetItem("2", guid);

            // Assert
            blog.IsNotNull();
            blog.Author.Is("Suzuki");
        }

        [Fact]
        public async Task Update_正常更新()
        {
            // Arrange
            await _service.DeleteTable();
            var guid = Guid.NewGuid().ToString();
            await _service.Insert(new Blog { BlogId = 1, Author = "Tanaka", Name = "Taro", RowKey = guid, PartitionKey = "1", Timestamp = DateTime.Now });

            // Act
            var blog = await _service.GetItem("1", guid);
            blog.Author = "Yamada";
            await _service.Update(blog);
            blog = await _service.GetItem("1", guid);

            // Assert
            blog.IsNotNull();
            blog.Author.Is("Yamada");
        }

        [Fact]
        public async Task Delete_正常削除()
        {
            // Arrange
            await _service.DeleteTable();
            var guid = Guid.NewGuid().ToString();
            await _service.Insert(new Blog { BlogId = 1, Author = "Tanaka", Name = "Taro", RowKey = guid, PartitionKey = "1", Timestamp = DateTime.Now });

            // Act
            var blog = await _service.GetItem("1", guid);
            await _service.Delete("1", guid);

            // Assert
            var blogs = await _service.GetList();
            blogs.Count().Is(0);
        }
    }
}
