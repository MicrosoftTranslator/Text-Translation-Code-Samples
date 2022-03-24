using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Text_Translation_AAD
{
    /// <summary>
    /// Client to call Cognitive Services Azure Active directory Token service in order to get an access token.
    /// </summary>
    public class AADToken
    {
        // Gets the TenantID 
        public string TenantId { get; private set; } = string.Empty;

        // Gets the AADApplicationSecret 
        public string AADApplicationSecret { get; private set; } = string.Empty;

        // Gets the AuthorityUrl 
        public string AuthorityUrl { get; private set; } = string.Empty;

        // Gets the ResourceUrl 
        public string ResourceUrl { get; private set; } = string.Empty;

        // Gets the AADApplicationId 
        public string AADApplicationId { get; private set; } = string.Empty;

        // Gets the HTTP status code for the most recent request to the token service.
        public HttpStatusCode RequestStatusCode { get; private set; }

        /// <summary>
        /// <param name="tenantId">The Azure Active Directory tenant (directory) Id of the service principal.</param>
        /// <param name="authorityUrl">Windows auth URL</param>
        /// <param name="resourceUrl">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="aadApplicationId">AAD application Id requesting the token.</param>
        /// <param name="aadApplicationSecret">Secret of the AAD application Id requesting the token.</param>
        /// </summary>
        public AADToken(string tenantId, string authorityUrl, string resourceUrl, string aadApplicationId, string aadApplicationSecret)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentNullException("tenantID", "TenantID is required");
            }
            if (string.IsNullOrEmpty(aadApplicationSecret))
            {
                throw new ArgumentNullException("aadApplicationSecret", "AADApplicationSecret is required");
            }
            if (string.IsNullOrEmpty(authorityUrl))
            {
                throw new ArgumentNullException("authorityUrl", "AuthorityUrl is required");
            }
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new ArgumentNullException("resourceUrl", "ResourceUrl is required");
            }
            if (string.IsNullOrEmpty(aadApplicationId))
            {
                throw new ArgumentNullException("aadApplicationId", "AADApplicationId is required");
            }

            this.TenantId = tenantId;
            this.AADApplicationSecret = aadApplicationSecret;
            this.AuthorityUrl = authorityUrl + this.TenantId;
            this.ResourceUrl = resourceUrl;
            this.AADApplicationId = aadApplicationId;            
            this.RequestStatusCode = HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Get AAD token
        /// </summary>        
        public async Task<string> GetAADAccessToken()
        {
            var app = ConfidentialClientApplicationBuilder.Create(this.AADApplicationId)
                .WithTenantId(this.TenantId)
                .WithAuthority(this.AuthorityUrl)
                .WithClientSecret(this.AADApplicationSecret)
                .Build();

            var scopes = new List<String>() { this.ResourceUrl };
            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);
            var accessToken = authResult.AccessToken;

            return $"Bearer {accessToken}";
        }
    }
}
