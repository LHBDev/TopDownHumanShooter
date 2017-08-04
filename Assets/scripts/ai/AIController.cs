using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour {

	public float attackRange = 5f;
	public float attackRate = 2f;
	public float damage = 10f;

	private GameObject player;
	private float lastAttackTime = 0f;

	void Awake() {
		player = GameObject.FindWithTag("Player");
	}

	public bool CanAttack() {
		return Time.time > (lastAttackTime + (1f / attackRate)) && GetPlayerDistance() < attackRange;
	}

	public void Attack() {
		lastAttackTime = Time.time;
		player.SendMessage("ApplyDamage", damage);
	}

	public GameObject GetPlayer() {
		return player;
	}

	public float GetPlayerDistance() {
		return (transform.position - player.transform.position).magnitude;
	}
}
