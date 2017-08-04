using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {

	public GameObject bulletObject;
	public GameObject trailObject;
	public float length = 10f;
	public float bulletPortion = 0.2f;

	private LineRenderer trail;
	private LineRenderer bullet;
	private ProjectileController projectileController;
	private Vector3 startPosition;
	private bool endReached = false;

	void Awake() {
		bullet = bulletObject.GetComponent<LineRenderer>();
		trail = trailObject.GetComponent<LineRenderer>();
		projectileController = transform.GetComponent<ProjectileController>();
	}

	void OnEnable() {
		startPosition = transform.position;
		endReached = false;
		Update();
	}
	
	void Update () {
		float d = MovementSinceImpact();
		float s = 0f;
		float e = Mathf.Max(transform.InverseTransformPoint(startPosition).z + d, -length + d);
		float m = Mathf.Max(Mathf.Min(s, (bulletPortion * -length) + d), e);
		bullet.SetPosition(0, new Vector3(0, 0, s));
		bullet.SetPosition(1, new Vector3(0, 0, m));
		trail.SetPosition(0, new Vector3(0, 0, m));
		trail.SetPosition(1, new Vector3(0, 0, e));
		if(e > 0) SimplePool.Despawn(this.gameObject);
	}

	public void OnProjectileHitTarget(RaycastHit hit) {
		endReached = true;
		Light light = GetComponent<Light>();
		if(light != null) light.enabled = false;
		NavAgentImpulse impulse = hit.transform.GetComponent<NavAgentImpulse>();
		if(impulse != null) impulse.ApplyImpulse(transform.forward * 10f);
	}

	private float MovementSinceImpact() {
		if(endReached) return (Time.time - projectileController.GetImpactTime()) * projectileController.speed;
		return 0f;
	}
}
