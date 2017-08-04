using UnityEngine;
using System.Collections;

public class Expire : MonoBehaviour {

	public float time = 1f;
	public bool fadeOut;
	public float fadeOutStartTimeFraction = 0.8f;

	private float startTime;
	private bool useSimplePool;
	private Renderer rendererComponent;
	private Color color;

	public void Awake() {
		useSimplePool = (GetComponent<SimplePool.PoolMember>() != null);
		rendererComponent = GetComponent<Renderer>();
		if(rendererComponent != null) color = rendererComponent.material.color;
	}

	public void OnEnable() {
		startTime = Time.time;
	}

	void Update () {
		if(fadeOut && Time.time > (startTime + (time * fadeOutStartTimeFraction))) {
			float alpha = 1f - ((Time.time - (startTime + (time * fadeOutStartTimeFraction))) / (time * (1f - fadeOutStartTimeFraction)));
			rendererComponent.material.color = new Color(color.r, color.g, color.b, alpha);
		}
		if(Time.time > (startTime + time)) {
			if(useSimplePool) SimplePool.Despawn(transform.gameObject);
			else Destroy(transform.gameObject);
		}
	}
}
