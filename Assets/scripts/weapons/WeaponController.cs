﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	public float fireRate; //shots per second
	public GameObject projectilePrefab;
	public Vector3 muzzlePosition; //point to spawn the projetile
	public int initBulletCount = 100;

	private float lastShotTime;
	private int bulletCount;

	public void Start() {
		bulletCount = initBulletCount;
	}

	public bool Fire() {
		if(CanShoot()) {
			if(projectilePrefab)
				SimplePool.Spawn(projectilePrefab, transform.TransformPoint(muzzlePosition), Quaternion.LookRotation(transform.right));
			lastShotTime = Time.time;
			bulletCount--;
			return true;
		}
		return false;
	}

	public int GetBulletCount() {
		return bulletCount;
	}

	private bool CanShoot() {
		return (Time.time - lastShotTime) > (1f / fireRate) && (bulletCount > 0);
	}
}
