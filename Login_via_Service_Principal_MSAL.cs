using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Azure_Auth
{
    public class UserLogin_via_Service_Principal
    {
        // appid for the service principal, a unique identifier for the application registered in Azure AD.
        private const string _clientId = "APPLICATION_CLIENT_ID";
        // tenantId for the service principal, which is the directory where the application is registered.
        private const string _tenantId = "DIRECTORY_TENANT_ID";

        // Authenticate a user using a service principal in Azure Active Directory (Azure AD).
        public static async Task LoginUser_via_ServicePrinciple()
        {
            // Create a PublicClientApplication instance using the client ID and tenant ID.
            // This PublicClientApplication represents a public client application, such as a desktop or mobile app, in MSAL.
            var app = PublicClientApplicationBuilder
                // Initializes the application with a clientId.
                .Create(_clientId)
                // Specifies the Azure AD authority, which is the endpoint used for authentication.
                // AzureCloudInstance.AzurePublic indicates the Microsoft Azure public cloud.
                // Using tenant ID restricts authentication to users in that tenant.
                .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
                // Sets the redirect URI  which is where the authentication response will be sent after the user logs in. This is used in the OAuth 2.0 authorization flow.
                // In this case, it is set to "http://localhost", which is common for local development scenarios.
                .WithRedirectUri("http://localhost")
                // Finalizes the configuration and creates the IPublicClientApplication object (app) used for authentication.
                .Build();

            // Set the permission scope for the token request.
            string[] scopes = { "user.read" };

            // AcquireTokenInteractive method initiates an interactive authentication flow.
            // The user will see a login prompt, such as a Microsoft login page, where they enter their credentials.
            AuthenticationResult result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

            // Extracts the AccessToken (a JSON Web Token, or JWT) from the AuthenticationResult.
            // Console.WriteLine($"Token:\t{result.AccessToken}");
        }
    }
}