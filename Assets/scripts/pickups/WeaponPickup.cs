using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {

	public GameObject weaponPrefab;

	public void OnTriggerEnter(Collider other) {
		if(other.tag == "Player") {
			other.transform.GetComponent<PlayerWeaponController>().SetWeapon(weaponPrefab);
			Destroy(gameObject);
		}
	}
}
