using UnityEngine;
using System.Collections;

public static class Common {

	public static bool MouseRaycast(out RaycastHit hit) {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Physics.Raycast(ray, out hit)) {
			return true;
		}
		return false;
	}

	public static bool MouseRaycast(out RaycastHit hit, int layerMask) {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
			return true;
		}
		return false;
	}

	public static Vector3 MouseRaycastPosition() {
		RaycastHit hit;
		if(MouseRaycast(out hit)) {
			return hit.point;
		}
		return Vector3.zero;
	}

	public static Vector3 MouseRaycastPosition(int layerMask) {
		RaycastHit hit;
		if(MouseRaycast(out hit, layerMask)) {
			return hit.point;
		}
		return Vector3.zero;
	}

	/*
	public static void DebugDrawRay(Ray ray) {
		DebugDrawLine(ray.origin, ray.direction, 1000f);
	}

	public static void DebugDrawLine(Vector3 start, Vector3 direction, float distance) {
		DebugDrawLine(start, start + (direction.normalized * distance));
	}

	public static void DebugDrawLine(Vector3 start, Vector3 end) {
		DebugDrawLine(start, end, Color.green);
	}

	public static void DebugDrawLine(Vector3 start, Vector3 end, Color color) {
		Material material = Resources.Load("urpo") as Material;
		GameObject instance = new GameObject();
		instance.name = "debug line";
		LineRenderer lineRenderer = instance.AddComponent<LineRenderer>();
		lineRenderer.material = material;
		lineRenderer.SetVertexCount(2);
		lineRenderer.SetPosition(0, start);
		lineRenderer.SetPosition(1, end);
		lineRenderer.SetWidth(0.1f, 0.1f);
		lineRenderer.SetColors(color, color);
	}*/

	public static float ClockwiseAngle(Vector3 a, Vector3 b, Vector3 axis){
		float angle = Vector3.Angle(a,b);
		float sign = Mathf.Sign(Vector3.Dot(axis,Vector3.Cross(a,b)));
		return ((angle * sign) + 360) % 360;
	}

	public static Vector3 RotateX(Vector3 vector, float angle ) {
		float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );
		return new Vector3(vector.x, (cos * vector.y) - (sin * vector.z), (cos * vector.z) + (sin * vector.y));
	}
	
	public static Vector3 RotateY(Vector3 vector, float angle ) {
		float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );
		return new Vector3((cos * vector.x) + (sin * vector.z), vector.y, (cos * vector.z) - (sin * vector.x));
	}
	
	public static Vector3 RotateZ(Vector3 vector, float angle ) {
		float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );
		return new Vector3((cos * vector.x) - (sin * vector.y), (cos * vector.y) + (sin * vector.x), vector.z);
	}

	public static float LookAtYRotation(Vector3 forward) {
		return Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
	}

	public static float AngleLerp(float from, float to, float delta) {
		delta = Mathf.Clamp01(delta);
		to = Mathf.Abs(to - from) < Mathf.Abs((to - 360f) - from) ? to : (to - 360f);
		to = Mathf.Abs(to - from) < Mathf.Abs((to + 360f) - from) ? to : (to + 360f);
		return (from + (to - from) * delta) % 360f;
	}

	public static Vector3 Flat(this Vector3 point) {
		point.y = 0;
		return point;
	}

	public static float Snap(float value, float snap) {
		if(snap == 0f) return value;
		float a = Mathf.Floor(value / snap);
		float b = value - (snap * a);
		if(Mathf.Abs(b / snap) > 0.5f) a += Mathf.Sign(b);
		return a * snap;
	}

	public static Vector3 Snap(Vector3 value, float snap) {
		return new Vector3(Snap(value.x, snap), Snap(value.y, snap), Snap(value.z, snap));
	}

	public static Vector3 Plus(this Vector3 vector, float x, float y, float z) {
		vector.x += x;
		vector.y += y;
		vector.z += z;
		return vector;
	}

	public static bool Equal(this Vector3 a, Vector3 b, float delta) {
		return (Mathf.Abs(a.x - b.x) < delta && Mathf.Abs(a.y - b.y) < delta && Mathf.Abs(a.z - b.z) < delta);
	}

	public static bool HasParameterOfType (this Animator self, string name, AnimatorControllerParameterType type) {
		var parameters = self.parameters;
		foreach (var currParam in parameters) {
			if (currParam.type == type && currParam.name == name) {
				return true;
			}
		}
		return false;
	}
}
