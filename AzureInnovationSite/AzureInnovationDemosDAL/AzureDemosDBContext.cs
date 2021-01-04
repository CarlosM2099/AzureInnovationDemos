using AzureInnovationDemosDAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL
{
    public class AzureDemosDBContext : DbContext
    {
        public DbSet<Demo> Demos { get; set; }
        public DbSet<DemoType> DemoTypes { get; set; }
        public DbSet<DemoGuide> DemoGuides { get; set; }
        public DbSet<DemoVM> DemoVMs { get; set; }
        public DbSet<DemoAsset> DemoAssets { get; set; }
        public DbSet<DemoAzureResource> DemoAzureResources { get; set; }
        public DbSet<DemoSharedCredentials> DemoSharedCredentials { get; set; }
        public DbSet<DemoAssetType> DemoAssetTypes { get; set; }
        public DbSet<DemoAzureResourceType> DemoAzureResourceTypes { get; set; }
        public DbSet<DemoUserEnvironment> DemoUserEnvironments { get; set; }
        public DbSet<DemoUserResource> DemoUserResources { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDemoOrganization> UserDemoOrganizations { get; set; }
        public DbSet<UserDemoAzureResource> UserDemoAzureResources { get; set; }
        public DbSet<UserRDPLog> UserRDPLogs { get; set; }
   
        public AzureDemosDBContext(DbContextOptions contextOptions)
           : base(contextOptions)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.c.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<DemoAzureResource>().HasIndex(resource => new { resource.LockedUntil }).IsUnique(false);

            modelBuilder.Entity<DemoAsset>()
                     .HasOne(dr => dr.Demo)
                     .WithMany(s => s.Assets)
                     .HasForeignKey(s => s.DemoId);


            modelBuilder.Entity<DemoAzureResource>()
                   .HasOne(dr => dr.Demo)
                   .WithMany(s => s.AzureResources)
                   .HasForeignKey(s => s.DemoId);

            modelBuilder.Entity<UserDemoAzureResource>()
                .HasKey(r => new { r.UserId, r.DemoAzureResourceId });
        }
    }
}
