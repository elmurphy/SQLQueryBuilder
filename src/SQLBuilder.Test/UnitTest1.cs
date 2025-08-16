using Dapper.FastCrud;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using SQLBuilder.Test.Models;
using SQLQueryBuilder.Core;
using System;
using System.Data.Common;

namespace SQLBuilder.Test
{
    public class Tests
    {
        SqlConnection connection;
        [SetUp]
        public void Setup()
        {
            string connectionString = "Server=localhost,1433;Database=SqlSample;User Id=sa;Password=As.1234567890;TrustServerCertificate=True;";
            connection = new SqlConnection(connectionString);
        }

        [TearDown]
        public void Destructor()
        {
            if (connection != null)
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
            }
        }

        [Test]
        public void GetList()
        {
            var sqlQueryBuilderSimple = SQLQueryBuilderExtension.ConstructBuilder<Category>(x => x.Name.Length > 0).BuildQuery();
        }

        [Test]
        public void GetSingle()
        {
            var sqlQueryBuilderSimple = SQLQueryBuilderExtension.ConstructBuilder<Category>().GetSingleAsync().Result;
        }

        [Test]
        public void GetListWithPage()
        {
            var sqlQueryBuilderSimple = SQLQueryBuilderExtension
                                            .ConstructBuilder<Category>()
                                            .Page(0, 10)
                                            .GetListAsync().Result;
        }

    }
}