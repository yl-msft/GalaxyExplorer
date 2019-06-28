using System;
using System.Collections;
using System.Collections.Generic;
using GalaxyExplorer;
using UnityEngine;

public class SceneGameObjectEnabler : MonoBehaviour
{

    private static Dictionary<string, int> seenScenes = new Dictionary<string, int>();
    
    [SerializeField] private float delay;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private int executeOnLoadNumber;
    
    private string sceneName;

    private void Awake()
    {
        sceneName = gameObject.scene.name;
    }

    private IEnumerator Start()
    {
        yield return IsInTransitionRoutine();
        yield return new WaitForSeconds(delay);
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
