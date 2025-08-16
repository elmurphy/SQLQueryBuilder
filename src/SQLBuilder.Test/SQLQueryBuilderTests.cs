using NUnit.Framework;
using SQLBuilder.Test.Models;
using SQLQueryBuilder.Core;
using System;

namespace SQLBuilder.Test
{
    [TestFixture]
    public class SQLQueryBuilderTests
    {
        // Beklenen SQL sorgularýný daha okunabilir kýlmak için güncellenmiþ sabitler.
        private const string SELECT_FROM_CATEGORIES = "SELECT [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Description] AS [e0_Description], [e0].[ParentCategoryId] AS [e0_ParentCategoryId] FROM [Categories] AS [e0]";
        private const string SELECT_FROM_PRODUCTS = "SELECT [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId] FROM [Products] AS [e0]";

        [SetUp]
        public void Setup()
        {
            // Testler arasý baþlangýç ayarlarý için.
        }

        #region Basic SELECT Tests

        [Test]
        public void Select_BuildsCorrectSimpleSelectQueryForCategory()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>().BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES};"));
        }

        [Test]
        public void Select_BuildsCorrectSimpleSelectQueryForProduct()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>().BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS};"));
        }

        #endregion

        #region WHERE Clause Tests - Operators & Values

        [Test]
        public void Where_BuildsCorrectlyForEqualsOperator()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Id == 5).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Id] = 5;"));
        }

        [Test]
        public void Where_BuildsCorrectlyForGreaterThanOperator()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount > 50).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] > 50;"));
        }

        [Test]
        public void Where_HandlesStringValueCorrectlyWithQuotes()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Name == "Laptop").BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Name] = 'Laptop';"));
        }

        [Test]
        public void Where_HandlesVariableInExpression()
        {
            var minStock = 10;
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount >= minStock).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] >= 10;"));
        }

        [Test]
        public void Where_HandlesBooleanTrueCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive == true).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1;"));
        }

        [Test]
        public void Where_HandlesImplicitBooleanTrue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1;"));
        }

        [Test]
        public void Where_HandlesBooleanNotOperator()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => !x.IsDeleted).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[IsDeleted] = 0;"));
        }

        [Test]
        public void Where_HandlesNullableWithValue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.UpdatedBy == 1).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[UpdatedBy] = 1;"));
        }

        [Test]
        public void Where_HandlesNullableWithNull()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.UpdatedOn == null).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[UpdatedOn] = NULL;"));
        }

        #endregion

        #region WHERE Clause Tests - String Functions

        [Test]
        public void Where_TranslatesStringLengthCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Length > 10).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE LEN([e0].[Name]) > 10;"));
        }

        [Test]
        public void Where_TranslatesStringContainsCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Contains("Book")).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Name] LIKE '%Book%';"));
        }

        [Test]
        public void Where_TranslatesStringStartsWithCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Name.StartsWith("Smart")).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Name] LIKE 'Smart%';"));
        }

        [Test]
        public void Where_TranslatesStringEndsWithCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Description.EndsWith("...")).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Description] LIKE '%...';"));
        }

        #endregion

        #region WHERE Clause Tests - Multiple Conditions

        [Test]
        public void Where_HandlesMultipleAndConditionsCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive && x.Price < 2000 && x.StockCount > 0).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1 AND [e0].[Price] < 2000 AND [e0].[StockCount] > 0;"));
        }

        #endregion

        #region ORDER BY Tests

        [Test]
        public void OrderBy_BuildsCorrectlyForAscending()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .OrderByAscending(x => x.Name)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} ORDER BY [e0_Name] ASC;"));
        }

        [Test]
        public void OrderBy_BuildsCorrectlyForDescending()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByDescending(x => x.Price)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0_Price] DESC;"));
        }

        [Test]
        public void OrderBy_WithWhereClause_BuildsCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount > 0)
                .OrderByDescending(x => x.CreatedOn)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] > 0 ORDER BY [e0_CreatedOn] DESC;"));
        }

        #endregion

        #region Paging (OFFSET/FETCH) Tests

        [Test]
        public void SkipAndTake_BuildsCorrectOffsetFetchClause()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(50)
                .Take(25)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0_Id] ASC OFFSET 50 ROWS FETCH NEXT 25 ROWS ONLY;"));
        }

        [Test]
        public void Page_BuildsCorrectOffsetFetchClause()
        {
            // 3. sayfa, sayfa boyutu 20 (ilk 40'ý atla, sonraki 20'yi al)
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByDescending(x => x.Id)
                .Page(2, 20)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0_Id] DESC OFFSET 40 ROWS FETCH NEXT 20 ROWS ONLY;"));
        }

        [Test]
        public void Paging_ThrowsException_WithoutOrderBy()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Product>().Skip(10).BuildQuery()
            );
        }

        #endregion

        #region JOIN (Include/ThenInclude) Tests

        [Test]
        public void Include_BuildsCorrectLeftJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .BuildQuery();

            // DÜZELTÝLMÝÞ KISIM: Beklenen SQL metni artýk daha okunaklý ve doðru.
            const string expectedSql =
                "SELECT " +
                "[e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], " +
                "[e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId] " +
                "FROM [Products] AS [e0] " +
                "LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id];";

            Assert.That(query, Is.EqualTo(expectedSql));
        }

        // Bu test, güncellenmiþ Category modeli sayesinde artýk somut olarak çalýþabilir.
        [Test]
        public void ThenInclude_BuildsCorrectChainedLeftJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
                .BuildQuery();

            // DÜZELTÝLMÝÞ KISIM: Bu test için de beklenen SQL metni daha saðlam hale getirildi.
            const string expectedSql =
                "SELECT " +
                "[e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], " +
                "[e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId], " +
                "[e2].[Id] AS [e2_Id], [e2].[CreatedOn] AS [e2_CreatedOn], [e2].[CreatedBy] AS [e2_CreatedBy], [e2].[UpdatedOn] AS [e2_UpdatedOn], [e2].[UpdatedBy] AS [e2_UpdatedBy], [e2].[IsActive] AS [e2_IsActive], [e2].[IsDeleted] AS [e2_IsDeleted], [e2].[Name] AS [e2_Name], [e2].[Description] AS [e2_Description], [e2].[ParentCategoryId] AS [e2_ParentCategoryId] " +
                "FROM [Products] AS [e0] " +
                "LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] " +
                "LEFT JOIN [Categories] AS [e2] ON [e1].[ParentCategoryId] = [e2].[Id];";

            Assert.That(query, Is.EqualTo(expectedSql));
        }

        [Test]
        public void ThenInclude_ThrowsException_WhenCalledBeforeInclude()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Product>()
                    .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
                    .BuildQuery()
            );
        }

        #endregion

        #region GROUP BY Tests

        [Test]
        public void GroupBy_BuildsCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => p.CategoryId)
                .BuildQuery();

            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} GROUP BY [e0_CategoryId];"));
        }

        [Test]
        public void GroupBy_WithWhereAndOrderBy_BuildsCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.IsActive)
                .GroupBy(p => p.CategoryId)
                .OrderByAscending(p => p.CategoryId)
                .BuildQuery();

            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1 GROUP BY [e0_CategoryId] ORDER BY [e0_CategoryId] ASC;"));
        }

        #endregion
    }
}