using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TestEnemyAI : MonoBehaviour {

	private NavMeshAgent navAgent;
	private AIController aiController;
	private GameObject player;

	void Start () {
		aiController = GetComponent<AIController>();
		navAgent = GetComponent<NavMeshAgent>();
		player = aiController.GetPlayer();
	}
	
	void Update () {
		if(player == null) return;
		if(aiController.CanAttack()) aiController.Attack();
		else navAgent.SetDestination(player.transform.position);
	}

	public void OnDeath() {
		
	}
}
