using System;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace Azure_Auth
{
    public class ApiCall_via_Service_Principal_MSALandAzureIdentity
    {
        // A client-side app(like desktop app, or mobile app) call an API deployed in Azure App Service after acquiring an access token
        public static async Task CallApi_via_ServicePrincipal_using_MSAL_and_AzureIdentity_Library()
        {
            // Specifies the scope for the access token, which defines the permissions the token grants;
            // The .default suffix requests all permissions configured for the application in Azure AD for this API.
            // For example, if the API exposes permissions like read or write, the token will include those permissions.
            string[] scopes = new string[] { "https://storage.azure.com/.default" };

            // The unique identifier of the application registered in Azure AD.
            var clientId = "your_client_id";
            // The ID of the Azure AD tenant (directory) where the application and API are registered.
            var tenantId = "your_tenant_id";
            // The URL of the Azure Key Vault where the client secret is stored.
            var keyVaultUrl = "https://your-keyvault-name.vault.azure.net/";
            var secretName = "your-secret-name";

            // Create a credential using Azure.Identity(DefaultAzureCredential)
            // DefaultAzureCredential automatically tries multiple authentication methods in a predefined
            // order (environment variables, managed identity, Visual Studio, Azure CLI, interactive login).
            // This eliminates the need to hardcode or manually manage credentials like client ID, client secret, or certificates for accessing Azure Key Vault.
            var credential = new DefaultAzureCredential();

            // Initialize SecretClient to access Key Vault
            var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

            // Retrieve the client secret from Key Vault
            KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
            var clientSecret = secret.Value;

            // Create a confidential client application instance using MSAL (Microsoft Authentication Library).
            // This is used for server-side applications or daemon apps that need to authenticate without user interaction.
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                // Specifies the client secret, marking this as a confidential client, such as a web app or service, not a public client like a desktop app.
                .WithClientSecret(clientSecret)
                // Specifies the authority, which is the Azure AD endpoint for authentication.
                // The authority URL includes the tenant ID, which restricts authentication to that specific Azure AD tenant.
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            try
            {
                // Requests an access token using the client credentials flow, which authenticates the application itself without user interaction.
                AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                var accessToken = result.AccessToken;

                // Use this token in your requests to Azure resources
                // For example, in HTTP headers:
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                // Make your API call here.
                // A placeholder for the API endpoint URL (for example, https://<app-name>.azurewebsites.net/api/endpoint).
                // This should be replaced with the actual URL of the API
                var requestURI = "<the address of one api deployed in Azure App Service>";
                var response = await client.GetAsync(requestURI);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            catch (MsalServiceException ex)
            {
                Console.WriteLine($"Error acquiring access token: {ex.Message}");
            }
        }
    }
}