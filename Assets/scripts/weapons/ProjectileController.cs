using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	public float damage = 10f;
	public float speed = 20f;
	public float range = 100f;
	public bool destroyOnImpact = true;
	public bool stopOnImpact = true;
	public float physicsImpulse = 1f;

	private Vector3 startPosition;
	private bool hasImpacted = false;
	private float impactTime = 0f;

	void OnEnable() {
		startPosition = transform.position;
		hasImpacted = false;
		impactTime = 0f;
		Update();
	}

	void Update() {
		if(hasImpacted && stopOnImpact) return;
		RaycastHit hit = new RaycastHit();
		if(!hasImpacted && Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime)) {
			transform.position = hit.point;
			HitTarget(hit);
		}
		else {
			transform.position += transform.forward * speed * Time.deltaTime;
			if((transform.position - startPosition).magnitude > range) HitTarget(hit);
		}
	}

	public float GetImpactTime() {
		return impactTime;
	}

	private void HitTarget(RaycastHit hit) {
		GameObject target = hit.transform == null ? null : hit.transform.gameObject;
		hasImpacted = true;
		impactTime = Time.time;
		if(target == null) SimplePool.Despawn(this.gameObject); //end of bullet range
		else {
			target.BroadcastMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
			transform.BroadcastMessage("OnProjectileHitTarget", hit, SendMessageOptions.DontRequireReceiver);
			if(hit.rigidbody != null) hit.rigidbody.AddForceAtPosition(transform.forward * physicsImpulse, hit.point, ForceMode.Impulse);
			if(destroyOnImpact) SimplePool.Despawn(this.gameObject);
		}
		
	}
}
