using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureInnovationDemosDAL
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AzureDemosDBContext>
    {
        public AzureDemosDBContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(@Directory.GetCurrentDirectory() + "/../AzureInnovationDemosDAL/appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<AzureDemosDBContext>();
            var connectionString = configuration.GetConnectionString("AzureDemosDB");
            builder.UseSqlServer(connectionString);
            return new AzureDemosDBContext(builder.Options);
        }
    }
}
