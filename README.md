> [!WARNING]
> This is a personal project. No consistent development or maintenance is planned for the future.

> [!NOTE]
> Check out the [wiki](https://github.com/Memonia/Barbados/wiki) for more information and usage examples.

# About

[![Memonia.Barbados](https://img.shields.io/nuget/v/Memonia.Barbados?style=for-the-badge&logo=nuget&labelColor=%23004880&color=grey)](https://www.nuget.org/packages/Memonia.Barbados)  
Barbados is an embedded, ACID, thread-safe document store which includes write-ahead log, transaction processing, queries and custom document storage format.

Major features:
* **Transaction processing**: each collection or index operation is a part of an automatic transaction or a user-created explicit transaction.  
* **Disaster recovery**: transactions rely on WAL (Write-Ahead Log) to avoid data loss. 
* **Queries:** documents in a collection can be retrieved using provided query API. 
* **Concurrency:** collections and indexes support multiple simultaneous readers or a single writer. 
* **B-Tree:** collections are organised as clustered indexes and may have an unlimited number of non-clustered indexes. Instead of using standard overflow pages to handle documents which don't fit on a single page, the underlying B-Tree implements automatic chunking. Chunking improves space utilisation of a clustered index and makes it dependent on how well the B-Tree is balanced. 
* **Custom document format:** under the hood, a document is a serialised radix tree. This format allows documents to be loaded into memory without any deserialisation or pre-processing steps. Because each field, no matter how deep it is nested in sub-documents, is addressable individually, independent of other fields, they can be indexed or loaded partially, just like the top-level fields. 
