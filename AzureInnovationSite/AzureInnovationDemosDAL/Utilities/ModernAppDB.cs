using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AzureInnovationDemosDAL.Utilities
{
    public class ModernAppDB
    {
        private readonly string DBConnectionString;
        public ModernAppDB()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(@Directory.GetCurrentDirectory() + "/appsettings.json").Build();
            DBConnectionString = configuration.GetConnectionString("ModernAppDB");
        }

        public ModernAppDB(string dbConnectionString)
        {
            DBConnectionString = dbConnectionString;
        }

        public bool InsertUser(string userName, string userLastName, string email)
        {
            SqlConnection conn = new SqlConnection(DBConnectionString);
            int result = 0;

            try
            {
                string sqlCommand = $"INSERT INTO [dbo].[Customers] " +
               $"([Email], [AccountCode], [FirstName], [LastName], [FirstAddress], [City], [Country], " +
               $"[ZipCode], [Website], [Active], [Enrrolled], [PhoneNumber], [MobileNumber], [FaxNumber]) " +
               $"VALUES (N'{email}', N'AC741655', N'{userName}', N'{userLastName}', N'400 Broad St', N'Seattle', N'United States', " +
               $"N'98109', N'http://workingdata.com', 1, 0, N'4251231234', N'4253214321', N'4259990000')";

                SqlCommand cmd = new SqlCommand()
                {
                    Connection = conn
                };

                conn.Open();

                cmd.CommandText = sqlCommand;
                cmd.ExecuteScalar();

                sqlCommand = $"INSERT INTO [dbo].[Customers] " +
               $"([Email], [AccountCode], [FirstName], [LastName], [FirstAddress], [City], [Country], " +
               $"[ZipCode], [Website], [Active], [Enrrolled], [PhoneNumber], [MobileNumber], [FaxNumber]) " +
               $"VALUES (N'{email}', N'AC741655', N'{userName} II', N'{userLastName}', N'SRPN - Asa Norte', N'Brasilia', N'Brazil', " +
               $"N'70070', N'http://workingdata.com', 1, 0, N'4251231234', N'4253214321', N'4259990000')";

                cmd.CommandText = sqlCommand;
                cmd.ExecuteScalar();

            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error in ModernAppDB manager occured",ex);
            }
            finally
            {
                conn.Close();
            }

            return result > 0;
        }

        public bool DeleteUserByEmail(string userEmail)
        {
            SqlConnection conn = new SqlConnection(DBConnectionString);
            int result = 0;
            string sqlCommand = $"DELETE FROM [dbo].[Customers] WHERE [Email] = '{userEmail}'";

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = sqlCommand
                };

                conn.Open();
                cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error in ModernAppDB manager occured", ex);
            }
            finally
            {
                conn.Close();
            }

            return result > 0;
        }
    }
}
