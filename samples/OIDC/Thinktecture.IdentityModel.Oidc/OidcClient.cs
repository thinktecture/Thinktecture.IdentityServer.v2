﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityModel.Tokens;

namespace Thinktecture.IdentityModel.Oidc
{
    public class OidcClient
    {
        public static Uri GetRedirectToProviderUrl(Uri authorizeEndpoint, Uri redirectUri, string clientId, string scopes, string state, string responseType = "code")
        {
            var queryString = string.Format("?client_id={0}&scope={1}&redirect_uri={2}&state={3}&response_type={4}",
                clientId,
                scopes,
                redirectUri,
                state,
                responseType);

            return new Uri(authorizeEndpoint.AbsoluteUri + queryString);
        }

        public static OidcAuthorizeResponse HandleOidcAuthorizeResponse(NameValueCollection query)
        {
            var response = new OidcAuthorizeResponse
            {
                Error = query["error"],
                Code = query["code"],
                State = query["state"]
            };

            response.IsError = !string.IsNullOrWhiteSpace(response.Error);
            return response;
        }

        public static OidcTokenResponse CallTokenEndpoint(Uri tokenEndpoint, Uri redirectUri, string code, string clientId, string clientSecret)
        {
            var client = new HttpClient
            {
                BaseAddress = tokenEndpoint
            };

            client.SetBasicAuthentication(clientId, clientSecret);

            var parameter = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri.AbsoluteUri }
                };

            var response = client.PostAsync("", new FormUrlEncodedContent(parameter)).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException("error calling token endpoint");
            }

            var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return json.ToObject<OidcTokenResponse>();
        }

        public static ClaimsPrincipal ValidateIdentityToken(string token, string issuer, string audience, X509Certificate2 signingCertificate)
        {
            var idToken = new JwtSecurityToken(token);
            var handler = new JwtSecurityTokenHandler();
            
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                AllowedAudience = audience,
                SigningToken = new X509SecurityToken(signingCertificate)
            };

            var ids = handler.ValidateToken(token, parameters);
            return new ClaimsPrincipal(ids);
        }

        public static string GetUserInfo(Uri userInfoEndpoint, string accessToken)
        {
            throw new NotImplementedException();
        }

    }
}
