# Intro system

![Intro system](Images/intro_flow_galaxy_explorer.png)

## IntroFlow
The introduction sequence with its transitions at the start of Galaxy Explorer is implemented through the IntroFlow component that can be found attached to the IntroFlow gameobject in the main_scene.

The IntroFlow component in turn is dependent on the FlowManager component.

## FlowManager
FlowManager is a sequencing tool intended to easily drive a simple sequence of interactive (or non-interactive) events. It can be used via its single C# script, or in prefab form. It's dependent on the MRTK for input events.

The FlowManager component can be found attached to the flow_manager_prefab gameobject, child of the Managers gameobject in the core_systems_scene.

### Stages
FlowManager manages transitions between different application stages. In the component's inspector window you can see the 5 stages that are currently part of Galaxy Explorer's introduction: `Intro`, `EarthPinMR`, `EarthPinDesktop`, `SolarSystemScene` and `GalaxyScene`.

![Flow manager stages](Images/flow_manager_inspector.png)

### Editing the intro flow
FlowManager has a custom in-editor UI which allows you to edit the introduction flow. In the Unity inspector window of the flow_manager_prefab gameobject, click the `Open Flow Window` button on the FlowManager component to open up FlowManager's custom in-editor window.

![In-editor UI](Images/flow_manager_in_editor_ui.png)

In this window you can see the intro stages and their transitions in more details. When this flow has been completed and the final GalaxyScene stage has been reached, control over transitions is taken over by other parts of the code which handle the more complex transition logic of the rest of the application.

To edit the intro stages, scroll to the utmost right side of the list of stages and press the `+ new stage` button. If you want the remove a stage, simply press the `x` in the top right corner of the stage you want to remove.

For each stage you can add `Event groups`. These event groups contain both `Events` and `Exit events` that can be hooked up to the appropriate functionality following the regular work flow of Unity events.

### Exposed functionality
To control non-automatic transitions, FlowManager exposes 2 methods. `AdvanceStage()` can be called to transition to the next stage, while `JumpToStage(int targetStage)` can be called to transition to a specified stage.

A set of public variables are exposed as well: `int m_currentStage`, `bool m_restartEnabled`, `int m_loopBackStage`, `string m_currentStageName`, `float m_fastestTapTime` and `FlowStage[] m_stages`.

Additionally, the FlowManager exposes a series of events for you to subscribe to: `OnAutoTransition`, `OnManualTransition`, `OnStageTransition`, `OnLoopbackTransition`.

### Debug
To get access to the above mentioned public variables through the FlowManager component in the Unity editor, toggle on the Debug setting at the bottom of the component in the Unity inspector.