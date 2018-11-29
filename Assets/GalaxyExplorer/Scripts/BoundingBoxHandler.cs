// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class BoundingBoxHandler : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Size of scale handles in Bounding box.")]
        private Vector3 scaleHandleSize = new Vector3(0.08f, 0.08f, 0.08f);

        [SerializeField]
        [Tooltip("Size of rotate handles in Bounding box.")]
        private Vector3 rotateHandleSize = new Vector3(0.08f, 0.08f, 0.08f);

        private bool groupBoundinBoxEntities = false;
        private GameObject parent = null;

        private void Start()
        {
            StartCoroutine(OnBoundingBoxCreated());
        }

        // Bounding Box Rig creates many entities used by bounding box
        // All of them are created in the scene under no parent, and makes the scene editor meesy and difficult to find sth
        // Find all entities created by BoundingBoxRig and group them under one single parent
        // ALso scale handles of bounding box as MRTK doesnt provide public properties for this
        public IEnumerator OnBoundingBoxCreated()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (groupBoundinBoxEntities)
            {
                yield break;
            }

            parent = (parent == null) ? new GameObject() : parent;
            parent.name = "BoundingBoxEntities";
            //parent.transform.parent = transform.parent;

            GameObject center = GameObject.Find("center");
            if (center)
            {
                center.transform.parent = parent.transform;
            }

            GameObject bbb = GameObject.Find("BoundingBoxBasic(Clone)");
            if (bbb)
            {
                bbb.transform.parent = parent.transform;
            }

            List<GameObject> corners = GetObjectsWithName("Corner");
            if (corners != null && corners.Count > 0)
            {
                foreach (var entity in corners)
                {
                    entity.transform.parent = parent.transform;
                    entity.transform.localScale = scaleHandleSize;
                }
            }

            List<GameObject> middles = GetObjectsWithName("Middle");
            if (middles != null && middles.Count > 0)
            {
                foreach (var entity in middles)
                {
                    entity.transform.parent = parent.transform;
                    entity.transform.localScale = rotateHandleSize;
                }

                groupBoundinBoxEntities = true;
            }

            AppBar appBar = FindObjectOfType<AppBar>();
            if (appBar)
            {
                appBar.transform.parent = parent.transform;
                appBar.gameObject.SetActive(false);
            }

            if (GalaxyExplorerManager.IsDesktop)
            {
                parent.SetActive(false);
            }

            yield return null;
        }

        // Returns all objects that contain in their name the given string
        private List<GameObject> GetObjectsWithName(string objectName)
        {
            GameObject[] allObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));
            List<GameObject> allObjWithName = new List<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].name.Contains(objectName))
                {
                    allObjWithName.Add(allObjects[i]);
                }
            }

            return allObjWithName;
        }
    }
}
