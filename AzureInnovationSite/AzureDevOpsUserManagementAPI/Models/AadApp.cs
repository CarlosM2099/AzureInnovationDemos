using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class AadAppList
    {
        public string odatacontext { get; set; }
        public List<AadApp> Value { get; set; }
    }

    public class AadApp
    {
        public string Id { get; set; }
        public object DeletedDateTime { get; set; }
        public object IsFallbackPublicClient { get; set; }
        public string AppId { get; set; }
        public object ApplicationTemplateId { get; set; }
        public object[] IdentifierUris { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string DisplayName { get; set; }
        public object IsDeviceOnlyAuthSupported { get; set; }
        public object GroupMembershipClaims { get; set; }
        public object OptionalClaims { get; set; }
        public object[] OrgRestrictions { get; set; }
        public string PublisherDomain { get; set; }
        public string SignInAudience { get; set; }
        public object[] Tags { get; set; }
        public object TokenEncryptionKeyId { get; set; }
        public AadAppApi Api { get; set; }
        public object[] AppRoles { get; set; }
        public PublicClient PublicClient { get; set; }
        public AadAppInfo Info { get; set; }
        public object[] KeyCredentials { get; set; }
        public Parentalcontrolsettings ParentalControlSettings { get; set; }
        public List<AadAppPasswordcredential> PasswordCredentials { get; set; }
        public RequiredResourceAccess[] RequiredResourceAccess { get; set; }
        public AadAppWeb Web { get; set; }
    }

    public class AadAppApi
    {
        public object RequestedAccessTokenVersion { get; set; }
        public object AcceptMappedClaims { get; set; }
        public object[] KnownClientApplications { get; set; }
        public object[] Oauth2PermissionScopes { get; set; }
        public object[] PreAuthorizedApplications { get; set; }
    }

    public class PublicClient
    {
        public object[] RedirectUris { get; set; }
    }

    public class AadAppInfo
    {
        public object TermsOfServiceUrl { get; set; }
        public object SupportUrl { get; set; }
        public object PrivacyStatementUrl { get; set; }
        public object MarketingUrl { get; set; }
        public object LogoUrl { get; set; }
    }

    public class Parentalcontrolsettings
    {
        public object[] CountriesBlockedForMinors { get; set; }
        public string LegalAgeGroupRule { get; set; }
    }

    public class AadAppWeb
    {
        public object[] RedirectUris { get; set; }
        public object HomePageUrl { get; set; }
        public object LogoutUrl { get; set; }
        public Implicitgrantsettings ImplicitGrantSettings { get; set; }
    }

    public class Implicitgrantsettings
    {
        public bool EnableIdTokenIssuance { get; set; }
        public bool EnableAccessTokenIssuance { get; set; }
    }

    public class AadAppPasswordcredential
    {
        public object CustomKeyIdentifier { get; set; }
        public DateTime EndDateTime { get; set; }
        public string KeyId { get; set; }
        public DateTime StartDateTime { get; set; }
        public string SecretText { get; set; }
        public string Hint { get; set; }
        public string DisplayName { get; set; }
    }

    public class RequiredResourceAccess
    {
        public string ResourceAppId { get; set; }
        public ResourceAccess[] ResourceAccess { get; set; }
    }

    public class ResourceAccess
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }
}