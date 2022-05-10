// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

namespace ManageResource
{
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId;
        /**
        * Azure Resource sample for managing resources -
        * - Create a resource
        * - Update a resource
        * - Create another resource
        * - List resources
        * - Delete a resource.
        */
        public static async Task RunSample(ArmClient client)
        {
            // change the values here for your own resource names
            var resourceGroupName = "rgRSMR";
            var resourceName1 = "rn1";
            var resourceName2 = "rn2";

            try
            {
                //=============================================================
                // Create resource group.

                Console.WriteLine($"Creating a resource group with name: {resourceGroupName}");

                var subscription = await client.GetDefaultSubscriptionAsync();
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, resourceGroupName, new ResourceGroupData(AzureLocation.WestUS));

                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;

                Console.WriteLine($"Created a resource group: {_resourceGroupId}");

                //=============================================================
                // Create storage account.

                Console.WriteLine($"Creating a storage account with name: {resourceName1}");

                var storageAccountLro = await resourceGroup.GetStorageAccounts().CreateOrUpdateAsync(WaitUntil.Completed, resourceName1, new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLRS), StorageKind.Storage, AzureLocation.WestUS));
                var storageAccount = storageAccountLro.Value;

                Console.WriteLine($"Storage account created: {storageAccount.Id}");

                //=============================================================
                // Update - set the sku name

                Console.WriteLine($"Updating the storage account {storageAccount.Id}");

                storageAccount = await storageAccount.UpdateAsync(new StorageAccountPatch()
                {
                    Sku = new StorageSku(StorageSkuName.StandardRagrs),
                });

                Console.WriteLine($"Updated the storage account {storageAccount.Id}");

                //=============================================================
                // Create another storage account.

                Console.WriteLine($"Creating another storage account with name: {resourceName2}");

                storageAccountLro = await resourceGroup.GetStorageAccounts().CreateOrUpdateAsync(WaitUntil.Completed, resourceName2, new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardGRS), StorageKind.Storage, AzureLocation.WestUS));
                var storageAccount2 = storageAccountLro.Value;

                Console.WriteLine($"Storage account created: {storageAccount2.Id}");

                //=============================================================
                // List storage accounts.

                Console.WriteLine("Listing all storage accounts for resource group: " + resourceGroupName);

                foreach (var account in resourceGroup.GetStorageAccounts())
                {
                    Console.WriteLine($"Storage account: {account.Id}");
                }

                //=============================================================
                // Delete a storage accounts.

                Console.WriteLine($"Deleting storage account: {storageAccount2.Id}");

                await storageAccount2.DeleteAsync(WaitUntil.Completed);

                Console.WriteLine($"Deleted storage account: {storageAccount2.Id}");
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Console.WriteLine($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Console.WriteLine($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static async Task Main(string[] args)
        {

            try
            {
                //=================================================================
                // Authenticate
                var credential = new DefaultAzureCredential();

                var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
                // you can also use `new ArmClient(credential)` here, and the default subscription will be the first subscription in your list of subscription
                var client = new ArmClient(credential, subscriptionId);

                await RunSample(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}