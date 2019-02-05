---
topic: sample
languages:
- csharp
products:
- windows
- windows-uwp
---

<!---
  category: ControlsLayoutAndText CustomUserInteractions GraphicsAndAnimation Inking Touch TilesToastAndNotifications
  language: cs
  keywords: surface studio desktop composition inking uwp community toolkit cortana dial XAML
-->

# Northwind Photo

The Northwind Photo application was used at the Windows Developer Day keynote to demonstrate some of the great features of the Creators Update.

**Features:** | Pin To Start | Composition | Dial | Inking | XAML Composition Brush | Toast and Notifications
---|---|---|---|---|---|---

# Setup

## Requirements
* [Visual Studio 2017](https://www.visualstudio.com/vs/visual-studio-2017-rc/)
* [Windows Insider Build](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewiso)
* [Windows SDK Insider Preview](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewSDK )

## Code navigation
To help you find the code which was used in the demo, **//Todo** comments have been put in place. To quickly navigate to the comments open the Task List. To open the task list in Visual Studio, from the View Menu select Task List. 

Note: As part of the demo, a Surface Studio was used with a Dial. If you do not have this hardware setup, then you can click on the effects rings to perform the image manipulation. The Dial was also used to perform shape recognition, in this case, you can right click to achieve the same result. A Cortana skill was demonstrated which was deployed to a local account; you can use keyboard shortcuts to achieve the same effect. After collage creation and shape recognition, press 'P' to populate the images, press 'L' to change the left picture.

## Constants.cs
To demo posting to twitter, you will need to populate the Model/Constants.cs file for the following services:

### 1. Twitter

The application uses the [UWP Community Toolkit](https://github.com/Microsoft/UWPCommunityToolkit) to tweet an image. The Twitter Service requires authentication using a consumer key, consumer secret, and callback URI. Follow the steps [here](https://apps.twitter.com/app/new) to create a new Twitter app and paste the keys into Constants.cs. Visit the [UWP Community Toolkit documentation](https://developer.microsoft.com/en-us/windows/uwp-community-toolkit/services/twitter.htm) for more details.


## Next Steps ##

#### - Download the source by clicking on **Clone or download** above

#### - View the demo video [31 minutes 30 seconds](https://developer.microsoft.com/en-us/windows/projects/campaigns/windows-developer-day)
