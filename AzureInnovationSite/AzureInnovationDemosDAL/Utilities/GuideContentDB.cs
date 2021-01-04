using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Utilities
{
    public class GuideContentDB
    {
        private readonly string DBConnectionString;
        public GuideContentDB()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(@Directory.GetCurrentDirectory() + "/../AzureInnovationDemosDAL/appsettings.json").Build();
            DBConnectionString = configuration.GetConnectionString("GuideContentDB");
           
        }

        public GuideContentDB(string dbConnectionString)
        {            
            DBConnectionString = dbConnectionString;

        }

        public async Task InsertGuide(int demoAssetId, string demoGuideContent)
        {
            SqlConnection conn = new SqlConnection(DBConnectionString);

            try
            {
                SqlCommand sqlCommand = new SqlCommand($"INSERT INTO [dbo].[DemoGuide] ([DemoAssetId], [GuideContent]) " +
                    $"VALUES (@demoAssetId,@demoGuideContent)");

                sqlCommand.Parameters.Add(new SqlParameter("@demoAssetId", demoAssetId));
                sqlCommand.Parameters.Add(new SqlParameter("@demoGuideContent", demoGuideContent));

                sqlCommand.Connection = conn;
                conn.Open();

                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error in GuideContentDB manager occured", ex);
            }
            finally
            {
                conn.Close();
            }
        }

        public async Task<bool> GuideExists(int demoAssetId)
        {
            SqlConnection conn = new SqlConnection(DBConnectionString);
            int result = 0;
            string sqlCommand = $"SELECT COUNT(*) FROM [dbo].[DemoGuide] WHERE [DemoAssetId] = {demoAssetId}";

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = sqlCommand
                };

                conn.Open();
                result = (int)await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error in GuideContentDB manager occured", ex);
            }
            finally
            {
                conn.Close();
            }

            return result > 0;
        }


        public async Task<string> GetGuideContent(int demoAssetId)
        {
            SqlConnection conn = new SqlConnection(DBConnectionString);
            string result = string.Empty;
            string sqlCommand = $"SELECT [GuideContent] FROM [dbo].[DemoGuide] WHERE [DemoAssetId] = {demoAssetId}";

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = sqlCommand
                };

                conn.Open();
                result = (string)await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error in GuideContentDB manager occured", ex);
            }
            finally
            {
                conn.Close();
            }

            return result;
        }

        public async Task UpdateGuide(int demoAssetId, string demoGuideContent)
        {
            SqlConnection conn = new SqlConnection(DBConnectionString);

            try
            {
                SqlCommand sqlCommand = new SqlCommand($"UPDATE [DemoGuide] SET [GuideContent] = @demoGuideContent " +
                   $"WHERE [DemoAssetId] = @demoAssetId");

                sqlCommand.Parameters.Add(new SqlParameter("@demoAssetId", demoAssetId));
                sqlCommand.Parameters.Add(new SqlParameter("@demoGuideContent", demoGuideContent));

                sqlCommand.Connection = conn;

                conn.Open();
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error in GuideContentDB manager occured", ex);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
