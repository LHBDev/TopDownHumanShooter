using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	public float fireRate; //shots per second
	public GameObject projectilePrefab;
	public Vector3 muzzlePosition; //point to spawn the projetile

	private float lastShotTime;

	public bool Fire() {
		if(CanShoot()) {
			if(projectilePrefab)
				SimplePool.Spawn(projectilePrefab, transform.TransformPoint(muzzlePosition), Quaternion.LookRotation(transform.right));
			lastShotTime = Time.time;
			return true;
		}
		return false;
	}

	private bool CanShoot() {
		return (Time.time - lastShotTime) > (1f / fireRate);
	}
}
