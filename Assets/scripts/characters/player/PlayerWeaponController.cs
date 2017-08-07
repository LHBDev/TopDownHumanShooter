using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour {

	public GameObject defaultWeaponPrefab;
	public Vector3 weaponPosition;
	public Vector3 weaponRotation;

	private GameObject weaponInstance;
	private WeaponController weaponController;

	public void Start() {
		SetWeapon(defaultWeaponPrefab);
	}

	public void Update() {
		if(Input.GetButton("Fire1")) Fire();
	}

	public void SetWeapon(GameObject weaponPrefab) {
		Destroy(weaponInstance);
		weaponInstance = Instantiate(weaponPrefab, transform);
		weaponInstance.transform.localPosition = weaponPosition;
		weaponInstance.transform.localRotation = Quaternion.Euler(weaponRotation);
		weaponController = weaponInstance.GetComponent<WeaponController>();
	}

	private void Fire() {
		weaponController.Fire();
		if(weaponController.GetBulletCount() <= 0) SetWeapon(defaultWeaponPrefab);
	}
}
