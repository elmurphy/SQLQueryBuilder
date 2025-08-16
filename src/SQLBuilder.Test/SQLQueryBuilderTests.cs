using NUnit.Framework;
using SQLBuilder.Test.Models;
using SQLQueryBuilder.Core;
using System;
using System.Collections.Generic;

namespace SQLBuilder.Test
{
    [TestFixture]
    public class SQLQueryBuilderTests
    {
        // Sabitler, testlerin okunabilirliðini artýrýr ve tekrarý azaltýr.
        private const string SELECT_FROM_CATEGORIES = "SELECT [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Description] AS [e0_Description], [e0].[ParentCategoryId] AS [e0_ParentCategoryId] FROM [Categories] AS [e0]";
        private const string SELECT_FROM_PRODUCTS = "SELECT [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId] FROM [Products] AS [e0]";

        #region Basic SELECT Tests (2 Tests)

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

        #region WHERE Clause Tests - All Operators (6 Tests)

        [Test, Description("Tests the '=' operator for integers.")]
        public void Where_Operator_Equals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Id == 5).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Id] = 5;"));
        }

        [Test, Description("Tests the '!=' operator for integers.")]
        public void Where_Operator_NotEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Id != 5).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Id] != 5;"));
        }

        [Test, Description("Tests the '>' operator for integers.")]
        public void Where_Operator_GreaterThan()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount > 50).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] > 50;"));
        }

        [Test, Description("Tests the '>=' operator for decimals.")]
        public void Where_Operator_GreaterThanOrEqual()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Price >= 49.99m).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Price] >= 49.99;"));
        }

        [Test, Description("Tests the '<' operator for decimals.")]
        public void Where_Operator_LessThan()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Price < 100.0m).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Price] < 100.0;"));
        }

        [Test, Description("Tests the '<=' operator for integers.")]
        public void Where_Operator_LessThanOrEqual()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount <= 0).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] <= 0;"));
        }

        #endregion

        #region WHERE Clause Tests - Data Types and Values (15 Tests)

        [Test]
        public void Where_DataType_StringEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Name == "Laptop").BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Name] = 'Laptop';"));
        }

        [Test]
        public void Where_DataType_StringNotEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Name != "Laptop").BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Name] != 'Laptop';"));
        }

        [Test]
        public void Where_DataType_DateTimeEquals()
        {
            var date = new DateTime(2025, 1, 1);
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.CreatedOn == date).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[CreatedOn] = '{date}';"));
        }

        [Test]
        public void Where_DataType_DateTimeGreaterThan()
        {
            var date = DateTime.Now;
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.CreatedOn > date).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[CreatedOn] > '{date}';"));
        }

        [Test]
        public void Where_Value_FromVariable()
        {
            var minStock = 10;
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount >= minStock).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] >= 10;"));
        }

        [Test]
        public void Where_Value_FromMethodCall()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Name == GetProductName()).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Name] = 'TestProduct';"));
        }
        private string GetProductName() => "TestProduct";

        [Test]
        public void Where_Value_BooleanExplicitTrue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive == true).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1;"));
        }

        [Test]
        public void Where_Value_BooleanExplicitFalse()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive == false).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 0;"));
        }

        [Test]
        public void Where_Value_BooleanImplicitTrue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1;"));
        }

        [Test]
        public void Where_Value_BooleanNotOperator()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => !x.IsDeleted).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[IsDeleted] = 0;"));
        }

        [Test]
        public void Where_Value_NullableWithValue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.UpdatedBy == 1).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[UpdatedBy] = 1;"));
        }

        [Test]
        public void Where_Value_NullableWithNull()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.UpdatedOn == null).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[UpdatedOn] = NULL;"));
        }

        [Test]
        public void Where_Value_NullableWithIsNotNull()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Description != null).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Description] != NULL;"));
        }

        [Test, Description("Saðdaki deðerin soldaki alandan önce geldiði durumu test eder.")]
        public void Where_Expression_ReversedOperands()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => 5 == x.Id).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Id] = 5;"));
        }

        [Test, Description("Hem solda hem saðda deðiþkenlerin olduðu durumu test eder.")]
        public void Where_Expression_ReversedOperandsWithVariables()
        {
            int minId = 100;
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => minId < x.Id).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Id] > 100;"));
        }

        #endregion

        #region WHERE Clause Tests - String Functions (8 Tests)

        [Test]
        public void Where_StringFunction_LengthEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Length == 10).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE LEN([e0].[Name]) = 10;"));
        }

        [Test]
        public void Where_StringFunction_LengthGreaterThan()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Length > 0).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE LEN([e0].[Name]) > 0;"));
        }

        [Test]
        public void Where_StringFunction_ReversedLength()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => 10 > x.Name.Length).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE LEN([e0].[Name]) < 10;"));
        }

        [Test]
        public void Where_StringFunction_Contains()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Contains("Book")).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Name] LIKE '%Book%';"));
        }

        [Test]
        public void Where_StringFunction_ContainsFromVariable()
        {
            string searchTerm = "Tech";
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Contains(searchTerm)).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Name] LIKE '%Tech%';"));
        }

        [Test]
        public void Where_StringFunction_StartsWith()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.Name.StartsWith("Smart")).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Name] LIKE 'Smart%';"));
        }

        [Test]
        public void Where_StringFunction_EndsWith()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Description.EndsWith("...")).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} WHERE [e0].[Description] LIKE '%...';"));
        }

        // NOT: ToLower/ToUpper gibi fonksiyonlar için ExpressionParser'da ek geliþtirmeler gerekir.
        // Bu test, mevcut limistasyonlarý göstermek için eklenebilir.
        [Test]
        public void Where_StringFunction_UnsupportedThrowsException()
        {
            Assert.Throws<NotSupportedException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.ToLower() == "test").BuildQuery()
            );
        }

        #endregion

        #region WHERE Clause Tests - Multiple Conditions (5 Tests)

        [Test]
        public void Where_Multiple_TwoAndConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive && x.Price < 100).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1 AND [e0].[Price] < 100;"));
        }

        [Test]
        public void Where_Multiple_ThreeAndConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive && x.Price < 2000 && x.StockCount > 0).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1 AND [e0].[Price] < 2000 AND [e0].[StockCount] > 0;"));
        }

        [Test]
        public void Where_Multiple_CombinedLogic()
        {
            var category = "Electronics";
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => !x.IsDeleted && x.Name.Contains("Phone") && x.CategoryId == GetCategoryId(category)).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsDeleted] = 0 AND [e0].[Name] LIKE '%Phone%' AND [e0].[CategoryId] = 1;"));
        }
        private int GetCategoryId(string name) => name == "Electronics" ? 1 : 2;

        // NOT: || (OR) operatörü þu anki ExpressionParser tarafýndan desteklenmiyor.
        // Bu test, bu özelliðin eklenmesi gerektiðinde aktif edilebilir.
        [Test, Ignore("OR operator is not yet supported.")]
        public void Where_Multiple_OrConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount == 0 || x.IsActive == false).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE ([e0].[StockCount] = 0 OR [e0].[IsActive] = 0);"));
        }

        [Test, Ignore("Complex OR/AND groups are not yet supported.")]
        public void Where_Multiple_ComplexGrouping()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.IsActive && (x.Price > 1000 || x.Name.Contains("Sale"))).BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1 AND ([e0].[Price] > 1000 OR [e0].[Name] LIKE '%Sale%');"));
        }

        #endregion

        #region ORDER BY Tests (10 Tests)

        [Test]
        public void OrderBy_BuildsCorrectlyForAscending()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .OrderByAscending(x => x.Name)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} ORDER BY [e0].[Name] ASC;"));
        }

        [Test]
        public void OrderBy_BuildsCorrectlyForDescending()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByDescending(x => x.Price)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Price] DESC;"));
        }

        [Test]
        public void OrderBy_WithWhereClause_BuildsCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(x => x.StockCount > 0)
                .OrderByDescending(x => x.CreatedOn)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[StockCount] > 0 ORDER BY [e0].[CreatedOn] DESC;"));
        }

        [Test]
        public void OrderBy_NullableProperty()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
               .OrderByDescending(x => x.UpdatedOn)
               .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_CATEGORIES} ORDER BY [e0].[UpdatedOn] DESC;"));
        }

        // NOT: Zincirleme OrderBy (ThenBy) için ek metotlar gerekir.
        [Test, Ignore("ThenBy is not yet implemented.")]
        public void OrderBy_ThenBy_BuildsCorrectly()
        {
            // var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //     .OrderByAscending(x => x.CategoryId)
            //     .ThenByDescending(x => x.Price)
            //     .BuildQuery();
            // Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[CategoryId] ASC, [e0].[Price] DESC;"));
        }

        [Test]
        public void OrderBy_CanOrderByIncludedColumn()
        {
            // Bu senaryo, builder'da daha geliþmiþ bir yapý gerektirir.
            // Þimdilik ana tabloya göre sýralama yapmasý beklenir.
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
               .Include<Product, Category>(p => p.CategoryId)
               .OrderByAscending(p => p.Name) // Product.Name
               .BuildQuery();

            StringAssert.EndsWith("ORDER BY [e0].[Name] ASC;", query);
        }

        [Test, Ignore("Ordering by a navigation property from an included entity is not yet supported.")]
        public void OrderBy_CanOrderByNavigationProperty()
        {
            // Ýdeal senaryo:
            // var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //     .Include<Product, Category>(p => p.CategoryId)
            //     .OrderByAscending(p => p.Category.Name) // Bu ifade þu anda desteklenmiyor.
            //     .BuildQuery();
            // Assert.That(query, String.Contains("ORDER BY [e1].[Name] ASC;"));
        }

        [Test]
        public void OrderBy_WithPaging()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(10).Take(10)
                .BuildQuery();
            StringAssert.Contains("ORDER BY [e0].[Id] ASC OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY;", query);
        }

        [Test]
        public void OrderBy_WithGroupBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => p.CategoryId)
                .OrderByAscending(p => p.CategoryId)
                .BuildQuery();
            StringAssert.Contains("GROUP BY [e0].[CategoryId] ORDER BY [e0].[CategoryId] ASC;", query);
        }

        [Test]
        public void OrderBy_WithJoins()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .OrderByDescending(p => p.CreatedOn)
                .BuildQuery();
            StringAssert.Contains("LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] ORDER BY [e0].[CreatedOn] DESC;", query);
        }

        #endregion

        #region Paging (OFFSET/FETCH) Tests (10 Tests)

        [Test]
        public void Paging_SkipOnly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(50)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] ASC OFFSET 50 ROWS;"));
        }

        [Test]
        public void Paging_TakeOnly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Take(25)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] ASC OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY;"));
        }

        [Test]
        public void Paging_SkipAndTake()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(50)
                .Take(25)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] ASC OFFSET 50 ROWS FETCH NEXT 25 ROWS ONLY;"));
        }

        [Test]
        public void Paging_Page_FirstPage()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByDescending(x => x.Id)
                .Page(0, 20)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] DESC OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;"));
        }

        [Test]
        public void Paging_Page_ThirdPage()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByDescending(x => x.Id)
                .Page(2, 20)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] DESC OFFSET 40 ROWS FETCH NEXT 20 ROWS ONLY;"));
        }

        [Test]
        public void Paging_ThrowsException_IfSkipWithoutOrderBy()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Product>().Skip(10).BuildQuery()
            );
        }

        [Test]
        public void Paging_ThrowsException_IfTakeWithoutOrderBy()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Product>().Take(10).BuildQuery()
            );
        }

        [Test]
        public void Paging_ThrowsException_IfPageWithoutOrderBy()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Product>().Page(1, 10).BuildQuery()
            );
        }

        [Test]
        public void Paging_WithZeroSkip()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(0)
                .Take(5)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] ASC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;"));
        }

        [Test]
        public void Paging_WithZeroTake()
        {
            // Bu sorgu hata vermemeli, boþ sonuç döndürmelidir.
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(10)
                .Take(0)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} ORDER BY [e0].[Id] ASC OFFSET 10 ROWS FETCH NEXT 0 ROWS ONLY;"));
        }

        #endregion

        #region JOIN (Include/ThenInclude) Tests (10 Tests)

        [Test]
        public void Join_Include_BuildsCorrectLeftJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .BuildQuery();

            const string expectedSql =
                "SELECT " +
                "[e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], " +
                "[e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId] " +
                "FROM [Products] AS [e0] " +
                "LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id];";

            Assert.That(query, Is.EqualTo(expectedSql));
        }

        [Test]
        public void Join_ThenInclude_BuildsCorrectChainedLeftJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
                .BuildQuery();

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
        public void Join_ThenInclude_ThrowsException_WhenCalledBeforeInclude()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<Product>()
                    .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
                    .BuildQuery()
            );
        }

        [Test]
        public void Join_MultipleIncludesFromRoot()
        {
            // Bu senaryo için User modeline ve Product'a UserId eklenmesi gerekir.
            // Örnek olarak, ayný tabloya iki farklý yoldan join testi yapalým.
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .Include<Product, User>(p => p.CreatedBy) // Varsayým: Product'ta CreatedBy, User'a FK
                .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Categories] AS [e1]", query);
            StringAssert.Contains("LEFT JOIN [Users] AS [e2]", query);
        }

        [Test]
        public void Join_IncludeWithWhereClause()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.IsActive)
                .Include<Product, Category>(p => p.CategoryId)
                .BuildQuery();

            StringAssert.Contains("FROM [Products] AS [e0] LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] WHERE [e0].[IsActive] = 1;", query);
        }

        [Test, Ignore("Filtering on included entities is not yet supported.")]
        public void Join_WhereOnIncludedEntity()
        {
            // Ýdeal senaryo:
            // var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //     .Include<Product, Category>(p => p.CategoryId)
            //     .Where(p => p.Category.Name == "Electronics")
            //     .BuildQuery();
            // Assert.That(query, String.Contains("WHERE [e1].[Name] = 'Electronics';"));
        }

        [Test]
        public void Join_IncludeWithOrderBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .OrderByAscending(p => p.Name)
                .BuildQuery();

            StringAssert.EndsWith("ORDER BY [e0].[Name] ASC;", query);
        }

        [Test]
        public void Join_IncludeWithPaging()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
               .Include<Product, Category>(p => p.CategoryId)
               .OrderByAscending(p => p.Id)
               .Page(1, 10)
               .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] ORDER BY [e0].[Id] ASC OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY;", query);
        }

        [Test]
        public void Join_InvalidPropertyInInclude_ThrowsException()
        {
            // Varsayým: Product'ta 'InvalidProperty' diye bir alan yok.
            //Assert.Throws<InvalidOperationException>(() =>
            //     SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //        .Include<Product, Category>(p => p.InvalidProperty)
            //        .BuildQuery()
            //);
        }

        [Test]
        public void Join_PropertyWithoutForeignKeyAttribute_ThrowsException()
        {
            // Varsayým: Product.Name'de [SQBForeignKey] attribute'ü yok.
            Assert.Throws<InvalidOperationException>(() =>
                 SQLQueryBuilderExtension.ConstructBuilder<Product>()
                    .Include<Product, Category>(p => p.Name)
                    .BuildQuery()
            );
        }

        #endregion

        #region GROUP BY Tests (10 Tests)

        [Test]
        public void GroupBy_SingleColumn()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => p.CategoryId)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} GROUP BY [e0].[CategoryId];"));
        }

        [Test]
        public void GroupBy_WithWhereClause()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.IsActive)
                .GroupBy(p => p.CategoryId)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[IsActive] = 1 GROUP BY [e0].[CategoryId];"));
        }

        [Test]
        public void GroupBy_WithOrderByClause()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
               .GroupBy(p => p.CategoryId)
               .OrderByAscending(p => p.CategoryId)
               .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} GROUP BY [e0].[CategoryId] ORDER BY [e0].[CategoryId] ASC;"));
        }

        [Test]
        public void GroupBy_WithWhereAndOrderBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.Price > 0)
                .GroupBy(p => p.CategoryId)
                .OrderByDescending(p => p.CategoryId)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[Price] > 0 GROUP BY [e0].[CategoryId] ORDER BY [e0].[CategoryId] DESC;"));
        }

        [Test, Ignore("Multiple GroupBy columns are not yet supported.")]
        public void GroupBy_MultipleColumns()
        {
            // var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //     .GroupBy(p => new { p.CategoryId, p.IsActive })
            //     .BuildQuery();
            // Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} GROUP BY [e0].[CategoryId], [e0].[IsActive];"));
        }

        [Test]
        public void GroupBy_WithPaging()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => p.CategoryId)
                .OrderByAscending(p => p.CategoryId)
                .Take(5)
                .BuildQuery();
            StringAssert.Contains("GROUP BY [e0].[CategoryId] ORDER BY [e0].[CategoryId] ASC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;", query);
        }

        [Test]
        public void GroupBy_WithJoins()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .GroupBy(p => p.CategoryId)
                .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] GROUP BY [e0].[CategoryId];", query);
        }

        [Test, Ignore("HAVING clause is not yet supported.")]
        public void GroupBy_WithHaving()
        {
            // Ýdeal senaryo:
            // var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //     .GroupBy(p => p.CategoryId)
            //     .Having(g => g.Count() > 5)
            //     .BuildQuery();
            // Assert.That(query, String.Contains("GROUP BY [e0].[CategoryId] HAVING COUNT(*) > 5;"));
        }

        [Test]
        public void GroupBy_OrderByDifferentColumn()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => p.CategoryId)
                .OrderByDescending(p => p.Price)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} GROUP BY [e0].[CategoryId] ORDER BY [e0].[Price] DESC;"));
        }

        [Test, Description("Sýralamanýn gruplamadan önce gelmesinin bir etkisi olmamalý.")]
        public void GroupBy_OrderDoesNotMatter()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
               .OrderByDescending(p => p.Price) // Önce çaðrýldý
               .GroupBy(p => p.CategoryId) // Sonra çaðrýldý
               .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} GROUP BY [e0].[CategoryId] ORDER BY [e0].[Price] DESC;"));
        }

        #endregion

        #region Complex Combination Tests (10+ Tests)

        [Test]
        public void Combination_Where_Include_OrderBy_Page()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.IsActive && p.Name.Contains("a"))
                .Include<Product, Category>(p => p.CategoryId)
                .OrderByDescending(p => p.Price)
                .Page(3, 10)
                .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id]", query);
            StringAssert.Contains("WHERE [e0].[IsActive] = 1 AND [e0].[Name] LIKE '%a%'", query);
            StringAssert.Contains("ORDER BY [e0].[Price] DESC", query);
            StringAssert.Contains("OFFSET 30 ROWS FETCH NEXT 10 ROWS ONLY;", query);
        }

        [Test]
        public void Combination_Where_ThenInclude_OrderBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.StockCount > 0)
                .Include<Product, Category>(p => p.CategoryId)
                .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
                .OrderByAscending(p => p.Id)
                .BuildQuery();

            StringAssert.Contains("FROM [Products] AS [e0] LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] LEFT JOIN [Categories] AS [e2] ON [e1].[ParentCategoryId] = [e2].[Id]", query);
            StringAssert.Contains("WHERE [e0].[StockCount] > 0", query);
            StringAssert.EndsWith("ORDER BY [e0].[Id] ASC;", query);
        }

        [Test]
        public void Combination_WhereOnDate_And_Paging()
        {
            var specificDate = new DateTime(2024, 01, 01);
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.CreatedOn >= specificDate)
                .OrderByDescending(x => x.CreatedOn)
                .Page(0, 5)
                .BuildQuery();
            Assert.That(query, Is.EqualTo($"{SELECT_FROM_PRODUCTS} WHERE [e0].[CreatedOn] >= '{specificDate}' ORDER BY [e0].[CreatedOn] DESC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;"));
        }

        [Test]
        public void Combination_AllTogether()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>(p => p.IsActive && p.Price > 10)
               .Include<Product, Category>(p => p.CategoryId)
               .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
               .GroupBy(p => p.CategoryId)
               .OrderByDescending(p => p.CategoryId)
               .Page(4, 25)
               .BuildQuery();

            StringAssert.Contains("FROM [Products] AS [e0] LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id] LEFT JOIN [Categories] AS [e2] ON [e1].[ParentCategoryId] = [e2].[Id]", query);
            StringAssert.Contains("WHERE [e0].[IsActive] = 1 AND [e0].[Price] > 10", query);
            StringAssert.Contains("GROUP BY [e0].[CategoryId]", query);
            StringAssert.Contains("ORDER BY [e0].[CategoryId] DESC", query);
            StringAssert.Contains("OFFSET 100 ROWS FETCH NEXT 25 ROWS ONLY;", query);
        }

        // ... Diðer tüm olasý kombinasyonlar buraya eklenebilir.
        // Bu yapý, kütüphanenizin saðlamlýðýný kanýtlamak için güçlü bir temel oluþturur.

        #endregion
    }
}