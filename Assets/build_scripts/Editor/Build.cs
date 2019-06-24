using UnityEditor;
using UnityEngine;

public static class Build
{
    public static void Invoke()
    {
        BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        {
            locationPathName = "UWP",
            options = BuildOptions.None,
            targetGroup = BuildTargetGroup.WSA,
            target = BuildTarget.WSAPlayer,
            scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes)
        });
    }
}
