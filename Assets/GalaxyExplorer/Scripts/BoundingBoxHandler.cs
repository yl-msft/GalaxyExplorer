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

        [SerializeField]
        [Tooltip("Parent entity of bounding box automatic generated gameobjects.")]
        private GameObject ParentOfBBEntities = null;

        private bool groupBoundinBoxEntities = false;
        private bool isAppBarFound = false;

        private void Start()
        {
            StartCoroutine(OnBoundingBoxCreated(false));

            // because of execution order maybe we need to wait before parenting the bb entities
            if (!isAppBarFound)
            {
                StartCoroutine(OnBoundingBoxCreated(true));
            }

            if (GalaxyExplorerManager.Instance.ToolsManager)
            {
                GalaxyExplorerManager.Instance.ToolsManager.OnBoundingBoxDelegate += OnBoundingBoxDelegate;
            }
        }

        private void OnBoundingBoxDelegate(bool enable)
        {
            StartCoroutine(OnBoundingBoxCreated(true));
        }

        private void LateUpdate()
        {
            if (transform.lossyScale.x > GalaxyExplorerManager.Instance.ToolsManager.LargestZoom && transform.parent)
            {
                transform.localScale = GalaxyExplorerManager.Instance.ToolsManager.LargestZoom * Vector3.one / transform.parent.lossyScale.x;
            }
            else if (transform.lossyScale.x < GalaxyExplorerManager.Instance.ToolsManager.MinZoom && transform.parent)
            {
                transform.localScale = GalaxyExplorerManager.Instance.ToolsManager.MinZoom * Vector3.one / transform.parent.lossyScale.x; 
            }
        }

        // Bounding Box Rig creates many entities used by bounding box
        // All of them are created in the scene under no parent, and makes the scene editor meesy and difficult to find sth
        // Find all entities created by BoundingBoxRig and group them under one single parent
        // ALso scale handles of bounding box as MRTK doesnt provide public properties for this
        public IEnumerator OnBoundingBoxCreated(bool wait)
        {
            // In desktop dont wait as the app bar is visible for a fraction of time and it shouldnt be
            if (wait)
            {
                yield return new WaitForEndOfFrame();
            }

            if (groupBoundinBoxEntities)
            {
                yield break;
            }

            ParentOfBBEntities = (ParentOfBBEntities == null) ? new GameObject() : ParentOfBBEntities;
            ParentOfBBEntities.name = "BoundingBoxEntities";

            GameObject center = GameObject.Find("center");
            if (center)
            {
                center.transform.parent = ParentOfBBEntities.transform;
            }

            GameObject bbb = GameObject.Find("BoundingBoxBasic(Clone)");
            if (bbb)
            {
                bbb.transform.parent = ParentOfBBEntities.transform;
            }

            List<GameObject> corners = GetObjectsWithName("Corner");
            if (corners != null && corners.Count > 0)
            {
                foreach (var entity in corners)
                {
                    entity.transform.parent = ParentOfBBEntities.transform;
                    entity.transform.localScale = scaleHandleSize;
                }
            }

            List<GameObject> middles = GetObjectsWithName("Middle");
            if (middles != null && middles.Count > 0)
            {
                foreach (var entity in middles)
                {
                    entity.transform.parent = ParentOfBBEntities.transform;
                    entity.transform.localScale = rotateHandleSize;
                }

                groupBoundinBoxEntities = true;
            }

            AppBar appBar = FindObjectOfType<AppBar>();
            if (appBar)
            {
                appBar.transform.parent = ParentOfBBEntities.transform;
                appBar.gameObject.SetActive(false);
                isAppBarFound = true;
            }

            if (GalaxyExplorerManager.IsDesktop)
            {
                ParentOfBBEntities.SetActive(false);
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
