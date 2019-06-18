---
services: Event-Hub
platforms: dotnet
author: yaohaizh
---

# Getting started on managing event hub and associated resources using C# #

      Azure Event Hub sample for managing event hub -
        - Create an event hub namespace
        - Create an event hub in the namespace with data capture enabled along with a consumer group and rule
        - List consumer groups in the event hub
        - Create a second event hub in the namespace
        - Create a consumer group in the second event hub
        - List consumer groups in the second event hub
        - Create an event hub namespace along with event hub.


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/eventhub-dotnet-manage-event-hub.git

    cd eventhub-dotnet-manage-event-hub
  
    dotnet build
    
    bin\Debug\net452\ManageEventHub.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.