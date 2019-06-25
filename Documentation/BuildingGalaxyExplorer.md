# Building & running the application

![BuildingGalaxyExplorer](Images/ge_app_icon_banner.png)

## Running in Unity

Note that by default when you download / clone this repository and open it in Unity, it will open a new scene. Navigate to the *Assets/scenes* folder and double-click main_scene to set up the editor properly. After that, you can hit the play button to succesfully start the experience.

## Building Galaxy Explorer

In the Unity editor, choose *File > Build Settings* to bring up the build settings window. All of the scenes in the *Scenes In Build* section should be checked.

Choose *Universal Windows Platform* as the platform. On the right side, choose *Any device* as the target device, *XAML* as the build type, and *10.0.18361.0* as the target SDK version.

You can now build the project.

After the build completes successfully, an explorer window will pop up. Navigate into the build folder you created and double-click the generated *GalaxyExplorer.sln* file in its root to launch the solution in Visual Studio.

In Visual Studio, set the configuration to *Release* for faster builds (doesn't use .NET Native) or *Master* to build the type of package the Microsoft Store needs (uses .NET Native).

### Building for HoloLens (1st gen)

In *File > Build Settings*:

Architecture should be set to **x86**.

### Building for HoloLens 2

In *File > Build Settings*:

Architecture should be set to **ARM**.

### Building for Windows Mixed Reality headsets

In *File > Build Settings*:

Architecture should be set to **x64**.

### Building for Windows Desktop

In *File > Build Settings*:

Architecture should be set to **x64** or **x86**.

It's now possible to deploy to the emulator, a remote device, or create a Microsoft Store package to deploy at a later time.

# See also

- [Unity Manual: Universal Windows Platform](https://docs.unity3d.com/Manual/windowsstore-il2cpp.html)
