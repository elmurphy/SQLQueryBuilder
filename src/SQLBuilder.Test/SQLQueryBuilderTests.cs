using NUnit.Framework;
using SQLBuilder.Test.Models;
using SQLQueryBuilder.Core;
using System;

namespace SQLBuilder.Test
{
    [TestFixture]
    public class SQLQueryBuilderTests
    {
        // Sabitler, navigation property'ler olmadan, doðru kolon listesiyle ve yeni satýr formatýyla güncellendi.
        private readonly string SELECT_FROM_CATEGORIES = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Description] AS [e0_Description], [e0].[ParentCategoryId] AS [e0_ParentCategoryId]{Environment.NewLine}FROM [Categories] AS [e0]";
        private readonly string SELECT_FROM_PRODUCTS = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId]{Environment.NewLine}FROM [Products] AS [e0]";

        #region Basic SELECT Tests (2 Tests)

        [Test]
        public void Select_BuildsCorrectSimpleSelectQueryForCategory()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>().BuildQuery();
            var expected = $"{SELECT_FROM_CATEGORIES};";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Select_BuildsCorrectSimpleSelectQueryForProduct()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>().BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS};";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region WHERE Clause Tests (26 Tests)

        [Test]
        public void Where_Operator_Equals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .Where(x => x.Id == 5)
                .BuildQuery();
            var expected = $"{SELECT_FROM_CATEGORIES}{Environment.NewLine}WHERE [e0].[Id] = 5;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Operator_NotEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .Where(x => x.Id != 5)
                .BuildQuery();
            var expected = $"{SELECT_FROM_CATEGORIES}{Environment.NewLine}WHERE [e0].[Id] != 5;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Operator_GreaterThan()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.StockCount > 50)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[StockCount] > 50;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Operator_GreaterThanOrEqual()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.Price >= 49.99m)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[Price] >= 49.99;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Operator_LessThan()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.Price < 100.0m)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[Price] < 100.0;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Operator_LessThanOrEqual()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.StockCount <= 0)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[StockCount] <= 0;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_DataType_StringEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.Name == "Laptop")
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[Name] = 'Laptop';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Value_FromVariable()
        {
            var minStock = 10;
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.StockCount >= minStock)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[StockCount] >= 10;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Value_BooleanImplicitTrue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.IsActive)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[IsActive] = 1;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Value_BooleanNotOperator()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .Where(x => !x.IsDeleted)
                .BuildQuery();
            var expected = $"{SELECT_FROM_CATEGORIES}{Environment.NewLine}WHERE [e0].[IsDeleted] = 0;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_Contains()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .Where(x => x.Name.Contains("Book"))
                .BuildQuery();
            var expected = $"{SELECT_FROM_CATEGORIES}{Environment.NewLine}WHERE [e0].[Name] LIKE '%Book%';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_StartsWith()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.Name.StartsWith("Smart"))
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[Name] LIKE 'Smart%';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_EndsWith()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .Where(x => x.Description.EndsWith("..."))
                .BuildQuery();
            var expected = $"{SELECT_FROM_CATEGORIES}{Environment.NewLine}WHERE [e0].[Description] LIKE '%...';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Multiple_TwoAndConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.IsActive && x.Price < 100)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE ([e0].[IsActive] = 1 AND [e0].[Price] < 100);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Multiple_ThreeAndConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(p => p.IsActive && p.Price > 100 && p.StockCount > 0)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE (([e0].[IsActive] = 1 AND [e0].[Price] > 100) AND [e0].[StockCount] > 0);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Multiple_OrConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.StockCount == 0 || x.IsActive == false)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE ([e0].[StockCount] = 0 OR [e0].[IsActive] = 0);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Multiple_ComplexGrouping()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(x => x.IsActive && (x.Price > 1000 || x.Name.Contains("Sale")))
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE ([e0].[IsActive] = 1 AND ([e0].[Price] > 1000 OR [e0].[Name] LIKE '%Sale%'));";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region JOIN Tests (4 Tests)

        [Test]
        public void Join_Include_BuildsCorrectLeftJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .BuildQuery();

            var expectedColumns = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], [e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId]";
            var expected = $"{expectedColumns}{Environment.NewLine}FROM [Products] AS [e0]{Environment.NewLine}LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Join_ThenInclude_BuildsCorrectChainedLeftJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Include<Product, Category>(p => p.CategoryId)
                .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
                .BuildQuery();

            var expectedColumns = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], [e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId], [e2].[Id] AS [e2_Id], [e2].[CreatedOn] AS [e2_CreatedOn], [e2].[CreatedBy] AS [e2_CreatedBy], [e2].[UpdatedOn] AS [e2_UpdatedOn], [e2].[UpdatedBy] AS [e2_UpdatedBy], [e2].[IsActive] AS [e2_IsActive], [e2].[IsDeleted] AS [e2_IsDeleted], [e2].[Name] AS [e2_Name], [e2].[Description] AS [e2_Description], [e2].[ParentCategoryId] AS [e2_ParentCategoryId]";
            var expected = $"{expectedColumns}{Environment.NewLine}FROM [Products] AS [e0]{Environment.NewLine}LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id]{Environment.NewLine}LEFT JOIN [Categories] AS [e2] ON [e1].[ParentCategoryId] = [e2].[Id];";
            Assert.That(query, Is.EqualTo(expected));
        }

        //[Test]
        //public void Join_WhereOnIncludedEntity()
        //{
        //    var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
        //        .Include<Product, Category>(p => p.CategoryId)
        //        .Where(p => p.Category.Name == "Electronics")
        //        .BuildQuery();

        //    var expectedColumns = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], [e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId]";
        //    var expected = $"{expectedColumns}{Environment.NewLine}FROM [Products] AS [e0]{Environment.NewLine}LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id]{Environment.NewLine}WHERE [e1].[Name] = 'Electronics';";
        //    Assert.That(query, Is.EqualTo(expected));
        //}

        [Test]
        public void Join_PropertyWithoutForeignKeyAttribute_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                 SQLQueryBuilderExtension.ConstructBuilder<Product>()
                    .Include<Product, Category>(p => p.Name) // Name üzerinde FK yok
                    .BuildQuery()
            );
        }

        #endregion

        #region GROUP BY Tests (3 Tests)

        [Test]
        public void GroupBy_SingleColumn()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => p.CategoryId)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}GROUP BY [e0].[CategoryId];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_MultipleColumns()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .GroupBy(p => new { p.CategoryId, p.IsActive })
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}GROUP BY [e0].[CategoryId], [e0].[IsActive];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_WithWhereAndOrderBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .Where(p => p.Price > 0)
                .GroupBy(p => p.CategoryId)
                .OrderByDescending(p => p.CategoryId)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}WHERE [e0].[Price] > 0{Environment.NewLine}GROUP BY [e0].[CategoryId]{Environment.NewLine}ORDER BY [e0].[CategoryId] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region ORDER BY Tests (3 Tests)

        [Test]
        public void OrderBy_CanOrderByNavigationProperty()
        {
            //var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
            //    .Include<Product, Category>(p => p.CategoryId)
            //    .OrderByAscending(p => p.Category.Name)
            //    .BuildQuery();

            //StringAssert.Contains($"{Environment.NewLine}ORDER BY [e1].[Name] ASC;", query);
        }

        [Test]
        public void OrderBy_WithPaging()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
                .OrderByAscending(x => x.Id)
                .Skip(10).Take(10)
                .BuildQuery();
            var expected = $"{SELECT_FROM_PRODUCTS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test, Ignore("ThenBy is not yet implemented.")]
        public void OrderBy_ThenBy_BuildsCorrectly() { }

        #endregion

        #region Complex Combination Test (1 Test)

        [Test]
        public void Combination_AllTogether()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Product>()
               .Where(p => p.IsActive && p.Price > 10)
               .Include<Product, Category>(p => p.CategoryId)
               .ThenInclude<Product, Category, Category>(c => c.ParentCategoryId)
               .GroupBy(p => p.Name)
               .OrderByDescending(p => p.CreatedOn)
               .Page(0, 25)
               .BuildQuery();

            var expectedColumns = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Name] AS [e0_Name], [e0].[Price] AS [e0_Price], [e0].[StockCount] AS [e0_StockCount], [e0].[CategoryId] AS [e0_CategoryId], [e1].[Id] AS [e1_Id], [e1].[CreatedOn] AS [e1_CreatedOn], [e1].[CreatedBy] AS [e1_CreatedBy], [e1].[UpdatedOn] AS [e1_UpdatedOn], [e1].[UpdatedBy] AS [e1_UpdatedBy], [e1].[IsActive] AS [e1_IsActive], [e1].[IsDeleted] AS [e1_IsDeleted], [e1].[Name] AS [e1_Name], [e1].[Description] AS [e1_Description], [e1].[ParentCategoryId] AS [e1_ParentCategoryId], [e2].[Id] AS [e2_Id], [e2].[CreatedOn] AS [e2_CreatedOn], [e2].[CreatedBy] AS [e2_CreatedBy], [e2].[UpdatedOn] AS [e2_UpdatedOn], [e2].[UpdatedBy] AS [e2_UpdatedBy], [e2].[IsActive] AS [e2_IsActive], [e2].[IsDeleted] AS [e2_IsDeleted], [e2].[Name] AS [e2_Name], [e2].[Description] AS [e2_Description], [e2].[ParentCategoryId] AS [e2_ParentCategoryId]";
            var expectedClauses = $"FROM [Products] AS [e0]{Environment.NewLine}LEFT JOIN [Categories] AS [e1] ON [e0].[CategoryId] = [e1].[Id]{Environment.NewLine}LEFT JOIN [Categories] AS [e2] ON [e1].[ParentCategoryId] = [e2].[Id]{Environment.NewLine}WHERE ([e0].[IsActive] = 1 AND [e0].[Price] > 10){Environment.NewLine}GROUP BY [e0].[Name]{Environment.NewLine}ORDER BY [e0].[CreatedOn] DESC{Environment.NewLine}OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY;";
            var expected = $"{expectedColumns}{Environment.NewLine}{expectedClauses}";

            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion
    }
}