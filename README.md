# dotnet-core-table-storage-sample
.NET CoreでAzure Table Storageを操作するサンプル

## Feature
- .NET Core 2.2
- Azure Table Storage
- Storage Emulator

## Usage
1. TableEntity を継承したClassでTable Storage用のEntityを作成する。
```cs
public class Blog : TableEntity
{
    public int BlogId { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
}
```

2. ServiceCollectionにTableStorageServiceを登録し、アプリケーション内で利用する。
```cs
serviceCollection.AddSingleton<ITableStorageService<Blog>>(provider => new TableStorageService<Blog>(configuration["ConnectionStrings:StorageConnection"]));
```