using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;

namespace Azure_Auth
{
    public class ApiCall_via_UserAssignedIdentity_AzureIdentity
    {
        // Configuration settings - replace with your values
        private const string apiUrl = "https://yourappservice.azurewebsites.net/api/your-endpoint";
        private const string scope = "api://your-api-app-id/.default";
        private const string managedIdentityClientId = "your-user-assigned-managed-identity-client-id";

        public static async Task CallApi_via_UserAssignedIdentity()
        {
            try
            {
                // Creates an authentication credential using DefaultAzureCredential from the Azure.Identity library, configured to use a user-assigned managed identity
                // DefaultAzureCredential is a versatile class that attempts multiple authentication methods in a specific order (managed identity, environment variables, Azure CLI).
                // Here, it's configured to use a user-assigned managed identity by specifying the ManagedIdentityClientId.
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    // managedIdentityClientId is a variable containing the Client ID of the user - assigned managed identity.
                    // This ID is obtained from the Azure portal when the managed identity is created.
                    ManagedIdentityClientId = managedIdentityClientId
                    // The user-assigned managed identity must be assigned to the Azure resource (such as App Service, VM) running this code and must have permissions to access the target API.
                    // Unlike a system-assigned managed identity (tied to a single resource), a user-assigned managed identity is a standalone Azure resource that can be assigned to
                    // multiple resources, providing flexibility for authentication.
                });

                // Acquire access token for the API
                // TokenRequestContext specifies the scope for which the token is requested.
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
