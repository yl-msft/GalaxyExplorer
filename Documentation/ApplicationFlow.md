# Application flow

![Application flow](Images/ge_unity_flow_manager.png)

This page contains information about how the application is structured, when scenes are triggered and so on.

## Core systems scene

*scenes/core_systems_scene.unity* is a scene that contains most of the global game objects. It contains things like the [menu system](MenuSystem.md) and [audio functionality](AudioSystem.md). The core systems scene is loaded into all content scenes via *Layers* so that developers and artists can run any scene independently from running the main scene (see the `LayerCompositor` component attached to the *LayerComp* game object in the main scene).

That said, while core systems scene is auto loaded, specific view scenes will only play correctly if the core scene is added as the "Active Scene". The main scene is the exception to this.

![Activating the core systems scene](Images/ge_unity_core_systems_scene.png)

## SceneTransition

Each view in the application (galactic disc, galactic center and solar system) is a scene in *scenes/view_scenes*. Every scene needs to have a `SceneTransition` component added to the root entity of the scene. All other game objects need to live under this entity. This is used by the [TransitionManager](##TransitionManager) to move from the current scene to another one.

![Scene transition](Images/ge_unity_scene_transition.png)

The component is aware of the size of the scene via the reference to a collider named `EntireSceneCollider`.

It also contains a `SceneFocusCollider` that defines the focus point within the scene for a transition. For example the galactic disc will have the focus collider on the center of the galaxy, the solar system scene has the sun as focus collider. The idea is that the previous and new scene's focus colliders are being transitioned from one into the other.

It also has a reference to `SceneObject` - the entity in the scene that the transform is applied to during a transition. This needs to be a child of the view scene root object (which contains the SceneTransition component) and needs to have an identity transform. All game objects of the scene need to be under this scene object or they won't be moved along during transition.

## TransitionManager

The actual transition is managed through the [GalaxyExplorerManager](##GalaxyExplorerManager) in the core systems scene. [ViewLoader](##ViewLoader) handles (un-)loading these scenes and the `TransitionManager` manages the actual movement from one scene to the other through callbacks from the ViewLoader. This system also handles additional animations that are run between scenes for theatrical effect.

![Transition manager](Images/ge_unity_transition_manager.png)

### How the transition manager works

First, components not relevant for the transition are disabled, for example the `OrbitUpdater`, `PointOfInterest`, POI rotation animation, and so on. All these are components that move the game objects in the scene and they should not move during a transition to a new scene. The new scene will be scaled to fill the given percentage of a global volume. The global volume is defined in the transform of TransformSource, which can be found in the core systems scene under *Loader > TouchController > Pivot > BoundingBox*. The specific percentage for the new scene is set in the [SceneTransition](##SceneTransition) component as `FillVolumePercentage` (see image above).

The `ZoomInOut` component implements the transition logic to modify the current and new scene to transition from the current scene's focus collider transform into the next scene's focus collider transform. After its initialization as `ZoomInOutBehaviour` the properties for the start and end position, rotation and scale are set and the scene objects should not change anything by themselves anymore.

During transitions all colliders of the current and the new scene are deactivated so no user interactions are possible. Any [POIs](PointsOfInterest.md) in the current scene fade out completely, then the whole scene starts fading out.

The audio transition plays and the new scene now becomes active and starts fading in. However, the alpha for the POIs stays at zero so they aren't visible.

Then the actual transition starts via the `ZoomInOutSimultaneouslyFlow` coroutine. When that transition is complete, the previous scene unloads, the new scene's POIs fade in and the colliders are enabled again.

When transitioning keep in mind that scenes need to be rotated and scaled around the focus collider objects as pivots. See [SceneTransition](##SceneTransition) for details on where to set focus colliders.

## ViewLoader

`ViewLoader` lives inside core systems scene and manages the loading & unloading of scenes used throughout the application. [TransitionManager](##TransitionManager) calls this in order to load and unload scenes.

All scripts hook up on ViewLoader's callbacks in order to know when a new scene is about to be loaded and when that has been completed.

ViewLoader also keeps the trail of scenes in a stack in order to know to which scene to go back to (used by the "back" button in the [menu](MenuSystem.md)). Scenes during the introduction flow should not go into this stack as the user is not supposed to go back to the introduction flow.

## GalaxyExplorerManager

This is one of the main components of the application. It's attached to the *Managers > GEManagers* game object in the core systems scene. Its execution order is set to execute before Unity's default execution order starts.

`GalaxyExplorerManager` decides on which platform the application is currently running (`PlatformId`) and holds properties that are different per platform. It also holds references to many other managers of the scene in order for components to access them through the GalaxyExplorerManager.

Platform detection happens as part of GalaxyExplorerManager's `Awake` function. When running as a UWP application, the component first checks if it is running on a HoloLens 2 by querrying Windows' [EasClientDeviceInformation](https://docs.microsoft.com/en-us/uwp/api/Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation) class to get local device information.

If this is not the case, it will continue with a check for HoloLens (1st gen) and VR through Unity's [XRDevice](https://docs.unity3d.com/ScriptReference/XR.XRDevice.html) class. If this is true, the script further distinguishes between HoloLens (1st gen) and VR by checking if the display is opaque or not through Unity's [HolographicSettings](https://docs.unity3d.com/ScriptReference/XR.WSA.HolographicSettings.html) class.

If none of the above cases is found to be true, the platform will be set to desktop.

## TransformHandler

Every scene should spawn around the placement object the user places at the start of the experience. In order to keep consecutive scenes in the same position, rotation and scale, every scene needs to have a `TransformHandler` with the matching id of TransformSource (`TransformSourceId`). The position, rotation and scaling properties are then synced through the transform in TransformHandle. This way, all scenes have the same transform values.

![Scene transition](Images/ge_unity_scene_transition.png)