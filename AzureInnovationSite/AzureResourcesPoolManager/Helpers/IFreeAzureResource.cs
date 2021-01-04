using AzureInnovationDemosDAL.Models;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Helpers
{
    public interface IFreeAzureResource
    {
        Task FreeExpiredResource(DemoAzureResource demoAzureResource);
    }
}
