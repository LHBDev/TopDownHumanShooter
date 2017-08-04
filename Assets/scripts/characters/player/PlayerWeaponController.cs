using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour {

	[SerializeField]
	private List<Weapon> weapons; //must always have atleast one weapon!
	private int currentWeaponIndex = 0;
	private Weapon currentWeapon;

	void Start() {
		currentWeapon = weapons[currentWeaponIndex];
	}

	void Update() {
		if(Input.GetButtonDown("NextWeapon")) NextWeapon();
		if(Input.GetButtonDown("PreviousWeapon")) PreviousWeapon();
		if(Input.GetButton("Fire1")) Fire();
	}

	public void AddWeapon(Weapon newWeapon) {
		weapons.Add(newWeapon);
	}

	private void Fire() {
		currentWeapon.Fire(transform);
	}

	private void NextWeapon() {
		SetCurrentWeapon(currentWeaponIndex + 1);
	}

	private void PreviousWeapon() {
		SetCurrentWeapon(currentWeaponIndex - 1);
	}

	private void SetCurrentWeapon(int weaponIndex) {
		if(weaponIndex >= weapons.Count) weaponIndex = 0;
		else if(weaponIndex < 0) weaponIndex = weapons.Count - 1;
		currentWeapon = weapons[weaponIndex];
		currentWeaponIndex = weaponIndex;
	}
}
