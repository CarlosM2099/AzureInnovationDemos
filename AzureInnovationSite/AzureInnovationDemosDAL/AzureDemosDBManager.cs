using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL
{
    public class AzureDemosDBManager
    {
        private static readonly TimeSpan DefaultLockValue = TimeSpan.FromDays(5);
        AzureDemosDBContext dbContext;
        public AzureDemosDBManager(DbContextOptions contextOptions)
        {
            dbContext = new AzureDemosDBContext(contextOptions);
        }

        public AzureDemosDBManager(AzureDemosDBContext context)
        {
            dbContext = context;
        }

        public async Task<List<Demo>> GetDemos()
        {
            return await dbContext.Demos
                .Include(d => d.Type)
                .ToListAsync();
        }

        public async Task<List<DemoAsset>> GetDemosAssets()
        {
            return await dbContext.DemoAssets
                .Include(a => a.Type)
                .ToListAsync();
        }

        public async Task<List<DemoAsset>> GetDemoAssets(int id)
        {
            return await dbContext.DemoAssets
                .Include(a => a.Type)
                .Where(a => a.DemoId == id)
                .ToListAsync();
        }

        public async Task<List<Demo>> GetEnabledDemos()
        {
            return await dbContext.Demos
                .Where(d => d.IsDisabled == false)
                .ToListAsync();
        }

        public async Task<List<Demo>> GetVisibleDemos()
        {
            return await dbContext.Demos
                .Where(d => d.IsVisible == true)
                .ToListAsync();
        }

        public async Task<Demo> GetDemo(int id)
        {
            return await dbContext.Demos
                .Include(d => d.SharedCredentials)
                .Include(d => d.Type)
                .Include(d => d.Assets)
                .ThenInclude(a => a.Type)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task UpdateDemoGuide(int demoAssetId, string mdHTMLContent)
        {
            var demoGuide = await dbContext
                 .DemoGuides
                 .FirstOrDefaultAsync(g => g.DemoAssetId == demoAssetId);

            if (demoGuide != null)
            {
                demoGuide.GuideContent = mdHTMLContent;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<User> GetUser(string name)
        {
            return await dbContext.Users
                    .FirstOrDefaultAsync(u => u.AccountName.ToLower() == name.ToLower());
        }

        public async Task AddUserDemoOrganization(string userAccountName, string organizationName)
        {
            var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.AccountName.ToLower() == userAccountName.ToLower());

            dbContext.UserDemoOrganizations.Add(new UserDemoOrganization() { Id = user.Id, Name = organizationName });

            await dbContext.SaveChangesAsync();
        }

        public async Task AddDemoGuide(int demoAssetId, string guideContent)
        {
            dbContext.DemoGuides.Add(new DemoGuide() { DemoAssetId = demoAssetId, GuideContent = guideContent });

            await dbContext.SaveChangesAsync();
        }

        public async Task<DemoGuide> GetDemoGuide(int demoAssetId)
        {
            return await dbContext
                .DemoGuides
                .FirstOrDefaultAsync(g => g.DemoAssetId == demoAssetId);
        }

        public async Task<UserDemoOrganization> GetUserDemoOrganization(string userAccountName)
        {
            var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.AccountName.ToLower() == userAccountName.ToLower());

            return dbContext.UserDemoOrganizations
                .FirstOrDefault(uo => uo.Id == user.Id);

        }

        public async Task<User> CreateUser(string account, string userName, string userMail)
        {
            User user = null;
            MailAddress mail = new MailAddress(userMail);

            user = await dbContext.Users.
                FirstOrDefaultAsync(u => u.AccountName.ToLower() == account.ToLower());

            if (user == null)
            {
                string[] userNames = userName.Split(' ');
                string givenName = "", surname = "";
                givenName = userNames[0];

                if (userNames.Length > 1)
                {
                    surname = userNames[1];
                }

                if (string.IsNullOrEmpty(surname))
                {
                    surname = mail.User;
                }

                string ticks = DateTime.Now.Ticks.ToString();
                string password = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(givenName.Substring(0, 3))}$ado@{ticks.Substring(ticks.Length - 4)}";

                var newUser = new User()
                {
                    AccountName = account,
                    Givenname = givenName,
                    Surname = surname,
                    Password = password,
                    IsDisabled = false,
                    LastLoggin = DateTime.Now,
                    CreatedDate = DateTime.Now
                };

                dbContext.Users.Add(newUser);
                dbContext.SaveChanges();

                return newUser;
            }

            return user;
        }

        public async Task<DemoVM> GetDemoVM(int demoId)
        {
            return await dbContext.DemoVMs
                .FirstOrDefaultAsync(d => d.DemoId == demoId);
        }

        public async Task CreateDemoUserEnvironment(DemoEnvironment demoEnvironment, int userId, int demoId)
        {
            var demoUser = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            var demoEnvironemntUser = demoEnvironment.Users.First();

            dbContext.DemoUserEnvironments.Add(new DemoUserEnvironment()
            {
                UserId = userId,
                DemoId = demoId,
                EnvironmentUser = demoEnvironemntUser.Email,
                EnvironmentPassword = demoUser.Password,
                EnvironmentURL = demoEnvironemntUser.Url,
                CreatedDate = DateTime.Now,
                EnvironmentProvisioned = demoEnvironemntUser.Provisioned,
                EnvironmentDescription = demoEnvironemntUser.Description
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task CreateUserDemoAzureResource(int userId, int azureResourceId)
        {
            dbContext.UserDemoAzureResources.Add(new UserDemoAzureResource()
            {
                UserId = userId,
                DemoAzureResourceId = azureResourceId
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task<UserDemoAzureResource> GetUserDemoAzureResource(int userId, int demoId)
        {
            return await dbContext.UserDemoAzureResources
                .Include(dr => dr.DemoAzureResource)
                .FirstOrDefaultAsync(dr => dr.UserId == userId && dr.DemoAzureResource.DemoId == demoId);
        }

        public async Task<DemoAzureResource> GetDemoAzureResource(int resourceId)
        {
            return await dbContext.DemoAzureResources.FirstOrDefaultAsync(r => r.Id == resourceId);
        }

        public async Task CreateDemoUserResource(int demoId, int userId, string resourceName, string resourceValue, DemoAssetTypeEnum resourceType)
        {
            var demoResourceType = dbContext
                .DemoAssetTypes
                .FirstOrDefault(r => r.Id == (int)resourceType);

            dbContext.DemoUserResources.Add(new DemoUserResource()
            {
                UserId = userId,
                DemoId = demoId,
                Name = resourceName,
                Value = resourceValue,
                Type = demoResourceType,
                CreatedDate = DateTime.Now
            });

            await dbContext.SaveChangesAsync();
        }


        public async Task<List<DemoUserEnvironment>> GetUserEnvironments(int id)
        {
            return await dbContext.DemoUserEnvironments
                .Where(de => de.UserId == id)
                .ToListAsync();
        }

        public async Task<List<DemoUserEnvironment>> GetDemoEnvironments(int id)
        {
            return await dbContext.DemoUserEnvironments
                .Where(de => de.DemoId == id)
                .ToListAsync();
        }

        public async Task<List<DemoAzureResource>> GetDemoAzureResources(int demoId)
        {
            return await dbContext.DemoAzureResources
                .Where(rs => rs.DemoId == demoId)
                .OrderBy(rs => rs.LockedUntil)
                .ToListAsync();
        }

        public async Task<List<DemoUserEnvironment>> GetDemoUserEnvironments(int demoId, int userId)
        {
            return await dbContext.DemoUserEnvironments
                .Where(de => de.UserId == userId && de.DemoId == demoId)
                .ToListAsync();
        }

        public async Task UpdateDemoUserEnvironments(string userAccount, DemoTypeEnum demoType)
        {
            var dbUser = await  GetUser(userAccount);
            var dbDemos = await GetDemos();
            var dbDemo = dbDemos.FirstOrDefault(d => d.Type.Id == (int)demoType);

            var userEnvironments =  await dbContext.DemoUserEnvironments
                .Where(de => de.UserId == dbUser.Id && de.DemoId == dbDemo.Id)
                .ToListAsync();

            userEnvironments.First().EnvironmentProvisioned = true;
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateLogginUser(string account, string userName, string userMail)
        {
            User user = null;
            user = await dbContext.Users.
                FirstOrDefaultAsync(u => u.AccountName.ToLower() == account.ToLower());

            if (user == null)
            {
                await CreateUser(account, userName, userMail);
            }

            else
            {
                user.LastLoggin = DateTime.Now;
                dbContext.SaveChanges();
            }
        }


        public async Task<DemoAzureResource> GetNextDemoAzureResource(int demoId, TimeSpan? lockTime = null)
        {

            if (dbContext.ChangeTracker.HasChanges())
            {
                throw new InvalidOperationException
               (
                   $"Repository contains unsaved changes; {nameof(GetNextDemoAzureResource)} cannot continue."
               );
            }

            DemoAzureResource demoAzureResource = null;
            if (lockTime is null) { lockTime = TimeSpan.FromDays(5); }

            demoAzureResource = await dbContext.DemoAzureResources.Where(dar =>
                        dar.DemoId == demoId
                       && (dar.AttemptCount < 10)
                       && (!dar.LockedUntil.HasValue)
                    )
                    .OrderBy(i => i.RequestedAt)
                    .FirstOrDefaultAsync();

            if (demoAzureResource != null)
            {
                try
                {
                    demoAzureResource.LockedUntil = DateTime.UtcNow + DefaultLockValue;
                    demoAzureResource.AttemptCount++;
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                    dbContext.RevertChanges(dbContext.Entry(demoAzureResource));
                    throw;
                }
            }

            return demoAzureResource;
        }

        public async Task<List<DemoUserResource>> GetDemoUserResources(int demoId, int userId)
        {
            return await dbContext.DemoUserResources
                .Include(dr => dr.Type)
                .Where(de => de.UserId == userId && de.DemoId == demoId)
                .ToListAsync();
        }

        public async Task<List<DemoAzureResource>> GetExpiredAzureDemoResources()
        {
            return await dbContext
                .DemoAzureResources
                .Where(r => r.LockedUntil < DateTime.Now)
                .ToListAsync();
        }

        public async Task SetUserRDPLog(string userAccountName, int demoId)
        {
            var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.AccountName.ToLower() == userAccountName.ToLower());

            var userRdp = await dbContext.UserRDPLogs
                    .FirstOrDefaultAsync(u => u.UserId == user.Id);

            if (userRdp == null)
            {
                dbContext.UserRDPLogs.Add(new UserRDPLog() { UserId = user.Id, DemoId = demoId, UserAccount = user.AccountName });
            }

            else
            {
                userRdp.DemoId = demoId;
            }

            await dbContext.SaveChangesAsync();
        }

    }
}
