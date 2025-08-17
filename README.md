# SQLQueryBuilder

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)]()
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Compatible-orange.svg)]()

A powerful, type-safe, and fluent SQL query builder library for .NET that provides LINQ-like syntax for building complex SQL queries with joins, filtering, ordering, grouping, and pagination.

## 🚀 Features

### ✅ Currently Supported

- **Type-Safe Query Building**: Strongly typed expressions prevent runtime SQL errors
- **Fluent API**: Intuitive method chaining similar to LINQ
- **Complex WHERE Clauses**: Support for AND/OR operations, nested conditions, and all comparison operators
- **JOIN Operations**: 
  - LEFT JOIN with `Include<T, V>()`
  - Chained joins with `ThenInclude<T, V, K>()`
  - Filter joined tables with `IncludeWhere<T, V>()`
- **Ordering**: 
  - Single and multiple column ordering
  - `OrderByAscending()` and `OrderByDescending()`
  - Secondary ordering with `ThenBy()` and `ThenByDescending()`
  - Ordering on joined tables with `IncludeOrderByAscending()` and `IncludeOrderByDescending()`
- **Grouping**: Support for `GROUP BY` with single or multiple columns
- **Pagination**: 
  - `Skip()` and `Take()` methods
  - `Page(index, size)` method for easier pagination
  - Automatic OFFSET/FETCH NEXT generation
- **String Operations**: 
  - `Contains()`, `StartsWith()`, `EndsWith()`
  - Automatic LIKE clause generation
- **Async Execution**: 
  - `GetListAsync<T>()` for retrieving collections
  - `GetSingleAsync<T>()` for single entity retrieval
- **Attribute-Based Mapping**:
  - `[SQBTable]` for custom table names
  - `[SQBPrimaryKey]` for primary key identification
  - `[SQBForeignKey<T>]` for relationship mapping

### 🎯 Query Types Supported
- **SELECT** queries with full feature support
- Complex multi-table queries with joins
- Filtered and sorted result sets
- Paginated queries
- Grouped and aggregated data

## 📦 Installation

```bash
# Add to your project
dotnet add package SQLQueryBuilder.Core
dotnet add package SQLQueryBuilder.Flags
```

## 🔧 Quick Start

### 1. Define Your Entities

```csharp
[SQBTable("Users")]
public class User
{
    [SQBPrimaryKey]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    
    [SQBForeignKey<UserProfile>]
    public int? ProfileId { get; set; }
}

[SQBTable("UserProfiles")]
public class UserProfile
{
    [SQBPrimaryKey]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
}
```

### 2. Basic Usage Examples

```csharp
using SQLQueryBuilder.Core;

// Simple SELECT query
var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
    .BuildQuery();
// Generates: SELECT [e0].[Id], [e0].[Username], ... FROM [Users] AS [e0];

// WHERE clause
var activeUsers = SQLQueryBuilderExtension.ConstructBuilder<User>()
    .Where(u => u.IsActive && u.Email.Contains("@company.com"))
    .BuildQuery();

// JOIN with related table
var usersWithProfiles = SQLQueryBuilderExtension.ConstructBuilder<User>()
    .Include<User, UserProfile>(u => u.ProfileId)
    .Where(u => u.IsActive)
    .BuildQuery();

// Complex query with all features
var complexQuery = SQLQueryBuilderExtension.ConstructBuilder<User>()
    .Include<User, UserProfile>(u => u.ProfileId)
    .Where(u => u.IsActive && u.CreatedOn > DateTime.Now.AddDays(-30))
    .IncludeWhere<User, UserProfile>(p => p.FirstName != null)
    .OrderByDescending(u => u.CreatedOn)
    .ThenBy(u => u.Username)
    .Skip(20)
    .Take(10)
    .BuildQuery();
```

### 3. Async Data Retrieval

```csharp
// Get list of entities
var users = await SQLQueryBuilderExtension.ConstructBuilder<User>()
    .Where(u => u.IsActive)
    .OrderByDescending(u => u.CreatedOn)
    .Take(50)
    .GetListAsync();

// Get single entity
var user = await SQLQueryBuilderExtension.ConstructBuilder<User>()
    .Where(u => u.Id == 123)
    .GetSingleAsync();
```

## 🔍 Advanced Features

### Complex WHERE Conditions

```csharp
// Multiple AND conditions
.Where(u => u.IsActive && u.Email.Contains("@") && u.CreatedOn > startDate)

// OR conditions with grouping
.Where(u => (u.IsActive && u.Email.EndsWith(".com")) || u.Id == 1)

// Nested conditions
.Where(u => u.IsActive && (u.Username.Contains("admin") || 
    (u.Email.StartsWith("admin") && u.Id > 10)))
```

### Multiple JOINs

```csharp
var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
    .Include<Order, User>(o => o.UserId)           // JOIN Users
    .Include<Order, Product>(o => o.ProductId)     // JOIN Products  
    .ThenInclude<Product, Category>(p => p.CategoryId)  // JOIN Categories
    .Where(o => o.IsActive)
    .IncludeWhere<Order, User>(u => u.IsActive)    // Filter joined users
    .BuildQuery();
```

### Pagination

```csharp
// Method 1: Skip/Take
.OrderByAscending(u => u.Id)
.Skip(20)
.Take(10)

// Method 2: Page method
.OrderByAscending(u => u.Id) 
.Page(2, 10)  // Page 2, 10 items per page (skips 20, takes 10)
```

### Grouping

```csharp
// Single column grouping
.GroupBy(o => o.UserId)

// Multiple column grouping  
.GroupBy(o => new { o.UserId, o.Status, o.CreatedYear })
```

## 🏗️ Architecture

The library consists of three main components:

1. **SQLQueryBuilder.Core**: Core query building logic and execution
2. **SQLQueryBuilder.Flags**: Attribute definitions for entity mapping
3. **SQLBuilder.Test**: Comprehensive test suite with 150+ tests

### Key Classes

- `SQLQueryBuilder<T>`: Main query builder class
- `SQLQueryBuilderExtension`: Extension methods and factory methods
- `ExpressionParser`: Converts LINQ expressions to SQL
- `SQBSqlWhere`: Represents WHERE clause conditions

## ⚠️ Current Limitations & Unsupported Features

### Database Support
- ❌ **Only SQL Server**: Currently supports SQL Server only
- ❌ **No MySQL/PostgreSQL**: Other databases not yet supported
- ❌ **No SQLite**: Lightweight database support missing

### Query Types
- ❌ **INSERT Operations**: No support for data insertion
- ❌ **UPDATE Operations**: No support for data updates  
- ❌ **DELETE Operations**: No support for data deletion
- ❌ **Stored Procedures**: Cannot execute stored procedures
- ❌ **Functions & Views**: No support for database functions or views

### Advanced SQL Features
- ❌ **Aggregations**: No SUM, COUNT, AVG, MIN, MAX support
- ❌ **Subqueries**: No nested SELECT statements
- ❌ **UNION/INTERSECT**: Set operations not supported
- ❌ **Window Functions**: No ROW_NUMBER, RANK, etc.
- ❌ **Common Table Expressions (CTEs)**: WITH clauses not supported
- ❌ **HAVING Clause**: Filtering after GROUP BY not available

### JOIN Types
- ❌ **INNER JOIN**: Only LEFT JOIN currently supported
- ❌ **RIGHT JOIN**: Not implemented
- ❌ **FULL OUTER JOIN**: Not available
- ❌ **CROSS JOIN**: Cartesian products not supported

### Data Types & Operations
- ❌ **JSON Support**: No JSON column operations
- ❌ **XML Support**: No XML data type handling
- ❌ **Spatial Data**: No geography/geometry support
- ❌ **Binary Data**: Limited binary data handling
- ❌ **Date Functions**: No DATEADD, DATEDIFF, etc.

### Performance & Scalability
- ❌ **Query Caching**: No built-in query plan caching
- ❌ **Connection Pooling**: Basic connection management
- ❌ **Bulk Operations**: No bulk insert/update support
- ❌ **Streaming**: No data streaming for large results

## 🚧 Roadmap

### Phase 1: Core Enhancements (Q1 2024)
- [ ] **Multi-Database Support**
  - MySQL compatibility
  - PostgreSQL support
  - SQLite integration
- [ ] **Advanced JOIN Types**
  - INNER JOIN implementation
  - RIGHT JOIN support
  - FULL OUTER JOIN
- [ ] **Aggregation Functions**
  - COUNT, SUM, AVG, MIN, MAX
  - GROUP BY with HAVING clause
- [ ] **Query Optimization**
  - Query plan caching
  - Better performance monitoring

### Phase 2: Advanced Features (Q2 2024)
- [ ] **DML Operations**
  - INSERT statement builder
  - UPDATE with complex WHERE
  - DELETE operations
  - UPSERT (MERGE) support
- [ ] **Subqueries & CTEs**
  - Nested SELECT statements
  - Common Table Expressions
  - Correlated subqueries
- [ ] **Advanced SQL Features**
  - Window functions
  - UNION/INTERSECT operations
  - CASE statements

### Phase 3: Enterprise Features (Q3 2024)
- [ ] **Performance & Scalability**
  - Connection pooling
  - Async streaming
  - Bulk operations
  - Query result caching
- [ ] **Security & Monitoring**
  - SQL injection prevention audit
  - Query performance analytics
  - Execution time monitoring
- [ ] **Advanced Data Types**
  - JSON column support
  - XML data handling
  - Spatial data types

### Phase 4: Developer Experience (Q4 2024)
- [ ] **Code Generation**
  - Entity class generators
  - Scaffold from existing DB
  - Migration tools
- [ ] **IDE Integration**
  - Visual Studio extensions
  - IntelliSense improvements
  - Debugging tools
- [ ] **Documentation & Samples**
  - Interactive tutorials
  - Best practices guide
  - Performance tuning guide

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/your-org/SQLQueryBuilder.git

# Restore packages
dotnet restore

# Run tests
dotnet test

# Build the solution
dotnet build
```

### Running Tests

The project includes a comprehensive test suite with 150+ tests covering:

- Basic SELECT operations
- Complex WHERE conditions  
- JOIN operations and scenarios
- ORDER BY and GROUP BY functionality
- Pagination and limiting
- Error handling and edge cases
- Performance testing

```bash
# Run all tests
dotnet test --verbosity normal

# Run specific test category
dotnet test --filter Category=JoinTests
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [Documentation](https://docs.sqlquerybuilder.com)
- [API Reference](https://api.sqlquerybuilder.com)
- [Examples Repository](https://github.com/your-org/SQLQueryBuilder-Examples)
- [Issue Tracker](https://github.com/your-org/SQLQueryBuilder/issues)

## 💡 Support

- 📧 Email: support@sqlquerybuilder.com
- 💬 Discussions: [GitHub Discussions](https://github.com/your-org/SQLQueryBuilder/discussions)
- 🐛 Bug Reports: [GitHub Issues](https://github.com/your-org/SQLQueryBuilder/issues)
- 📚 Stack Overflow: Tag with `sql-query-builder`

---

**Made with ❤️ by the SQLQueryBuilder Team**
