// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.EventHubs.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

namespace ManageEventHub
{
    /**
     * Azure Event Hub sample for managing event hub -
     *   - Create an event hub namespace
     *   - Create an event hub in the namespace with data capture enabled along with a consumer group and rule
     *   - List consumer groups in the event hub
     *   - Create a second event hub in the namespace
     *   - Create a consumer group in the second event hub
     *   - List consumer groups in the second event hub
     *   - Create an event hub namespace along with event hub.
     */
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string rgName = Utilities.CreateRandomName("rgeh");
            string namespaceName1 = Utilities.CreateRandomName("ns");
            string storageAccountName = Utilities.CreateRandomName("stg");
            string eventHubName1 = Utilities.CreateRandomName("eh");
            string eventHubName2 = Utilities.CreateRandomName("eh");

            try
            {
                //============================================================
                // Create a resource group
                //
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();
                ResourceGroupData resourceGroupData = new ResourceGroupData(region);
                ResourceGroupResource resourceGroup = (await subscription.GetResourceGroups()
                    .CreateOrUpdateAsync(WaitUntil.Completed, rgName, resourceGroupData)).Value;
                _resourceGroupId = resourceGroup.Id;

                //============================================================
                // Create an event hub namespace
                //
                Utilities.Log("Creating a namespace");

                EventHubsNamespaceData eventHubsNamespaceData = new EventHubsNamespaceData(region);
                EventHubsNamespaceResource namespace1 = (await resourceGroup.GetEventHubsNamespaces()
                    .CreateOrUpdateAsync(WaitUntil.Completed, namespaceName1, eventHubsNamespaceData)).Value;

                Utilities.Log($"Created a namespace with Id: {namespace1.Id}");

                //============================================================
                // Create an event hub in the namespace with data capture enabled
                //
                StorageAccountCreateOrUpdateContent storageAccountData = new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.StorageV2,
                    AzureLocation.EastUS2);
                StorageAccountResource storageAccountCreatable = (await resourceGroup.GetStorageAccounts()
                    .CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, storageAccountData)).Value;
                var blob = (await storageAccountCreatable.GetBlobService().GetAsync()).Value;
                var container = (await blob.GetBlobContainers().CreateOrUpdateAsync(WaitUntil.Completed, "testname", new BlobContainerData())).Value;

                Utilities.Log("Creating an event hub with data capture enabled");

                EventHubData eventHubData = new EventHubData()
                {
                    // Optional - configure data capture
                    CaptureDescription = new CaptureDescription()
                    {
                        Enabled = true,
                        Encoding = EncodingCaptureDescription.Avro,
                        Destination = new EventHubDestination()
                        {
                            Name = "EventHubArchive.AzureBlockBlob",
                            StorageAccountResourceId = storageAccountCreatable.Id,
                            BlobContainer = "testname"
                        }
                    }
                };
                EventHubResource eventHub1 = (await namespace1.GetEventHubs()
                    .CreateOrUpdateAsync(WaitUntil.Completed, eventHubName1, eventHubData)).Value;

                Utilities.Log($"Created an event hub with Id: {eventHub1.Id}");

                //============================================================
                // Create a consumer group in the event hub
                //
                EventHubsConsumerGroupData eventHubsConsumerGroupData = new EventHubsConsumerGroupData() { UserMetadata = "sometadata" };
                EventHubsConsumerGroupResource eventHubsConsumerGroup = (await eventHub1.GetEventHubsConsumerGroups()
                    .CreateOrUpdateAsync(WaitUntil.Completed, "cg1", eventHubsConsumerGroupData)).Value;

                // Optional - create an authorization rule for event hub
                var rules = new EventHubsAuthorizationRuleData()
                {
                    Rights = { EventHubsAccessRight.Listen }
                };
                EventHubAuthorizationRuleResource eventHubAuthorizationRule = (await eventHub1.GetEventHubAuthorizationRules()
                    .CreateOrUpdateAsync(WaitUntil.Completed, "listenrule1", rules)).Value;

                //============================================================
                // Retrieve consumer groups in the event hub
                //
                Utilities.Log("Retrieving consumer groups");

                var consumerGroups = eventHub1.GetEventHubsConsumerGroups().GetAllAsync();

                Utilities.Log("Retrieved consumer groups");
                await foreach (var group in consumerGroups)
                {
                    Utilities.Log(group.Data.Name);
                }

                //============================================================
                // Create a second event hub in same namespace
                //

                Utilities.Log("Creating a second event hub in same namespace");

                EventHubResource eventHub2 = (await namespace1.GetEventHubs()
                    .CreateOrUpdateAsync(WaitUntil.Completed, eventHubName2, new EventHubData())).Value;

                Utilities.Log($"Created an event hub with Id: {eventHub2.Id}");

                await foreach (var eh in namespace1.GetEventHubs().GetAllAsync())
                {
                    Utilities.Log(eh.Data.Name);
                }
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
                    Utilities.Log(ex);
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
                Utilities.Log(ex);
            }
        }
    }
}
