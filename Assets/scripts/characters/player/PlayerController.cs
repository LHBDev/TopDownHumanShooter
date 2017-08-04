using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour {

	private NavMeshAgent navAgent;
	private Health health;

	void Start () {
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		health = GetComponent<Health>();
	}
	
	void Update () {
		AimAt(RaycastMousePosition());
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		navAgent.Move(direction * navAgent.speed * Time.deltaTime);
	}

	public void OnGUI() {
		GUI.TextArea(new Rect(10f, 100f, 150f, 20f), "HEALTH: " + health.GetHealth());
	}

	public void OnDeath() {
		//Game Over!!!
	}

	private void AimAt(Vector3 worldPoint) {
		worldPoint.y = 0f;
		Vector3 characterPosition = transform.position;
		characterPosition.y = 0;
		transform.rotation = Quaternion.LookRotation(worldPoint - characterPosition);
	}

	private Vector3 RaycastMousePosition() {
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Physics.Raycast(ray, out hit)) {
			return hit.point;
		}
		return Vector3.zero;
	}
}
