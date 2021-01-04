using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class AadObject
    {
        public string odatacontext { get; set; }
        public string odatatype { get; set; }
        public string Id { get; set; }
        public object DeletedDateTime { get; set; }
        public bool AccountEnabled { get; set; }
        public string AppDisplayName { get; set; }
        public string AppId { get; set; }
        public object ApplicationTemplateId { get; set; }
        public string AppOwnerOrganizationId { get; set; }
        public bool AppRoleAssignmentRequired { get; set; }
        public string DisplayName { get; set; }
        public object ErrorUrl { get; set; }
        public object Homepage { get; set; }
        public AadObjectInfo Info { get; set; }
        public object LogoutUrl { get; set; }
        public object[] NotificationEmailAddresses { get; set; }
        public object[] PublishedPermissionScopes { get; set; }
        public object PreferredSingleSignOnMode { get; set; }
        public object PreferredTokenSigningKeyEndDateTime { get; set; }
        public object PreferredTokenSigningKeyThumbprint { get; set; }
        public string PublisherName { get; set; }
        public object[] ReplyUrls { get; set; }
        public object SamlMetadataUrl { get; set; }
        public object SamlSingleSignOnSettings { get; set; }
        public string[] ServicePrincipalNames { get; set; }
        public string SignInAudience { get; set; }
        public string[] Tags { get; set; }
        public object[] AddIns { get; set; }
        public object[] AppRoles { get; set; }
        public object[] KeyCredentials { get; set; }

        public List<AadObjectPasswordcredential> PasswordCredentials { get; set; }
    }

    public class AadObjectInfo
    {
        public object TermsOfServiceUrl { get; set; }
        public object SupportUrl { get; set; }
        public object PrivacyStatementUrl { get; set; }
        public object MarketingUrl { get; set; }
        public object LogoUrl { get; set; }
    }

    public class AadObjectPasswordcredential
    {
        public string CustomKeyIdentifier { get; set; }
        public DateTime EndDateTime { get; set; }
        public string KeyId { get; set; }
        public DateTime StartDateTime { get; set; }
        public object SecretText { get; set; }
        public object Hint { get; set; }
        public object DisplayName { get; set; }
    }
}