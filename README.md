# Augmented Reality Applications for Human-Robot Interaction (2023)
## A Software Suite for the Control of Mobile Robots
### Author: Torsten Sprey
### Supervisor: Prof. Niko Suenderhauf
*Add Image*

## About
*To Do*

## Installation Guide
1.	Download the folder from the GitHub link above and the [Microsoft Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778).
2.	Add the project into the Unity Hub list. 
3.	Open the project. A currently installed/preferred Unity version can be used with UWP selected as the target platform. Press **Change Version** and **Continue**. 
4.	In the MRTK Project Configurator click **Use OpenXR (Recommended)**. Close the **Project Settings** window and on the next window click **Skip until next session**. 
5.	If **NuGet** isn't visible on the editor's menu bar, in the **Assets** folder click on **NuGet**, select **Load on startup** and click **Apply**. 
6.	Click off and click back on the Unity Editor which should cause it to start compiling scripts. When the warning dialogue appears click **Yes** to restart the Editor. If it crashes just reopen the project from Unity Hub. 
7.	Go to **NuGet** -> **Manage NuGet Packages** on the menu bar. Search for **“Microsoft.MixedReality.QR”** and install it if it isn’t already installed. 
9.	Open the **Microsoft Mixed Reality Feature Tool**, select this project and select any packages that are currently installed. Click **Get Features**, **Validate**, **Import** and **Approve** to update these packages to the latest versions. 
10.	Complete the remaining steps of the **MRTK Project Configurator**. If it doesn’t appear after the previous step, restart the editor.
11.	Make sure the **OpenXR** and **Microsoft HoloLens feature group** checkboxes are checked under the **Universal Windows Platform settings** (tab with the windows logo) -> **Plug-in Providers**.
12.	Click **Skip this step**, **Next** and **Done** to complete the setup.
13.	If there are still errors and the editor was not restarted in steps 6 or 10, **restart** the editor now.
14.	Switch platform by going to **File** -> **Build Settings...**, selecting **Universal Windows Platform** and **Switch Platform** (if not done in step 3). Make sure the following settings are active: 
    - **Architecture:** ARM 64-bit 
    - **Build Type:** D3D Project 
    - **Target SDK Version:** Latest Installed 
    - **Minimum Platform Version:** 10.0.10240.0  
    - **Visual Studio Version:** Latest installed 
    - **Build and Run on:** Local Machine 
    - **Build configuration:** Release (there are known performance issues with Debug) 

This project should now be buildable. To do so, follow the steps in the Building and Deploying to the HoloLens section below. If any errors remain or occur during building and deployment, refer to the Troubleshooting section.

Many of the steps above were taken from the following resources:
Mixed Reality Feature Tool documentation: https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool 
Unity HoloLens 2 beginner tutorial: https://learn.microsoft.com/en-us/training/paths/beginner-hololens-2-tutorials/ 

## Building and Deploying to the HoloLens Guide
1.	Go to **File** -> **Build Settings** -> **Build**. Click **Add Open Scenes** to add the current scene. 
2.	Create a new folder called “**Builds**” in the project directory and **Select** that folder. 
3.	Once build is completed, open the **Builds** folder and open the **<Project Name>.sln** file in Visual Studio.
4.	Configure Visual Studio for HoloLens by selecting the **Master** or **Release** (**Release** preferred) configuration and the **ARM64** architecture.
5.	Click the deployment target drop-down and then do one of the following: 
    - If building and deploying via Wi-Fi, select **Remote Machine**. 
    - If building and deploying via USB, select **Device**.
6.	Set the remote connection. On the menu bar, select **Project** -> **Properties** -> **Configuration Properties** -> **Debugging**. 
7.	Click the "**Debugger to launch:**" drop down and then select **Remote Machine** if it's not selected already. Set the **Authentication Mode** to **Universal (Unencrypted protocol)**. 
8.	In the Machine Name field, enter the IP address of the HoloLens. To find the HoloLens’ IP address go to **Settings** -> **Updates & Security** -> **For developers** and scroll to the bottom. 
9.	To deploy to the HoloLens and automatically start the app without the Visual Studio debugger attached, select **Debug** -> **Start Without Debugging**. Or to deploy to the HoloLens without having the app start automatically, select **Build** -> **Deploy Solution**. 

Source: https://learn.microsoft.com/en-us/training/paths/beginner-hololens-2-tutorials/ (units 2 and 6).

## Troubleshooting
### Deploy Failed on Visual Studio 
This can occur after Visual Studio is updated, which causes it to lose the Machine Name configuration property. To fix, go to Project  Properties  Configuration Properties  Debugging and add the IP of the HoloLens back into the Machine Name field. 
This can also occur if the Unity project is built on a previous version of Visual Studio and then a Deploy is attempted on a later version. To fix this, navigate to the folder where the Unity project was built and delete all the files there. Rebuild the Unity project and it should now be deployable. Make sure to have also followed the steps in the previous paragraph. 
 
### Visual Studio NuGet Package Restore Failed Unable to Find Version 
Visual Studio doesn’t check the internet for NuGet packages by default which causes errors if the packages aren’t on your computer. To fix this in Visual Studio, go to Tools  Options  NuGet Package Manager  Packages Sources. Click the green plus button in the top right corner then enter the following into the fields. 
Name: nuget.org 
Source: https://www.nuget.org/api/v2/ 
Press Update, then in NuGet Package Manager click on General  Clear All NuGet Storage. Press Ok and the project should now compile without errors. 
 
### Unity WSATestCertificate is Expired 
Go to Edit  Project Settings  Player  Publishing Settings. Under Certificates click the button above Create and delete the WSATestCertificate.pfx file. Close the file explorer window and then click Create. Leave everything at default and create a new certificate. 
 
### ROS# Project Newtonsoft.Json.dll Not Found Error 
This can sometimes be fixed by closing and reopening the Unity project. When this isn’t the case, try installing NuGet by following the steps above. 

### Unity Event Callbacks Not Fully Executing 
The most surefire way to solve this is to change the value of a private Boolean variable, which is checked by a conditional statement in Update(). The formerly blocking code will be successfully executed inside this conditional statement. Be sure to reset the Boolean variable at the end.


## Credits
Connection to the ROS websocket is provided by EricVoll's [UWP fork](https://github.com/ericvoll/ros-sharp/tree/UWP) of the [ROS# package](https://github.com/siemens/ros-sharp) by siemens. This fork allows this 
project to be built to UWP devices such as the Microsoft HoloLens 2 augmented reality headset (although it won't really work since that device has no keyboard).

Many of the scripts for tracking QR codes were provided by [Microsoft's QR code tracking sample](https://github.com/chgatla-microsoft/QRTracking).

Marker placement for the navigation system was adapated from this [article[(https://localjoost.github.io/migrating-to-mrtk2interacting-with/) by Joost van Schaik. Included was this GitHub [demo project](https://github.com/LocalJoost/SpatialMapInteraction) containing scripts used in this project.

Special thanks to Harry Stone (former QUT student who worked on this project in 2022) for providing many of the resources to get this project started.
