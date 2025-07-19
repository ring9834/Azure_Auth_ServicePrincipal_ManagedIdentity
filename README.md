# C# Usage of Service Principle, Managed Identity(System-Designed Identity, User-Assigned Idendity)
This conent is mainly to introduce how to use Service Principle, Managed Identity in C# to access Azure Resource such as the APIs deployed in Azure App Service.

Most of the time, it's a little bit confusing for new learners to differenciate the concepts and usage of Service Principle, System-Designed Identity, User-Assigned Idendity, the code, with many comments in the code, I provided here is aimed to assist people in better understanding how to use them more easily in real world practices.

## Differences between Service Principle, System-Designed Identity, and User-Assigned Idendity
![Differences](https://lh3.googleusercontent.com/pw/AP1GczNeiJJP4umW-ufGg9TvOfGQYRg5MBBOU4doIPNTrywJJ8Djox22Ip_KZDWaYILoHBOKV-iAX8tsX4QU1GUv8XhIxfIMwiT0mvChtUFgnzOBW9b0TjkIGBU8RwDjSu7T3g6hXSht7w1glFPnhScfvBU=w1165-h548-s-no-gm?authuser=0)

### Key Uses of Service Principal
Authentication for Applications: Non-Interactive Access - Service Principals allow applications or scripts to authenticate to Azure without requiring user interaction. This is crucial for automated processes, background jobs, or services running without human intervention.

Role-Based Access Control (RBAC): Fine-Grained Permissions - You can assign specific roles to a Service Principal, which dictates what actions it can perform on Azure resources. This helps in implementing the principle of least privilege, ensuring applications only have the necessary permissions.

Security and Isolation: Separate Identity - Using a Service Principal means you're not using personal user accounts for automation, which enhances security by not exposing personal credentials and helps in managing access lifecycle (e.g., revoking access when no longer needed).

Consistency in Automation: Stable Credentials - Service Principals can be given long-lived credentials (like certificates or secrets) which don't change as often as user passwords might, providing stability for automated processes.

Multi-Tenant Applications: Cross-Tenant Authentication - For applications that need to work across multiple Azure AD tenants, Service Principals can be configured to operate within or across tenants, facilitating multi-tenant scenarios.

CI/CD Pipelines: Integration with DevOps - They are widely used in continuous integration/continuous deployment (CI/CD) pipelines to automate interactions with Azure resources, from deploying applications to managing infrastructure. In DevOps workflows, service principals are often used to authenticate and authorize tasks within CI/CD pipelines, like deploying code or managing resources in Azure, while maintaining a high level of security and control.

### Components of a Service Principal
Application ID (Client ID): Unique identifier for the application or service principal.
Tenant ID: The Azure AD tenant where the service principal resides.
Client Secret or Certificate: Used to authenticate the service principal.
Role/Permissions: Access rights assigned to the service principal.

### Create a Service Principal in Azure Portal
In Azure Portal, create an App Registration in Azure Entra ID.

Generate a Client Secret for the application. In the App registrations page, find your newly created application; Go to Certificates & secrets on the left side; Under the Client secrets section, click + New client secret; Provide a description and choose an expiration (1 year, 2 years, never, etc.); Click Add.

Record the Application (Client) ID, Directory (Tenant) ID, and Client Secret.

***Assign Roles/Permissions to the Service Principal***
After creating the service principal, you must assign appropriate roles to it so it can access the required Azure resources. Navigate to the Azure resource (such as an Azure Storage Account or Key Vault). Go to Access Control (IAM). Click + Add > Add Role Assignment. Select the appropriate Role (such as Contributor, Reader). In the Select field, search for your Service Principal and assign the role.

### Authenticate using the Service Principal in C#
Below is the C# code to authenticate using the Service Principal and use it to interact with an Azure Blob Storage.
```sh
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace ServicePrincipalExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Service Principal details
            string tenantId = "<TenantID>";
            string clientId = "<ClientID>";
            string clientSecret = "<ClientSecret>";

            // Blob Storage details
            string storageAccountName = "<StorageAccountName>";
            string containerName = "<ContainerName>";
            string blobName = "<BlobName>";

            // Authenticate using the Service Principal
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // Define the BlobServiceClient using the credentials
            string blobServiceUri = $"https://{storageAccountName}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobServiceUri), credential);

            // Get the container client
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // List the blobs in the container
            Console.WriteLine("Listing blobs:");
            await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
            {
                Console.WriteLine($"- {blobItem.Name}");
            }

            // Download a specific blob (example)
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var download = await blobClient.DownloadToAsync("downloaded_file.txt");
            Console.WriteLine($"Downloaded blob to 'downloaded_file.txt'");
        }
    }
}
```