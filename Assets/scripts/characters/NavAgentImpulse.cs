using UnityEngine;
using System.Collections;

public class NavAgentImpulse : MonoBehaviour {

	public float drag = 10f;

	private UnityEngine.AI.NavMeshAgent navAgent;
	private Vector3 speed;

	public void Start () {
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
	}

	public void Update () {
		if(speed != Vector3.zero) {
			navAgent.Move(speed * Time.deltaTime);
			float magnitude = speed.magnitude;
			float multiplier = (magnitude - (drag * Time.deltaTime)) / magnitude;
			if(multiplier <= 0) speed = Vector3.zero;
			else speed *= multiplier;
		}
	}

	public void ApplyImpulse(Vector3 impulse) {
		speed += impulse;
	}
}
