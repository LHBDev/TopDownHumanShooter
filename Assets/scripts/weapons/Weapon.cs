using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Weapon {

	public float fireRate; //shots per second
	public GameObject projectilePrefab;
	public Vector3 muzzlePosition; //point to spawn the projetile

	private float lastShotTime;

	public bool Fire(Transform transform) {
		if(CanShoot()) {
			if(projectilePrefab)
				SimplePool.Spawn(projectilePrefab, transform.TransformPoint(muzzlePosition), Quaternion.LookRotation(transform.forward));
			lastShotTime = Time.time;
			transform.BroadcastMessage("OnWeaponFire", SendMessageOptions.DontRequireReceiver);
			return true;
		}
		return false;
	}

	private bool CanShoot() {
		return (Time.time - lastShotTime) > (1f / fireRate);
	}
}