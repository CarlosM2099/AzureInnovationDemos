using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace AzureInnovationDemos.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        
        public static string GetDisplayName(this ClaimsIdentity identity)
        {
            return identity.FindFirst("name")?.Value;
        }

        public static string GetAccount(this ClaimsIdentity identity)
        {
            return identity.FindFirst(KnownClaimTypes.MicrosoftAccountClaims.MicrosoftAccountId)?.Value;
        }
        public static void SetClaimValue(this ClaimsIdentity identity, string claimType, string claimValue)
        {
            var claim = identity.FindFirst(claimType);
            if (claim != null) { identity.RemoveClaim(claim); }

            identity.AddClaim(new Claim(claimType, claimValue));
        }
    }
}