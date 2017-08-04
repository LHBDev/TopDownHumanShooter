using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

	public float maxHealth = 100f;
	public bool destroyOnDeath = true;

	private float health;
	private bool dead = false;

	void Start () {
		health = maxHealth;
	}

	public void ApplyDamage(float damage) {
		SetHealth(health - damage);
	}

	public void AddHealth(float amount) {
		SetHealth(health + amount);
	}

	public void SetHealth(float newHealth) {
		health = newHealth;
		if(health <= 0f) Die();
		else if(health > maxHealth) health = maxHealth;
	}

	public float GetHealth() {
		return health;
	}

	private void Die() {
		if(dead) return;
		dead = true;
		transform.BroadcastMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
		if(destroyOnDeath) Destroy(this.gameObject);
	}
}
