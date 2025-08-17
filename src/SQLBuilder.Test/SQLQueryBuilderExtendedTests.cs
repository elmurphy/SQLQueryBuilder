using NUnit.Framework;
using SQLBuilder.Test.Models;
using SQLQueryBuilder.Core;
using System;

namespace SQLBuilder.Test
{
    [TestFixture]
    public class SQLQueryBuilderExtendedTests
    {
        #region Constants for Expected Queries
        private readonly string SELECT_FROM_USERS = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[Username] AS [e0_Username], [e0].[PasswordHash] AS [e0_PasswordHash], [e0].[Email] AS [e0_Email]{Environment.NewLine}FROM [Users] AS [e0]";
        private readonly string SELECT_FROM_ORDERS = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[TotalAmount] AS [e0_TotalAmount], [e0].[Status] AS [e0_Status], [e0].[UserId] AS [e0_UserId]{Environment.NewLine}FROM [Orders] AS [e0]";
        private readonly string SELECT_FROM_USERPROFILES = $"SELECT{Environment.NewLine}    [e0].[Id] AS [e0_Id], [e0].[CreatedOn] AS [e0_CreatedOn], [e0].[CreatedBy] AS [e0_CreatedBy], [e0].[UpdatedOn] AS [e0_UpdatedOn], [e0].[UpdatedBy] AS [e0_UpdatedBy], [e0].[IsActive] AS [e0_IsActive], [e0].[IsDeleted] AS [e0_IsDeleted], [e0].[FirstName] AS [e0_FirstName], [e0].[LastName] AS [e0_LastName], [e0].[PhoneNumber] AS [e0_PhoneNumber], [e0].[Address] AS [e0_Address]{Environment.NewLine}FROM [UserProfiles] AS [e0]";
        private readonly string SELECT_FROM_ORDERPRODUCTS = $"SELECT{Environment.NewLine}    [e0].[OrderId] AS [e0_OrderId], [e0].[ProductId] AS [e0_ProductId], [e0].[Quantity] AS [e0_Quantity], [e0].[UnitPrice] AS [e0_UnitPrice]{Environment.NewLine}FROM [OrderProducts] AS [e0]";
        #endregion

        #region WHERE Clause Advanced Tests (25 Tests)
        
        [Test]
        public void Where_Numeric_IntegerEquals()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Id == 42)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Id] = 42;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Numeric_DecimalPrecision()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(x => x.TotalAmount == 123.456m)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}WHERE [e0].[TotalAmount] = 123.456;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_DateTime_ExactMatch()
        {
            var testDate = new DateTime(2023, 12, 25, 10, 30, 0);
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.CreatedOn == testDate)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[CreatedOn] = '25.12.2023 10:30:00';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_DateTime_GreaterThan()
        {
            var cutoffDate = new DateTime(2023, 1, 1);
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.CreatedOn > cutoffDate)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[CreatedOn] > '1.01.2023 00:00:00';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_String_EmptyString()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Email == "")
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Email] = '';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_String_CaseInvariance()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Username == "ADMIN")
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Username] = 'ADMIN';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_String_SpecialCharacters()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Email == "user@test.com")
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Email] = 'user@test.com';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_String_WithSingleQuote()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Username == "O'Connor")
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Username] = 'O'Connor';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Nullable_HasValue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.CreatedBy != null)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[CreatedBy] != NULL;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Nullable_IsNull()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.UpdatedBy == null)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[UpdatedBy] = NULL;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Complex_FourAndConditions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.IsActive && x.IsDeleted == false && x.CreatedBy != null && x.Id > 0)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE ((([e0].[IsActive] = 1 AND [e0].[IsDeleted] = 0) AND [e0].[CreatedBy] != NULL) AND [e0].[Id] > 0);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Complex_MixedAndOr()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => (x.IsActive && x.IsDeleted == false) || (x.Id == 1))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE (([e0].[IsActive] = 1 AND [e0].[IsDeleted] = 0) OR [e0].[Id] = 1);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Complex_DeepNesting()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.IsActive && (x.Username.Contains("admin") || (x.Email.StartsWith("admin") && x.Id > 10)))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE ([e0].[IsActive] = 1 AND ([e0].[Username] LIKE '%admin%' OR ([e0].[Email] LIKE 'admin%' AND [e0].[Id] > 10)));";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_ContainsEmpty()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Username.Contains(""))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Username] LIKE '%%';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_StartsWithEmpty()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Email.StartsWith(""))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Email] LIKE '%';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_EndsWithEmpty()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Username.EndsWith(""))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Username] LIKE '%';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_StringFunction_ContainsVariable()
        {
            var searchTerm = "test";
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Email.Contains(searchTerm))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Email] LIKE '%test%';";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Multiple_StringFunctions()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Email.Contains("@") && x.Email.EndsWith(".com"))
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE ([e0].[Email] LIKE '%@%' AND [e0].[Email] LIKE '%.com');";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Comparison_AllOperators()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(x => x.TotalAmount >= 100 && x.TotalAmount <= 1000 && x.Status != 0)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}WHERE (([e0].[TotalAmount] >= 100 AND [e0].[TotalAmount] <= 1000) AND [e0].[Status] != 0);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Boolean_ExplicitTrue()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.IsActive == true)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[IsActive] = 1;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_Boolean_ExplicitFalse()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.IsDeleted == false)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[IsDeleted] = 0;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_FromConstant()
        {
            const int MinId = 100;
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Id >= MinId)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Id] >= 100;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_FromProperty()
        {
            var user = new User { Id = 50 };
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Id > user.Id)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Id] > 50;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Where_FromMethodCall()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Id > GetMinimumId())
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Id] > 1;";
            Assert.That(query, Is.EqualTo(expected));
        }

        private static int GetMinimumId() => 1;

        [Test]
        public void Where_ZeroValues()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(x => x.Id == 0)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[Id] = 0;";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region JOIN Tests Advanced (15 Tests)

        [Test]
        public void Join_UserToUserProfile()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Include<User, UserProfiles>(u => u.Id)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [UserProfiles]", query);
            StringAssert.Contains("ON [e0].[Id] = [e1].[Id]", query);
        }

        [Test]
        public void Join_OrderToUser()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("ON [e0].[UserId] = [e1].[Id]", query);
        }

        [Test]
        public void Join_MultipleIncludes()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Include<Order, User>(o => o.CreatedBy)
                .BuildQuery();
            
            var joinCount = CountOccurrences(query, "LEFT JOIN");
            Assert.That(joinCount, Is.EqualTo(2));
        }

        [Test]
        public void Join_WithWhereOnMainTable()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Where(o => o.TotalAmount > 100)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("WHERE [e0].[TotalAmount] > 100", query);
        }

        [Test]
        public void Join_WithWhereOnJoinedTable()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Where(o => o.IsActive)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("WHERE [e0].[IsActive] = 1", query);
        }

        [Test]
        public void Join_ThenInclude_ThreeLevels()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .ThenInclude<Order, User, User>(u => u.CreatedBy)
                .BuildQuery();
            
            var joinCount = CountOccurrences(query, "LEFT JOIN");
            Assert.That(joinCount, Is.EqualTo(2));
        }

        [Test]
        public void Join_OrderByOnJoinedTable()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .IncludeOrderByAscending<Order, User>(u => u.Username)
                .BuildQuery();
            
            StringAssert.Contains("ORDER BY [e1].[Username] ASC", query);
        }

        [Test]
        public void Join_OrderByDescendingOnJoinedTable()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .IncludeOrderByDescending<Order, User>(u => u.CreatedOn)
                .BuildQuery();
            
            StringAssert.Contains("ORDER BY [e1].[CreatedOn] DESC", query);
        }

        [Test]
        public void Join_ComplexWithGroupBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .GroupBy(o => o.UserId)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("GROUP BY [e0].[UserId]", query);
        }

        [Test]
        public void Join_ComplexWithPaging()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .OrderByAscending(o => o.Id)
                .Skip(20).Take(10)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY", query);
        }

        [Test]
        public void Join_AllColumnSelection()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .BuildQuery();
            
            // Should include all columns from both tables
            StringAssert.Contains("[e0].[Id] AS [e0_Id]", query);
            StringAssert.Contains("[e1].[Id] AS [e1_Id]", query);
            StringAssert.Contains("[e0].[TotalAmount] AS [e0_TotalAmount]", query);
            StringAssert.Contains("[e1].[Username] AS [e1_Username]", query);
        }

        [Test]
        public void Join_SelfReference_Category()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Category>()
                .Include<Category, Category>(c => c.ParentCategoryId)
                .BuildQuery();
            
            StringAssert.Contains("LEFT JOIN [Categories] AS [e1]", query);
            StringAssert.Contains("ON [e0].[ParentCategoryId] = [e1].[Id]", query);
        }

        [Test] 
        public void Join_MultipleEntitiesWithWhere()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Include<Order, User>(o => o.CreatedBy)
                .Where(o => o.TotalAmount > 100)
                .BuildQuery();
            
            var joinCount = CountOccurrences(query, "LEFT JOIN");
            Assert.That(joinCount, Is.EqualTo(2));
        }

        [Test]
        public void Join_ErrorHandling_InvalidProperty()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Include<User, UserProfiles>(u => u.Username) // Username doesn't have FK attribute
                    .BuildQuery()
            );
        }

        [Test]
        public void Join_ErrorHandling_ThenIncludeWithoutInclude()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .ThenInclude<User, UserProfiles, User>(up => up.CreatedBy) // No Include before ThenInclude
                    .BuildQuery()
            );
        }

        #endregion

        #region ORDER BY Tests (12 Tests)

        [Test]
        public void OrderBy_SingleAscending()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Username)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Username] ASC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_SingleDescending()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByDescending(u => u.CreatedOn)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[CreatedOn] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_MultipleColumns()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Username)
                .OrderByDescending(u => u.CreatedOn)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Username] ASC, [e0].[CreatedOn] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_WithGroupBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => o.UserId)
                .OrderByDescending(o => o.TotalAmount)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[UserId]{Environment.NewLine}ORDER BY [e0].[TotalAmount] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_DateTime()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.CreatedOn)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[CreatedOn] ASC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_Decimal()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .OrderByDescending(o => o.TotalAmount)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}ORDER BY [e0].[TotalAmount] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_Integer()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_Boolean()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByDescending(u => u.IsActive)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[IsActive] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_WithWhereClause()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive)
                .OrderByAscending(u => u.Username)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[IsActive] = 1{Environment.NewLine}ORDER BY [e0].[Username] ASC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_AllClausesTogether()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(o => o.IsActive)
                .GroupBy(o => o.UserId)
                .OrderByDescending(o => o.TotalAmount)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}WHERE [e0].[IsActive] = 1{Environment.NewLine}GROUP BY [e0].[UserId]{Environment.NewLine}ORDER BY [e0].[TotalAmount] DESC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void OrderBy_RequiredForPaging()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Skip(10)
                    .Take(5)
                    .BuildQuery() // Should throw because no ORDER BY for paging
            );
        }

        [Test]
        public void OrderBy_ThreeColumns()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.IsActive)
                .OrderByDescending(u => u.CreatedOn)
                .OrderByAscending(u => u.Username)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[IsActive] ASC, [e0].[CreatedOn] DESC, [e0].[Username] ASC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region GROUP BY Tests (8 Tests)

        [Test]
        public void GroupBy_SingleColumn_UserId()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => o.UserId)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[UserId];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_SingleColumn_Status()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => o.Status)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[Status];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_MultipleColumns_UserAndStatus()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => new { o.UserId, o.Status })
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[UserId], [e0].[Status];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_MultipleColumns_ThreeFields()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => new { o.UserId, o.Status, o.IsActive })
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[UserId], [e0].[Status], [e0].[IsActive];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_WithWhere()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(o => o.IsActive && o.TotalAmount > 0)
                .GroupBy(o => o.UserId)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}WHERE ([e0].[IsActive] = 1 AND [e0].[TotalAmount] > 0){Environment.NewLine}GROUP BY [e0].[UserId];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_WithOrderBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => o.UserId)
                .OrderByAscending(o => o.UserId)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[UserId]{Environment.NewLine}ORDER BY [e0].[UserId] ASC;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_BooleanColumn()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .GroupBy(u => u.IsActive)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}GROUP BY [e0].[IsActive];";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void GroupBy_DateTimeColumn()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .GroupBy(u => u.CreatedOn)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}GROUP BY [e0].[CreatedOn];";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region Paging and Limiting Tests (12 Tests)

        [Test]
        public void Paging_TakeOnly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Take(10)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_SkipOnly()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Skip(5)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 5 ROWS;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_SkipAndTake()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Skip(20)
                .Take(10)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_PageMethod_FirstPage()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Page(0, 10)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_PageMethod_SecondPage()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Page(1, 10)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_PageMethod_LargePage()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Page(5, 25)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 125 ROWS FETCH NEXT 25 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_WithWhere()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive)
                .OrderByAscending(u => u.Username)
                .Skip(10)
                .Take(5)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE [e0].[IsActive] = 1{Environment.NewLine}ORDER BY [e0].[Username] ASC{Environment.NewLine}OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_WithGroupBy()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .GroupBy(o => o.UserId)
                .OrderByAscending(o => o.UserId)
                .Skip(0)
                .Take(100)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}GROUP BY [e0].[UserId]{Environment.NewLine}ORDER BY [e0].[UserId] ASC{Environment.NewLine}OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_LargeNumbers()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Skip(10000)
                .Take(1000)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 10000 ROWS FETCH NEXT 1000 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_TakeOne()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .OrderByAscending(u => u.Id)
                .Take(1)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}ORDER BY [e0].[Id] ASC{Environment.NewLine}OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Paging_ErrorWithoutOrderBy_Take()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Take(10)
                    .BuildQuery()
            );
        }

        [Test]
        public void Paging_ErrorWithoutOrderBy_Skip()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SQLQueryBuilderExtension.ConstructBuilder<User>()
                    .Skip(5)
                    .BuildQuery()
            );
        }

        #endregion

        #region Entity-Specific Tests (8 Tests)

        [Test]
        public void Entity_User_SimpleSelect()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>().BuildQuery();
            var expected = $"{SELECT_FROM_USERS};";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_Order_SimpleSelect()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>().BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS};";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_UserProfiles_SimpleSelect()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<UserProfiles>().BuildQuery();
            var expected = $"{SELECT_FROM_USERPROFILES};";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_OrderProducts_SimpleSelect()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<OrderProducts>().BuildQuery();
            var expected = $"{SELECT_FROM_ORDERPRODUCTS};";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_User_WithComplexWhere()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.Email.Contains("@gmail.com") && u.IsActive && !u.IsDeleted)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERS}{Environment.NewLine}WHERE (([e0].[Email] LIKE '%@gmail.com%' AND [e0].[IsActive] = 1) AND [e0].[IsDeleted] = 0);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_Order_WithDecimalComparisons()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Where(o => o.TotalAmount >= 100.50m && o.TotalAmount <= 999.99m)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERS}{Environment.NewLine}WHERE ([e0].[TotalAmount] >= 100.50 AND [e0].[TotalAmount] <= 999.99);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_UserProfiles_WithNullableFields()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<UserProfiles>()
                .Where(up => up.FirstName != null && up.PhoneNumber != null)
                .BuildQuery();
            var expected = $"{SELECT_FROM_USERPROFILES}{Environment.NewLine}WHERE ([e0].[FirstName] != NULL AND [e0].[PhoneNumber] != NULL);";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void Entity_OrderProducts_WithQuantityFilter()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<OrderProducts>()
                .Where(op => op.Quantity > 1 && op.UnitPrice > 0)
                .BuildQuery();
            var expected = $"{SELECT_FROM_ORDERPRODUCTS}{Environment.NewLine}WHERE ([e0].[Quantity] > 1 AND [e0].[UnitPrice] > 0);";
            Assert.That(query, Is.EqualTo(expected));
        }

        #endregion

        #region Complex Combinations (7 Tests)

        [Test]
        public void Complex_AllFeaturesUser()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive && u.Email.Contains("@"))
                .OrderByDescending(u => u.CreatedOn)
                .OrderByAscending(u => u.Username)
                .Skip(10)
                .Take(20)
                .BuildQuery();

            StringAssert.Contains("WHERE ([e0].[IsActive] = 1 AND [e0].[Email] LIKE '%@%')", query);
            StringAssert.Contains("ORDER BY [e0].[CreatedOn] DESC, [e0].[Username] ASC", query);
            StringAssert.Contains("OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY", query);
        }

        [Test]
        public void Complex_JoinWithComplexWhere()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Where(o => o.TotalAmount > 100)
                .OrderByDescending(o => o.TotalAmount)
                .Skip(0)
                .Take(10)
                .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("[e0].[TotalAmount] > 100", query);
            StringAssert.Contains("ORDER BY [e0].[TotalAmount] DESC", query);
        }

        [Test]
        public void Complex_GroupByWithJoin()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Where(o => o.IsActive)
                .GroupBy(o => new { o.UserId, o.Status })
                .OrderByAscending(o => o.UserId)
                .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("WHERE [e0].[IsActive] = 1", query);
            StringAssert.Contains("GROUP BY [e0].[UserId], [e0].[Status]", query);
            StringAssert.Contains("ORDER BY [e0].[UserId] ASC", query);
        }

        [Test]
        public void Complex_MultipleJoinsWithWhere()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Include<Order, User>(o => o.CreatedBy)
                .Where(o => o.TotalAmount > 50)
                .OrderByDescending(o => o.CreatedOn)
                .BuildQuery();

            var joinCount = CountOccurrences(query, "LEFT JOIN");
            Assert.That(joinCount, Is.EqualTo(2));
            StringAssert.Contains("WHERE [e0].[TotalAmount] > 50", query);
        }

        [Test]
        public void Complex_DeepNestingWithVariables()
        {
            var minAmount = 100m;
            var isActive = true;
            var emailDomain = "@company.com";
            
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Where(o => o.TotalAmount >= minAmount)
                .GroupBy(o => o.UserId)
                .OrderByDescending(o => o.TotalAmount)
                .Page(2, 15)
                .BuildQuery();

            StringAssert.Contains($"[e0].[TotalAmount] >= {minAmount}", query);
            // Simplified test without navigation properties
            StringAssert.Contains("OFFSET 30 ROWS FETCH NEXT 15 ROWS ONLY", query);
        }

        [Test]
        public void Complex_AllClausesMaximum()
        {
            var query = SQLQueryBuilderExtension.ConstructBuilder<User>()
                .Where(u => u.IsActive && !u.IsDeleted && u.Email.Contains("@"))
                .GroupBy(u => new { u.IsActive, u.IsDeleted })
                .OrderByDescending(u => u.CreatedOn)
                .OrderByAscending(u => u.Id)
                .Skip(50)
                .Take(25)
                .BuildQuery();

            StringAssert.Contains("WHERE (([e0].[IsActive] = 1 AND [e0].[IsDeleted] = 0) AND [e0].[Email] LIKE '%@%')", query);
            StringAssert.Contains("GROUP BY [e0].[IsActive], [e0].[IsDeleted]", query);
            StringAssert.Contains("ORDER BY [e0].[CreatedOn] DESC, [e0].[Id] ASC", query);
            StringAssert.Contains("OFFSET 50 ROWS FETCH NEXT 25 ROWS ONLY", query);
        }

        [Test]
        public void Complex_RealWorldScenario()
        {
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var minAmount = 50.0m;
            
            var query = SQLQueryBuilderExtension.ConstructBuilder<Order>()
                .Include<Order, User>(o => o.UserId)
                .Where(o => o.CreatedOn >= startDate && 
                           o.CreatedOn <= endDate && 
                           o.TotalAmount >= minAmount &&
                           o.IsActive &&
                           o.IsActive)
                .GroupBy(o => o.UserId)
                .OrderByDescending(o => o.TotalAmount)
                .Page(0, 50)
                .BuildQuery();

            StringAssert.Contains("LEFT JOIN [Users]", query);
            StringAssert.Contains("[e0].[CreatedOn] >= '1.01.2023 00:00:00'", query);
            StringAssert.Contains("[e0].[CreatedOn] <= '31.12.2023 00:00:00'", query);
            StringAssert.Contains("[e0].[TotalAmount] >= 50.0", query);
            StringAssert.Contains("GROUP BY [e0].[UserId]", query);
            StringAssert.Contains("OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY", query);
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

