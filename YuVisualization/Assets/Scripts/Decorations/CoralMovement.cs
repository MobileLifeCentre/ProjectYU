using UnityEngine;
using System.Collections;

public class CoralMovement : MonoBehaviour {
	
	// How much and how fast does the coral moves
	public float movementWidth = 10.0f;
	public float movementVelocity = 5.0f;
	
	// We have some Random to make the coral look more natural
	private float rotation;
	private float start;
	private float velocity;
	
	
	void Start () {
		rotation = Random.Range (movementWidth/2, movementWidth);
		start = Random.Range (0.0f, 3.0f);
		velocity = Random.Range (movementVelocity/2, movementVelocity);
	}
	
	void Update () {
		transform.Rotate(-rotation/2+Mathf.PingPong((Time.time+start)*velocity, rotation),0,0);
	}
}
