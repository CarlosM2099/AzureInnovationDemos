using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureInnovationDemos.Extensions
{
    public class KnownClaimTypes
    {
        public static class ApplicationClaims
        {
            
            public const string RoleLevel = "https://claims.demos.microsoft.com/user/permission-level";
            public const string Partner = "https://claims.demos.microsoft.com/user/ms-partner";
            public const string Email = "https://claims.demos.microsoft.com/user/email";
            public const string Id = "https://claims.demos.microsoft.com/user/Id";
            public const string UserCorrelationId = "https://claims.demos.microsoft.com/user/UserCorrelationId";
            public const string Permission = "https://claims.demos.microsoft.com/user/permissions";
            public const string SessionId = "https://claims.demos.microsoft.com/user/session-id";
        }

        public static class MicrosoftRpsClaims
        {
            public const string SignInName = "urn:microsoftaccount:signinname";
            public const string PUID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            public const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
            public const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
            public const string DisplayName = "urn:microsoftaccount:displaymembername";
            public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        }

        public static class MicrosoftAccountClaims
        {
            public const string MicrosoftAccountId = "urn:microsoftaccount:id";
            public const string MicrosoftAccountName = "urn:microsoftaccount:name";
        }

        public static class AzureOpenIdConnectClaims
        {
            public const string JwtName = "name";
            public const string JwtPreferredUsername = "preferred_username";
            public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
            public const string Identifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            
        }
    }
}