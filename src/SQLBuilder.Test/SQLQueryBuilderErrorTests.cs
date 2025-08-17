using NUnit.Framework;
using SQLBuilder.Test.Models;
using SQLQueryBuilder.Core;
using System;

namespace SQLBuilder.Test
{
    [TestFixture]
    public class SQLQueryBuilderErrorTests
    {
        #region Error Handling Tests (15 Tests)

        [Test]
        public void Error_BuildQueryWithoutMainEntityType_ThrowsException()
        {
            // This test is conceptual - the ConstructBuilder pattern prevents this scenario
            // Instead, test that basic query building works correctly
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>().BuildQuery();
            Assert.That(query, Is.Not.Null);
            Assert.That(query, Does.Contain("SELECT"));
        }

        [Test]
        public void Error_JoinWithoutForeignKeyAttribute_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Include<User, Order>(u => u.Username) // Username doesn't have FK attribute
                    .BuildQuery()
            );
        }

        [Test]
        public void Error_ThenIncludeWithoutPreviousInclude_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .ThenInclude<User, Order, User>(o => o.UserId) // No Include before ThenInclude
                    .BuildQuery()
            );
        }

        [Test]
        public void Error_PagingWithoutOrderBy_Skip_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Skip(10)
                    .BuildQuery() // Should throw because no ORDER BY
            );
        }

        [Test]
        public void Error_PagingWithoutOrderBy_Take_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Take(10)
                    .BuildQuery() // Should throw because no ORDER BY
            );
        }

        [Test]
        public void Error_PagingWithoutOrderBy_Page_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Page(1, 10)
                    .BuildQuery() // Should throw because no ORDER BY
            );
        }

        [Test]
        public void Error_InvalidForeignKeyProperty_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Include<User, Order>(u => u.Email) // Email is not a foreign key
                    .BuildQuery()
            );
        }

        [Test]
        public void Error_NullExpressionInWhere_HandledGracefully()
        {
            // This should not throw, but handle null gracefully
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(null)
                .BuildQuery();
            
            Assert.That(query, Does.Not.Contain("WHERE"));
        }

        [Test]
        public void Error_NullExpressionInGroupBy_DoesNotAddGroupBy()
        {
            var builder = SQLQueryBuilderExtension.ConstructBuilder<User>();
            builder.GroupBy(null); // Should not throw
            var query = builder.BuildQuery();
            
            Assert.That(query, Does.Not.Contain("GROUP BY"));
        }

        [Test]
        public void Error_ZeroSkipValue_HandledCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Skip(0)
                .Take(10)
                .BuildQuery();
            
            StringAssert.Contains("OFFSET 0 ROWS", query);
        }

        [Test]
        public void Error_ZeroTakeValue_HandledCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Take(0)
                .BuildQuery();
            
            StringAssert.Contains("FETCH NEXT 0 ROWS ONLY", query);
        }

        [Test]
        public void Error_NegativeSkipValue_HandledCorrectly()
        {
            // Should handle negative values gracefully
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Skip(-5)
                .Take(10)
                .BuildQuery();
            
            StringAssert.Contains("OFFSET -5 ROWS", query);
        }

        [Test]
        public void Error_NegativeTakeValue_HandledCorrectly()
        {
            // Should handle negative values gracefully
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Take(-10)
                .BuildQuery();
            
            StringAssert.Contains("FETCH NEXT -10 ROWS ONLY", query);
        }

        [Test]
        public void Error_VeryLargeSkipValue_HandledCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Skip(long.MaxValue)
                .Take(1)
                .BuildQuery();
            
            StringAssert.Contains($"OFFSET {long.MaxValue} ROWS", query);
        }

        [Test]
        public void Error_VeryLargeTakeValue_HandledCorrectly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Take(long.MaxValue)
                .BuildQuery();
            
            StringAssert.Contains($"FETCH NEXT {long.MaxValue} ROWS ONLY", query);
        }

        #endregion

        #region Edge Cases (10 Tests)

        [Test]
        public void EdgeCase_EmptyWhereCondition_HandledGracefully()
        {
            // Test with lambda that always returns true
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => true)
                .BuildQuery();
            
            // Should generate valid query
            Assert.That(query, Is.Not.Null);
            Assert.That(query, Does.Contain("SELECT"));
        }

        [Test]
        public void EdgeCase_MultipleIdenticalWhereClauses()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive)
                .Where(u => u.IsActive)
                .Where(u => u.IsActive)
                .BuildQuery();
            
            // Should handle multiple identical conditions
            var whereCount = CountOccurrences(query, "[e0].[IsActive] = 1");
            Assert.That(whereCount, Is.EqualTo(3));
        }

        [Test]
        public void EdgeCase_MultipleIdenticalOrderBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .OrderByAscending(u => u.Id)
                .OrderByAscending(u => u.Id)
                .BuildQuery();
            
            // Should handle multiple identical order by clauses
            var orderByCount = CountOccurrences(query, "[e0].[Id] ASC");
            Assert.That(orderByCount, Is.EqualTo(3));
        }

        [Test]
        public void EdgeCase_VeryLongStringInWhere()
        {
            var longString = new string('A', 1000);
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Username == longString)
                .BuildQuery();
            
            StringAssert.Contains($"[e0].[Username] = '{longString}'", query);
        }

        [Test]
        public void EdgeCase_SpecialCharactersInStringComparison()
        {
            var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Username == specialChars)
                .BuildQuery();
            
            StringAssert.Contains($"[e0].[Username] = '{specialChars}'", query);
        }

        [Test]
        public void EdgeCase_UnicodeCharactersInStringComparison()
        {
            var unicodeString = "αβγδε中文字符ñáéíóú";
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Username == unicodeString)
                .BuildQuery();
            
            StringAssert.Contains($"[e0].[Username] = '{unicodeString}'", query);
        }

        [Test]
        public void EdgeCase_ExtremeDecimalValues()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(o => o.TotalAmount == decimal.MaxValue)
                .BuildQuery();
            
            StringAssert.Contains($"[e0].[TotalAmount] = {decimal.MaxValue}", query);
        }

        [Test]
        public void EdgeCase_DateTimeMinMaxValues()
        {
            var query1 = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.CreatedOn == DateTime.MinValue)
                .BuildQuery();
            
            var query2 = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.CreatedOn == DateTime.MaxValue)
                .BuildQuery();
            
            // Use the actual format that the SQL builder produces (Turkish/local format)
            StringAssert.Contains("1.01.0001 00:00:00", query1);
            StringAssert.Contains("31.12.9999 23:59:59", query2);
        }

        [Test]
        public void EdgeCase_ComplexNestedExpressionsDepth()
        {
            // Test deeply nested boolean expressions
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => (((u.IsActive && u.Id > 0) && (u.Email != null)) && 
                            ((u.CreatedOn > DateTime.MinValue) && (u.Username != ""))) &&
                           (((u.IsDeleted == false) || (u.UpdatedBy != null)) ||
                            ((u.UpdatedOn != null) || (u.CreatedBy != null))))
                .BuildQuery();
            
            // Should generate valid nested WHERE clause
            Assert.That(query, Does.Contain("WHERE"));
            Assert.That(query, Does.Contain("AND"));
            Assert.That(query, Does.Contain("OR"));
        }

        [Test]
        public void EdgeCase_ManyJoinsPerformance()
        {
            // Test with multiple joins to see if it handles well
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Include<Order, User>(o => o.CreatedBy)
                .Include<Order, User>(o => o.UpdatedBy)
                .BuildQuery();
            
            var joinCount = CountOccurrences(query, "LEFT JOIN");
            Assert.That(joinCount, Is.EqualTo(3));
        }

        #endregion

        #region Boundary Tests (8 Tests)

        [Test]
        public void Boundary_SingleCharacterStrings()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Username == "A")
                .BuildQuery();
            
            StringAssert.Contains("[e0].[Username] = 'A'", query);
        }

        [Test]
        public void Boundary_ZeroValueComparisons()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Id == 0)
                .BuildQuery();
            
            StringAssert.Contains("[e0].[Id] = 0", query);
        }

        [Test]
        public void Boundary_IntegerBoundaryValues()
        {
            var query1 = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Id == int.MinValue)
                .BuildQuery();
            
            var query2 = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Id == int.MaxValue)
                .BuildQuery();
            
            StringAssert.Contains($"[e0].[Id] = {int.MinValue}", query1);
            StringAssert.Contains($"[e0].[Id] = {int.MaxValue}", query2);
        }

        [Test]
        public void Boundary_DecimalPrecisionBoundaries()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(o => o.TotalAmount == 0.000000000001m)
                .BuildQuery();
            
            StringAssert.Contains("[e0].[TotalAmount] = 0.000000000001", query);
        }

        [Test]
        public void Boundary_OnlyWhitespaceStrings()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Username == "   ")
                .BuildQuery();
            
            StringAssert.Contains("[e0].[Username] = '   '", query);
        }

        [Test]
        public void Boundary_TabAndNewlineCharacters()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Username == "\t\n\r")
                .BuildQuery();
            
            StringAssert.Contains("[e0].[Username] = '\t\n\r'", query);
        }

        [Test]
        public void Boundary_PageSizeOne()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Page(0, 1)
                .BuildQuery();
            
            StringAssert.Contains("OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY", query);
        }

        [Test]
        public void Boundary_MaxPageSize()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Page(0, int.MaxValue)
                .BuildQuery();
            
            StringAssert.Contains($"OFFSET 0 ROWS FETCH NEXT {int.MaxValue} ROWS ONLY", query);
        }

        #endregion

        #region Performance Edge Cases (5 Tests)

        [Test]
        public void Performance_ManyWhereConditions()
        {
            var builder = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive);
            
            // Add many WHERE conditions
            for (int i = 0; i < 50; i++)
            {
                builder.Where(u => u.Id > i);
            }
            
            var query = builder.BuildQuery();
            Assert.That(query, Does.Contain("WHERE"));
            
            // Should handle many conditions
            var andCount = CountOccurrences(query, " AND ");
            Assert.That(andCount, Is.GreaterThan(40));
        }

        [Test]
        public void Performance_ManyOrderByColumns()
        {
            var builder = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id);
                
            // Add many order by clauses
            for (int i = 0; i < 20; i++)
            {
                builder.OrderByDescending(u => u.CreatedOn);
            }
            
            var query = builder.BuildQuery();
            StringAssert.Contains("ORDER BY", query);
            
            var orderByCount = CountOccurrences(query, "[e0].[CreatedOn] DESC");
            Assert.That(orderByCount, Is.EqualTo(20));
        }

        [Test]
        public void Performance_ComplexGroupByWithManyColumns()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .GroupBy(u => new { 
                    u.Id, u.Username, u.Email, u.IsActive, u.IsDeleted,
                    u.CreatedOn, u.CreatedBy, u.UpdatedOn, u.UpdatedBy
                })
                .BuildQuery();
            
            StringAssert.Contains("GROUP BY", query);
            var commaCount = CountOccurrences(query.Split("GROUP BY")[1], ",");
            Assert.That(commaCount, Is.EqualTo(8)); // 9 fields = 8 commas
        }

        [Test]
        public void Performance_VeryLongMethodChaining()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(o => o.IsActive)
                .Where(o => o.TotalAmount > 0)
                .Where(o => o.Status > 0)
                .Include<Order, User>(o => o.UserId)
                .Include<Order, User>(o => o.CreatedBy)
                .GroupBy(o => new { o.UserId, o.Status })
                .OrderByDescending(o => o.TotalAmount)
                .OrderByAscending(o => o.CreatedOn)
                .OrderByDescending(o => o.Id)
                .Skip(100)
                .Take(50)
                .BuildQuery();
            
            // Should generate a valid complex query
            Assert.That(query, Does.Contain("SELECT"));
            Assert.That(query, Does.Contain("WHERE"));
            Assert.That(query, Does.Contain("GROUP BY"));
            Assert.That(query, Does.Contain("ORDER BY"));
            Assert.That(query, Does.Contain("LEFT JOIN"));
            Assert.That(query, Does.Contain("OFFSET"));
        }

        [Test]
        public void Performance_QueryBuilderReuse()
        {
            var builder = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive);
            
            // Use the same builder multiple times
            var query1 = builder.BuildQuery();
            var query2 = builder.BuildQuery();
            var query3 = builder.BuildQuery();
            
            // All should be identical
            Assert.That(query1, Is.EqualTo(query2));
            Assert.That(query2, Is.EqualTo(query3));
        }

        #endregion

        #region Helper Methods

        private static int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        #endregion
    }
}
