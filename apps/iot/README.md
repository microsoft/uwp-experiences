---
topic: sample
languages:
- csharp
products:
- windows
- windows-uwp
---

<!---
  category: ControlsLayoutAndText NetworkingAndWebServices
  language: cs
  keywords: xbox mobile desktop iot azure iot-hub azure-event-hub azure-stream-analytics
-->

# The IoT Experience

Best For You is a sample fitness UWP app that uses Windows IoT Core and Azure Iot Hub to collect data from a fictional IoT enabled yoga clothes and presenting it to the user in a meaningful and helpful way on all of their devices to track progress of exercise and health. 

![BestForYou](http://i.imgur.com/PdDt7lR.png)

**Features:** | Windows IoT Core | Azure IoT Hub | Azure Event Hub | Azure Stream Analytics
---|---|---|---|---

The solution contains two client side projects and several Azure services:

1. UWP client app designed for Xbox One, Desktop, and Mobile
2. UWP IoT app intended to run on an IoT device such as the Raspberry Pi 3 (it can also run on any other UWP device for demo purposes)
3. Azure IoT Hub, Azure Event Hub, Azure Stream Analytics and more

![Architecture](http://i.imgur.com/a1I5wBg.png)

## Setup

### Cloud
To deploy the Best For Me solution to your Azure subscription you will need to follow the below instructions. We are using Azure Resource Manager to deploy the needed services and connect them to one another. We are also using the Azure cross platform CLI tool which will allow you to deploy the services from your favorite development machine, running Windows, Linux or OSX. The below services will be deployed in your Azure subscription:
* 1 instance of Azure IoT Hub (using the SKU of your choice, considering you can only deploy 1 instance of the free SKU per subscription)
* 1 Storage account (Standard performance)
* 1 Service Bus instance (Basic tier) with 1 Event Hub (1 throughput Unit)
* 1 Stream Analytics Job (1 streaming unit)

The simplest way is to click on the button below and follow instructions. If you prefer command line tools, or to learn more, checkout the [ConnectTheDots project](https://github.com/Azure/connectthedots).

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/?repository=https://github.com/Microsoft/AppDevXbox/tree/BestForYou_iot_app/ARMTemplate)


### Client
1. Add IoT Hub Connected Service to the HeartRateDevice project and connect it to the IoT Hub service you created in the previous section
    * Make sure Visual Studio is closed and install the [Connected Service for Azure IoT Hub](https://visualstudiogallery.msdn.microsoft.com/e254a3a5-d72e-488e-9bd3-8fee8e0cd1d6)
    * Launch Visual Studio, right click on the *HeartRateDevice* project and click on **Add Connected Service**
    * Choose Azure IoT Hub and click configure
    * Choose **Hardcore shared access key in application's code**
    * Make sure you are logged in to Visual Studio with the same account associated with the Azure subscription and select the IoT Hub project you created in the previous section
    * Chose a device to connect - most likely you will need to create one
    * Click OK

2.  In the HeartRateDevice application, change the SendDeviceToCloudMessageAsync method in the *AzureIoTHub.cs* file to the following:

   ```csharp
   public static async Task SendDeviceToCloudMessageAsync(double heartBeat)
   {
      var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);
      var message = new Message(Encoding.ASCII.GetBytes(heartBeat.ToString()));
      await deviceClient.SendEventAsync(message);
   }
   ```

   That's it for the HeartRateDevice, feel free to run it on your desktop and play around with the slider to send the data to Azure IoT Hub. If you want to run it on a real IoT Device, you can run it on a Raspberry Pi 3. We used the [FEZ HAT](https://www.ghielectronics.com/catalog/product/500) to simulate heart rate by pressing one of the buttons. For more info on connecting your app to Azure IoT Hub, check out [this blog post](https://blogs.windows.com/buildingapps/2016/03/03/connect-your-windows-app-to-azure-iot-hub-with-visual-studio/) 

3. Next, you will need to add the correct strings in the *Keys.cs* source file in the Yoga project so it can receive the data from the HeartRateDevice through the IoT Hub and Event Hub.
    * **EventHubCompatibleEndpoint** and **EventHubCompatibleName** are found in the Azure Portal under the Event Hub -> Messaging section
    * **IoTHubAccessPolicyName** and **IoTHubAccessPolicyPrimaryKey** are found in the Azure Portal under the IoT Hub -> Settings -> Shared Policies

    The project uses the [Azure SB Lite](https://github.com/ppatierno/azuresblite) library to receive the IoT Hub data. Visit [this blog post](https://paolopatierno.wordpress.com/2015/11/02/azure-iot-hub-get-telemetry-data-using-amqp-stack-and-azure-sb-lite/) to learn more about it.

4. Run

***

## Next Steps ##
<!--- #### - Download the sample from the Windows Store. --->

#### - Read the [blog post](https://blogs.windows.com/buildingapps/2016/10/13/internet-of-things-on-the-xbox-app-dev-on-xbox-series)

#### - Download the source by clicking on **Clone or download** above

#### - View the [one minute dev video](https://channel9.msdn.com/Blogs/One-Dev-Minute/Creating-IoT-apps-for-the-Xbox)
