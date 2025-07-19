using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Net.Http.Headers;

namespace Azure_Auth
{
    public class ApiCall_via_Service_Principal_AzureIdentity
    {
        private const string tenantId = "your-tenant-id";
        private const string clientId = "your-service-principal-client-id";
        private const string keyVaultUrl = "https://your-keyvault-name.vault.azure.net/";
        private const string secretName = "your-client-secret-name"; // Name of the secret in Key Vault
        private const string apiUrl = "https://yourappservice.azurewebsites.net/api/your-endpoint";
        private const string scope = "api://your-api-app-id/.default";

        // An app call an API deployed in Azure App Service after acquiring an access token
        // This method uses a service principal with the Azure.Identity library to authenticate and call an API deployed in Azure App Service.
        // The service principal’s client secret is retrieved from Azure Key Vault for enhanced security, avoiding hardcoded credentials.
        public static async Task CallApi_via_ServicePrincipal_using_AzureIdentity_Library()
        {
            try
            {
                // Authenticate to Key Vault using DefaultAzureCredential
                // Creates a client to access Azure Key Vault to retrieve the service principal’s client secret
                var secretClient = new SecretClient(
                    vaultUri: new Uri(keyVaultUrl),
                    credential: new DefaultAzureCredential());

                // Retrieve the client secret from Key Vault
                KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
                string clientSecret = secret.Value;

                // Create credential using service principal
                var credential = new ClientSecretCredential(
                    tenantId: tenantId,
                    clientId: clientId,
                    clientSecret: clientSecret);

                // Acquire access token
                var tokenRequestContext = new TokenRequestContext(new[] { scope });
                var token = await credential.GetTokenAsync(tokenRequestContext);

                // Create HTTP client and set authorization header
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Token);

                // Make API call
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("API Response: " + responseContent);
                }
                else
                {
                    Console.WriteLine($"API call failed: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error details: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
