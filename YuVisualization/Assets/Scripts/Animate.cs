using UnityEngine;
using System.Collections;

public class Animate : MonoBehaviour {
	public float rate = 3f;
	
	void Start () {
		StartCoroutine(AnimateLoop());
	}
	
	IEnumerator AnimateLoop() {
		while(true) {
			yield return new WaitForSeconds(rate);
			Vector2 offset = transform.renderer.material.mainTextureOffset;
			offset.x += transform.renderer.material.mainTextureScale.x;
			transform.renderer.material.mainTextureOffset = offset;
		}
	}
}
