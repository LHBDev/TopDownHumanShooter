using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WallBuilder))]
public class WallBuilderInspector : Editor {

	private float mouseOverPointDistance = 30f;
	private WallBuilder wallBuilder;
	private int activePoint;
	private int selectedPoint;

	public void OnSceneGUI() {
		wallBuilder = (WallBuilder) target;
		activePoint = GetPointUnderMouse();
		selectedPoint = wallBuilder.GetSelectedPoint();
		DrawEdges();
		DrawPoints();
		HandleInput();
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		if(GUILayout.Button("Rebuild")) {
			WallBuilder wallBuilder = (WallBuilder) target;
			wallBuilder.Rebuild();
		}
		if(GUILayout.Button("Clear Instances")) {
			WallBuilder wallBuilder = (WallBuilder) target;
			wallBuilder.ClearInstances();
		}
		DrawDefaultInspector();
		if(GUILayout.Button("Clear All")) {
			WallBuilder wallBuilder = (WallBuilder) target;
			wallBuilder.ClearAll();
		}
	}

	private void DrawEdges() {
		Handles.color = Color.white;
		foreach(Edge edge in wallBuilder.GetEdges()) {
			Handles.DrawLine(wallBuilder.GetWorldPoint(edge.point1), wallBuilder.GetWorldPoint(edge.point2));
		}
	}

	private void DrawPoints() {
		for(int i = 0; i < wallBuilder.GetPointCount(); i++) {
			Vector3 p = wallBuilder.GetWorldPoint(i);
			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? wallBuilder.transform.rotation : Quaternion.identity;
			EditorGUI.BeginChangeCheck();
			Vector3 newPoint = Handles.DoPositionHandle(p, handleRotation);
			newPoint = wallBuilder.transform.InverseTransformPoint(newPoint);
			if(EditorGUI.EndChangeCheck()) {
				//Undo.RecordObject(wallBuilder, "move wallBuilder point");
				//Undo.IncrementCurrentGroup();
				EditorUtility.SetDirty(wallBuilder);
				wallBuilder.SetPoint(i, newPoint);
			}
			Handles.color = activePoint == i ? Color.green : Color.gray;
			Handles.color = selectedPoint == i ? Color.red : Handles.color;
			Handles.DrawSolidDisc(wallBuilder.GetWorldPoint(i), Vector3.up, 0.1f);
		}
	}

	private void HandleInput() {
		if(Camera.current == null) return;
		Event e = Event.current;
		if(e.type == EventType.KeyDown && !e.shift && e.character == 'g') { //add stuff
			//Undo.RecordObject(wallBuilder, "add stuff to WallBuilder");
			//Undo.IncrementCurrentGroup();
			EditorUtility.SetDirty(wallBuilder);
			if(activePoint == -1) {
				if(selectedPoint == -1) wallBuilder.AddPoint(MousePlanePosition());
				else {
					int newPoint = wallBuilder.AddPoint(MousePlanePosition());
					wallBuilder.AddEdge(selectedPoint, newPoint);
					wallBuilder.SelectPoint(-1);
				}
			} else {
				if(selectedPoint == -1) wallBuilder.SelectPoint(activePoint);
				else {
					if(selectedPoint != activePoint) wallBuilder.ToggleEdge(activePoint, selectedPoint);
					wallBuilder.SelectPoint(-1);
				}
			}
		}
		else if(e.type == EventType.KeyDown && e.shift && e.character == 'G') { //remove stuff
			//Undo.RecordObject(wallBuilder, "remove stuff from WallBuilder");
			//Undo.IncrementCurrentGroup();
			EditorUtility.SetDirty(wallBuilder);
			if(activePoint != -1) wallBuilder.RemovePoint(activePoint);
		}
	}

	private int GetPointUnderMouse() {
		if(Camera.current == null) return -1;
		Vector3 mousePosition = Event.current.mousePosition;
		for(int i = 0; i < wallBuilder.GetPointCount(); i++) {
			Vector3 screenPoint = Camera.current.WorldToScreenPoint(wallBuilder.GetWorldPoint(i));
			screenPoint.z = 0f;
			screenPoint.y = Camera.current.pixelHeight - screenPoint.y;
			if((screenPoint - mousePosition).magnitude < mouseOverPointDistance) return i;
		}
		return -1;
	}

	private Vector3 MousePlanePosition() {
		if(Camera.current == null) return Vector3.zero;
		Vector3 mousePosition = Event.current.mousePosition;
		mousePosition.y = Camera.current.pixelHeight - mousePosition.y;
		Ray ray = Camera.current.ScreenPointToRay(mousePosition);
		float distance;
		if(wallBuilder.GetPlane().Raycast(ray, out distance)) return ray.GetPoint(distance);
		return Vector3.zero;
	}

}
