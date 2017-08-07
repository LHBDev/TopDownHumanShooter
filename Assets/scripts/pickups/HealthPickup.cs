using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour {

	public float healthAmount = 25f;

	public void OnTriggerEnter(Collider other) {
		if(other.tag == "Player") {
			Health health = other.transform.GetComponent<Health>();
			if(!health.IsFull()) {
				health.AddHealth(healthAmount);
				Destroy(gameObject);
			}
		}
	}
}
