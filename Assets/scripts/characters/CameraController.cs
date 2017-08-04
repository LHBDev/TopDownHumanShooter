using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public GameObject targetGameObject;
	public Vector3 relativePosition;

	void Update () {
		if(targetGameObject == null) return;
		transform.position = targetGameObject.transform.position + relativePosition;
	}
}
