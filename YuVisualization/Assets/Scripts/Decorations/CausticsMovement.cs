using UnityEngine;
using System.Collections;

public class CausticsMovement : MonoBehaviour {
	public float movement = 1.0f;
	public float velocity = 0.5f;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(-velocity/2+Mathf.PingPong(Time.time*velocity,velocity),0,0);
	}
}
