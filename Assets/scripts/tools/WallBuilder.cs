using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct Edge {
	public int point1;
	public int point2;

	public Edge(int point1, int point2) {
		if(point1 < point2) {
			this.point1 = point1;
			this.point2 = point2;
		}
		else {
			this.point1 = point2;
			this.point2 = point1;
		}
	}

	public bool Equals(Edge other) {
		return (point1 == other.point1 && point2 == other.point2) || (point2 == other.point1 && point1 == other.point2);
	}
}

[System.Serializable]
public struct WallLayer {
	public float layerEdge; //value between 0 to 1, determines start of next layer
	public Material material;

	private List<Vector3> quads;

	public void AddQuad(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4) {
		if(quads == null) quads = new List<Vector3>();
		quads.Add(point1);
		quads.Add(point2);
		quads.Add(point3);
		quads.Add(point4);
	}

	public List<Vector3> GetQuads() {
		return quads;
	}

	public void ClearQuads() {
		if(quads == null) return;
		quads.Clear();
	}
}

public struct ColliderLine{
	public Vector3 point1;
	public Vector3 point2;

	public ColliderLine(Vector3 point1, Vector3 point2) {
		this.point1 = point1;
		this.point2 = point2;
	}
}

public class WallBuilder : MonoBehaviour {

	public WallLayer[] wallLayers;
	public float wallThickness = 0.5f;
	public float snapDistance = 0.1f;
	public float colliderHeight = 2f;
	public Material transparentMaterial;

	[SerializeField] private List<Vector3> points;
	[SerializeField] private List<Edge> edges;
	private int selectedPoint = -1;
	private List<Vector3> colliderVertices;


	public void Rebuild() {
		if(points.Count == 0 || edges.Count == 0) return;
		colliderVertices = new List<Vector3>();
		ClearWallLayers();
		GenerateLayerQuads();
		GenerateMesh();
		GenerateColliders();
	}

	public void ClearInstances() {
		Transform collider = transform.Find("collider");
		if(collider != null) DestroyImmediate(collider.gameObject);
		for(int i = 0; i < wallLayers.Length; i++) {
			Transform layer = transform.Find("wallLayer" + i);
			if(layer != null) DestroyImmediate(layer.gameObject);
		}
	}

	public void ClearAll() {
		ClearInstances();
		ClearWallLayers();
		edges.Clear();
		points.Clear();
		SelectPoint(-1);
	}

	public Edge AddEdge(int point1, int point2) {
		Edge newEdge = new Edge(point1, point2);
		edges.Add(newEdge);
		return newEdge;
	}

	//add if new, remove if exists
	public void ToggleEdge(int point1, int point2) {
		Edge edge = new Edge(point1, point2);
		for(int i = 0; i < edges.Count; i++) {
			if(edges[i].Equals(edge)) {
				RemoveEdge(i);
				return;
			}
		}
		AddEdge(point1, point2);
	}

	public List<Edge> GetEdges() {
		return edges;
	}

	public List<Edge> GetEdgesWithPoint(int pointIndex) {
		List<Edge> pointEdges = new List<Edge>();
		foreach(Edge edge in edges) {
			if(edge.point1 == pointIndex || edge.point2 == pointIndex) pointEdges.Add(edge);
		}
		return pointEdges;
	}

	public void RemoveEdge(int index) {
		edges.RemoveAt(index);
	}

	public Vector3 GetPoint(int index) {
		return points[index];
	}

	public Vector3 GetWorldPoint(int index) {
		return transform.TransformPoint(points[index]);
	}

	public void SetPoint(int index, Vector3 newPoint) {
		newPoint = Common.Snap(newPoint, snapDistance);
		newPoint.y = 0;
		points[index] = newPoint;
	}

	public int AddPoint(Vector3 newPoint) {
		newPoint = transform.InverseTransformPoint(newPoint);
		newPoint = Common.Snap(newPoint, snapDistance);
		newPoint.y = 0;
		points.Add(newPoint);
		return points.Count - 1;
	}

	public void RemovePoint(int index) {
		RemoveEdgesWithPoint(index);
		points.RemoveAt(index);
		if(selectedPoint == index) SelectPoint(-1);
		//update edge indexes
		for(int i = 0; i < edges.Count; i++) {
			Edge edge = edges[i];
			if(edge.point1 > index) edge.point1--;
			if(edge.point2 > index) edge.point2--;
			edges[i] = edge;
		}
	}

	public List<Vector3> GetPoints() {
		return points;
	}

	public int GetPointCount() {
		return points.Count;
	}

	public void SelectPoint(int point) {
		if(selectedPoint == point) selectedPoint = -1;
		else selectedPoint = point;
	}

	public int GetSelectedPoint() {
		return selectedPoint;
	}

	public Plane GetPlane() {
		return new Plane(transform.up ,transform.position);
	}

	private void AddCollider(Vector3 point1, Vector3 point2) {
		colliderVertices.Add(point1);
		colliderVertices.Add(point2);
	}

	//remove all edges that have that point
	private void RemoveEdgesWithPoint(int point) {
		for(int i = (edges.Count - 1); i >= 0; i--) {
			if(edges[i].point1 == point || edges[i].point2 == point) RemoveEdge(i);
		}
	}


	//clockwice angle to the centerline between two direction (edges)
	private float GetEdgeCenterlineClockwiceAngle(Edge edge, int centerPoint, Vector3 from) {
		int otherPoint = edge.point1 == centerPoint ? edge.point2 : edge.point1;
		Vector3 to = GetPoint(otherPoint) - GetPoint(centerPoint);
		float angle = Common.ClockwiseAngle(from, to, Vector3.up);
		angle = angle <= 180f ? angle / 2f : angle + ((360f - angle) / 2f); //centerline
		return angle * Mathf.Deg2Rad;
	}

	private float GetMaxEdgeAngle(List<Edge> edges, int centerPoint, Vector3 from) {
		if(edges.Count == 0) return (Mathf.PI / 2f) * 3;
		float max = 0f;
		foreach(Edge edge in edges) {
			float angle = GetEdgeCenterlineClockwiceAngle(edge, centerPoint, from);
			if(angle > max) max = angle;
		}
		return max;
	}

	private float GetMinEdgeAngle(List<Edge> edges, int centerPoint, Vector3 from) {
		if(edges.Count == 0) return Mathf.PI / 2f;
		float min = float.MaxValue;
		foreach(Edge edge in edges) {
			float angle = GetEdgeCenterlineClockwiceAngle(edge, centerPoint, from);
			if(angle < min) min = angle;
		}
		return min;
	}

	private void ClearWallLayers() {
		for(int i = 0; i < wallLayers.Length; i++) {
			wallLayers[i].ClearQuads();
		}
	}


	private Vector3 GetCornerPoint(Vector3 forward, float angle) {
		forward.Normalize();
		forward = Common.RotateY(forward, angle);
		return forward * (wallThickness / Mathf.Sin(angle));
	}

	//left and right are left and right when looking from point1 to point2, forward is direction from point1 to point2
	private void GenerateLayerQuads() {
		foreach(Edge edge in edges) {
			List<Edge> neighbours1 = GetEdgesWithPoint(edge.point1);
			List<Edge> neighbours2 = GetEdgesWithPoint(edge.point2);
			neighbours1.RemoveAll(x => x.Equals(edge)); //remove current edge
			neighbours2.RemoveAll(x => x.Equals(edge));
			Vector3 point1Position = GetPoint(edge.point1);
			Vector3 point2Position = GetPoint(edge.point2);
			Vector3 forward = (point2Position - point1Position).normalized;
			Vector3 cornerRight1 = GetCornerPoint(forward, GetMinEdgeAngle(neighbours1, edge.point1, forward));
			Vector3 cornerLeft1 = GetCornerPoint(-forward, GetMaxEdgeAngle(neighbours1, edge.point1, forward));
			Vector3 cornerRight2 = GetCornerPoint(forward, GetMaxEdgeAngle(neighbours2, edge.point2, -forward));
			Vector3 cornerLeft2 = GetCornerPoint(-forward, GetMinEdgeAngle(neighbours2, edge.point2, -forward));
			float layerStartDistance = 0f;
			for(int i = 0; i < wallLayers.Length; i++) {
				float layerEndDistance = wallLayers[i].layerEdge;
				wallLayers[i].AddQuad(
					point1Position + cornerLeft1 * layerStartDistance,
					point1Position + cornerLeft1 * layerEndDistance,
					point2Position + cornerLeft2 * layerEndDistance,
					point2Position + cornerLeft2 * layerStartDistance
				);
				wallLayers[i].AddQuad(
					point1Position + cornerRight1 * layerEndDistance,
					point1Position + cornerRight1 * layerStartDistance,
					point2Position + cornerRight2 * layerStartDistance,
					point2Position + cornerRight2 * layerEndDistance
				);
				layerStartDistance = layerEndDistance;
			}
			if(neighbours1.Count == 0) GenerateTipLayerQuads(edge, edge.point1);
			if(neighbours2.Count == 0) GenerateTipLayerQuads(edge, edge.point2);
			AddCollider(point1Position + cornerRight1, point2Position + cornerRight2);
			AddCollider(point2Position + cornerLeft2, point1Position + cornerLeft1);
		}
	}

	private void GenerateTipLayerQuads(Edge edge, int tipIndex) {
		Vector3 tipPoint = GetPoint(tipIndex);
		Vector3 otherPoint = tipIndex == edge.point1 ? GetPoint(edge.point2) : GetPoint(edge.point1);
		Vector3 forward = (tipPoint - otherPoint).normalized * wallThickness;
		Vector3 left = Common.RotateY(forward, -Mathf.PI / 2f);
		Vector3 cornerPoint1 = (tipPoint - otherPoint).normalized * wallThickness;
		cornerPoint1 += Common.RotateY(cornerPoint1, -Mathf.PI / 2f);
		Vector3 cornerPoint2 = Common.RotateY(cornerPoint1, Mathf.PI / 2f);
		float layerStartDistance = wallLayers[0].layerEdge;
		wallLayers[0].AddQuad(
			tipPoint + left * layerStartDistance,
			tipPoint + cornerPoint1 * layerStartDistance,
			tipPoint + cornerPoint2 * layerStartDistance,
			tipPoint + left * -layerStartDistance);
		for(int i = 1; i < wallLayers.Length; i++) {
			float layerEndDistance = wallLayers[i].layerEdge;
			wallLayers[i].AddQuad(
				tipPoint + left * layerStartDistance,
				tipPoint + left * layerEndDistance,
				tipPoint + cornerPoint1 * layerEndDistance,
				tipPoint + cornerPoint1 * layerStartDistance);
			wallLayers[i].AddQuad(
				tipPoint + cornerPoint1 * layerStartDistance,
				tipPoint + cornerPoint1 * layerEndDistance,
				tipPoint + cornerPoint2 * layerEndDistance,
				tipPoint + cornerPoint2 * layerStartDistance);
			wallLayers[i].AddQuad(
				tipPoint + left * -layerStartDistance,
				tipPoint + cornerPoint2 * layerStartDistance,
				tipPoint + cornerPoint2 * layerEndDistance,
				tipPoint + left * -layerEndDistance);
			layerStartDistance = layerEndDistance;
		}
		AddCollider(tipPoint + cornerPoint1, tipPoint + left);
		AddCollider(tipPoint + cornerPoint2, tipPoint + cornerPoint1);
		AddCollider(tipPoint + (left * -1f), tipPoint + cornerPoint2);
	}

	private void GenerateMesh() {
		List<MeshFilter> meshFilters = GetLayerMeshFilters();
		for(int i = 0; i < wallLayers.Length; i++) {
			List<Vector3> wallQuads = wallLayers[i].GetQuads();
			Vector3[] vertices = wallQuads.ToArray();
			int[] triangles = new int[(vertices.Length / 2) * 3];
			for(int j = 0; j < triangles.Length / 6; j++) {
				triangles[j * 6] = j * 4;
				triangles[j * 6 + 1] = j * 4 + 1;
				triangles[j * 6 + 2] = j * 4 + 2;
				triangles[j * 6 + 3] = j * 4 + 2;
				triangles[j * 6 + 4] = j * 4 + 3;
				triangles[j * 6 + 5] = j * 4 + 0;
			}
			Vector3[] normals = new Vector3[vertices.Length];
			for(int j = 0; j < normals.Length; j++) {
				normals[j] = Vector3.up;
			}
			meshFilters[i].sharedMesh.Clear();
			meshFilters[i].sharedMesh.vertices = vertices;
			meshFilters[i].sharedMesh.triangles = triangles;
			meshFilters[i].sharedMesh.normals = normals;
		}
	}

	private List<MeshFilter> GetLayerMeshFilters() {
		List<MeshFilter> meshFilters = new List<MeshFilter>();
		for(int i = 0; i < wallLayers.Length; i++) {
			Transform layer = transform.Find("wallLayer" + i);
			if(layer == null) {
				GameObject newGO = new GameObject("wallLayer" + i);
				newGO.transform.parent = transform;
				newGO.transform.localPosition = Vector3.zero;
				newGO.AddComponent<MeshFilter>();
				newGO.GetComponent<MeshFilter>().sharedMesh = new Mesh();
				newGO.AddComponent<MeshRenderer>();
				newGO.GetComponent<MeshRenderer>().material = wallLayers[i].material;
				layer = newGO.transform;
			}
			meshFilters.Add(layer.GetComponent<MeshFilter>());
		}
		return meshFilters;
	}

	private void GenerateColliders() {
		Vector3[] vertices = new Vector3[colliderVertices.Count * 2];
		for(int i = 0; i < colliderVertices.Count; i++) {
			vertices[i * 2] = colliderVertices[i];
			vertices[i * 2 + 1] = colliderVertices[i] + Vector3.down * colliderHeight;
		}
		int[] triangles = new int[(vertices.Length / 2) * 3];
		for(int i = 0; i < triangles.Length / 6; i++) {
			triangles[i * 6 + 0] = i * 4 + 0;
			triangles[i * 6 + 1] = i * 4 + 2;
			triangles[i * 6 + 2] = i * 4 + 1;
			triangles[i * 6 + 3] = i * 4 + 2;
			triangles[i * 6 + 4] = i * 4 + 3;
			triangles[i * 6 + 5] = i * 4 + 1;
		}
		SetColliderMesh(vertices, triangles);
	}

	private void SetColliderMesh(Vector3[] vertices, int[] triangles) {
		Transform colliderTransform = transform.Find("collider");
		if(colliderTransform == null) {
			GameObject colliderGO = new GameObject("collider");
			colliderTransform = colliderGO.transform;
			colliderTransform.parent = transform;
			colliderTransform.localPosition = Vector3.zero;
			colliderGO.AddComponent<MeshCollider>();
			colliderGO.AddComponent<MeshFilter>();
			colliderGO.AddComponent<MeshRenderer>();
			Mesh mesh = new Mesh();
			mesh.name = transform.name + "ColliderMesh";
			colliderGO.GetComponent<MeshCollider>().sharedMesh = mesh;
			colliderGO.GetComponent<MeshFilter>().sharedMesh = mesh;
			colliderGO.GetComponent<MeshRenderer>().material = transparentMaterial;
			#if UNITY_EDITOR
			GameObjectUtility.SetStaticEditorFlags(colliderGO, StaticEditorFlags.NavigationStatic);
			GameObjectUtility.SetNavMeshArea(colliderGO, GameObjectUtility.GetNavMeshAreaFromName("Not Walkable"));
			#endif
		}
		Mesh sharedMesh = colliderTransform.GetComponent<MeshCollider>().sharedMesh;
		sharedMesh.Clear();
		sharedMesh.vertices = vertices;
		sharedMesh.triangles = triangles;
	}

	private MeshCollider GetMeshCollider() {
		Transform colliderTransform = transform.Find("collider");
		if(colliderTransform == null) {
			GameObject colliderGO = new GameObject("collider");
			colliderTransform = colliderGO.transform;
			colliderTransform.parent = transform;
			colliderTransform.localPosition = Vector3.zero;
			colliderGO.AddComponent<MeshCollider>();
			Mesh mesh = new Mesh();
			mesh.name = transform.parent.name + "colliderMesh";
			colliderGO.GetComponent<MeshCollider>().sharedMesh = mesh;
		}
		return colliderTransform.GetComponent<MeshCollider>();
	}
}
	