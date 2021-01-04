using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class ManagementAzureSiteList
    {
        public List<ManagementAzureSite> Value { get; set; }
    }

    public class ManagementAzureSite
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public string Location { get; set; }
        public ManagementAzureSiteProperties Properties { get; set; }
    }

    public class ManagementAzureSiteProperties
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string[] HostNames { get; set; }
        public string WebSpace { get; set; }
        public string SelfLink { get; set; }
        public string RepositorySiteName { get; set; }
        public object Owner { get; set; }
        public string UsageState { get; set; }
        public bool Enabled { get; set; }
        public bool AdminEnabled { get; set; }
        public string[] EnabledHostNames { get; set; }
        public SiteProperties SiteProperties { get; set; }
        public string AvailabilityState { get; set; }
        public object SslCertificates { get; set; }
        public object[] Csrs { get; set; }
        public object Cers { get; set; }
        public object SiteMode { get; set; }
        public Hostnamesslstate[] HostNameSslStates { get; set; }
        public object ComputeMode { get; set; }
        public object ServerFarm { get; set; }
        public string ServerFarmId { get; set; }
        public bool Reserved { get; set; }
        public bool IsXenon { get; set; }
        public bool HyperV { get; set; }
        public DateTime LastModifiedTimeUtc { get; set; }
        public string StorageRecoveryDefaultState { get; set; }
        public string ContentAvailabilityState { get; set; }
        public string RuntimeAvailabilityState { get; set; }
        public object SiteConfig { get; set; }
        public string DeploymentId { get; set; }
        public object TrafficManagerHostNames { get; set; }
        public string Sku { get; set; }
        public bool ScmSiteAlsoStopped { get; set; }
        public object TargetSwapSlot { get; set; }
        public object HostingEnvironment { get; set; }
        public object HostingEnvironmentProfile { get; set; }
        public bool ClientAffinityEnabled { get; set; }
        public bool ClientCertEnabled { get; set; }
        public object ClientCertExclusionPaths { get; set; }
        public bool HostNamesDisabled { get; set; }
        public object DomainVerificationIdentifiers { get; set; }
        public string Kind { get; set; }
        public string OutboundIpAddresses { get; set; }
        public string PossibleOutboundIpAddresses { get; set; }
        public int ContainerSize { get; set; }
        public int DailyMemoryTimeQuota { get; set; }
        public object SuspendedTill { get; set; }
        public int SiteDisabledReason { get; set; }
        public object FunctionExecutionUnitsCache { get; set; }
        public object MaxNumberOfWorkers { get; set; }
        public string HomeStamp { get; set; }
        public object CloningInfo { get; set; }
        public object HostingEnvironmentId { get; set; }
        public object Tags { get; set; }
        public string ResourceGroup { get; set; }
        public string DefaultHostName { get; set; }
        public object SlotSwapStatus { get; set; }
        public bool HttpsOnly { get; set; }
        public string RedundancyMode { get; set; }
        public object InProgressOperationId { get; set; }
        public object GeoDistributions { get; set; }
    }

    public class SiteProperties
    {
        public object Metadata { get; set; }
        public SiteProperty[] Properties { get; set; }
        public object AppSettings { get; set; }
    }

    public class SiteProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Hostnamesslstate
    {
        public string Name { get; set; }
        public string SslState { get; set; }
        public object IpBasedSslResult { get; set; }
        public object VirtualIP { get; set; }
        public object Thumbprint { get; set; }
        public object ToUpdate { get; set; }
        public object ToUpdateIpBasedSsl { get; set; }
        public string IpBasedSslState { get; set; }
        public string HostType { get; set; }
    }
}