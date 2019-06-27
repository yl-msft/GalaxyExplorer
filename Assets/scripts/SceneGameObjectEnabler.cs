using System;
using System.Collections;
using System.Collections.Generic;
using GalaxyExplorer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGameObjectEnabler : MonoBehaviour
{

    private static Dictionary<string, int> seenScenes = new Dictionary<string, int>();
    
    [SerializeField] private float delay;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private int executeOnLoadNumber;

    private IEnumerator Start()
    {
        yield return IsInTransitionRoutine();
        yield return new WaitForSeconds(delay);
        var sceneName = gameObject.scene.name;
        if (seenScenes.ContainsKey(sceneName))
        {
            seenScenes[sceneName]++;
        }
        else
        {
            seenScenes.Add(sceneName,0);
        }
        targetObject.SetActive(seenScenes[sceneName] == executeOnLoadNumber || executeOnLoadNumber < 0);
    }

    private IEnumerator IsInTransitionRoutine()
    {
        while (GalaxyExplorerManager.Instance.TransitionManager.InTransition)
        {
            yield return null;
        }
    }
}
