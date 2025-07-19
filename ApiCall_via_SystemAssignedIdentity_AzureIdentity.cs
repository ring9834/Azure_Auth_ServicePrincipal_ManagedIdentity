using Azure.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Azure_Auth
{
    public class ApiCall_via_SystemAssignedIdentity_AzureIdentity
    {
        // Configuration settings - replace with your values
        private static readonly string apiUrl = "https://yourappservice.azurewebsites.net/api/your-endpoint";
        private static readonly string scope = "api://your-api-app-id/.default";

        public static async Task CallApi_via_SystemAssignedIdentity()
        {
            try
            {
                // Create credential using system-assigned managed identity of the Azure resource running the code.
                var credential = new DefaultAzureCredential();
                // A system-assigned managed identity is automatically created for an Azure resource (such as App Service, Azure Function, VM) when enabled in the resource’s Identity settings.
                // It’s tied to the resource’s lifecycle and doesn’t require a separate client ID.
                // The system-assigned managed identity must have permissions to access the target API’s scope, configured in Azure AD.


                // Acquire access token for the API
                // TokenRequestContext specifies the scope for which the token is requested
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
