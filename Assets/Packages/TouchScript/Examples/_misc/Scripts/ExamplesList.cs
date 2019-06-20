// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using UnityEngine;

/// <exclude />
public class ExamplesList : MonoBehaviour 
{

	public RectTransform Content;

	void Start () 
	{
		gameObject.SetActive(false);
	}

	public void ShowHide()
	{
		gameObject.SetActive(!gameObject.activeSelf);
		Content.localPosition = Vector3.zero;
	}

}
