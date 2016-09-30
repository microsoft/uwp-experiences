# South Ridge Video (video app)

[![SouthRidge](http://imgur.com/3lSmSJd.png)](https://www.youtube.com/watch?v=6kv3qyCdc1E)

South Ridge Video, an open source video app developed as a hosted web application built with React.js, and hosted on a web server. South Ridge can easily be converted to a UWP application that takes advantage of native platform capabilities and can be distributed through the Windows Store as any other UWP app. 

Read the [full blog post](https://blogs.windows.com/buildingapps/2016/09/30/uwp-hosted-web-app-on-xbox-one-app-dev-on-xbox-series) or visit the [App Dev on Xbox landing page](http://aka.ms/xboxappdev) for more videos and blogs.

## Setup

The app is hosted at [http://southridge.azurewebsites.net](http://southridge.azurewebsites.net) for this example but you can deploy it yourself to any backend you choose. Once the app is hosted, all you need is the url to create app packages.

1. Use [manifoldJS](http://manifoldjs.com/) from the command line on any platform npm is supported, or through the [web](http://manifoldjs.com/generator). In this example, we are using the npm generator from the command line. 

  *install manifoldjs*
  ```
  npm install -g manifoldjs
  ```

  *generate package*
  ```
  manifoldjs http://southridge.azurewebsites.net/
  ```

2. To deploy the package to the Xbox, first make sure your Xbox is in [Dev Mode](https://msdn.microsoft.com/en-us/windows/uwp/xbox-apps/devkit-activation). You can then use the [Windows Device Portal (WDP)](https://msdn.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal) through a web browser to configure and manage the Xbox. [Follow these steps to enable WDP on your Xbox](https://msdn.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal-xbox?f=255&MSPPError=-2147217396). Launch WDP in your favourite browser and navigate to the Apps tab.

3. Under the *Install app* section, click on *Browse* next to *pick a loose folder* 

4. Navigate to the *windows10* folder from the packages you generated with manifoldJS and select the *manifest* folder

5. Click the *Go* button to upload the loose files and install the app. Once this is complete, you will be able to find and launch the app on your Xbox

6. Enjoy!
