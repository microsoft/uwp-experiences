<!---
  category: ControlsLayoutAndText DeepLinksAndAppToAppCommunication GraphicsAndAnimation
  language: cs 
  keywords: xbox mobile desktop unity extensions services
-->

# The Weather Experience

Atmosphere is is a sample weather app that showcases the use of the popular Unity Engine in a normal Universal Windows Platform (UWP) application.

![Atmosphere](http://i.imgur.com/HlAcl4A.gif)

**Features:** | Unity and XAML interop | App Services | App Extensions
---|---|---|---

The app renders a 3D layer below a 2D XAML layer. The 3D world renders the weather with special effects like rain, thunder, clouds and so on for each day, and then the XAML User Interface (UI) layer communicates the information to the user. The app also showcases the use of app extensions that enable other developers to extend certain areas of the app – in this case, the extension provides music that matches the mood of the current weather. In addition, the weather service used by the app is exposed through an app service that enables other apps to use that weather information, as illustrated by [Fourth Coffee](https://github.com/Microsoft/AppDevXbox/tree/news).

## Steps for running the Unity Project
1. Open the project in Unity
2. Open the asset store in Unity and download the free Water FX Particles asset: https://www.assetstore.unity3d.com/en/#!/content/48580
3. Open the asset store in Unity and download the free Low Poly Nature Pack (Lite) asset: https://www.assetstore.unity3d.com/en/#!/content/40444
3. Run the project in the Unity Editor.

## Steps for Building the UWP App in Visual Studio 2015
1. You need to have Unity 3D installed. You can download it for free from [Unity3d.com](http://unity3d.com)
2. If you don't install Unity to the default location, update the apps/Atomosphere.UWP/UnityCommon.props file with the correct path to your Unity installation.
3. Open the apps/Atomosphere.UWP/Atmosphere.sln solution in Visual Studio 2015 - Make sure the Build Configuration is set to x86 and Master build.
4. Build and Run!

***

## Next Steps ##
<!--- #### - Download the sample from the Windows Store. --->

#### - Read the [blog post](https://blogs.windows.com/buildingapps/2016/09/15/unity-interop-and-app-extensibility-app-dev-on-xbox-series)

#### - Download the source by clicking on **Clone or download** above

#### - View the [one minute dev video](https://channel9.msdn.com/Blogs/One-Dev-Minute/Creating-UWP-Apps-for-Xbox-with-Unity)
