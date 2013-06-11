using UnityEngine;
using System.Collections;

public class WaterLillyMovement : MonoBehaviour {
	public float radiusMovement = 2.0f;
	public float velocity = 3.0f;
	private Vector3 originalPosition;
	private Vector3 targetPosition;
	private float currentVelocity;
	
	// Use this for initialization
	void Start () {
		originalPosition = transform.position;
		targetPosition = originalPosition;
	}
	
	// Update is called once per frame
	void Update () {
		float distance = Vector3.Distance(transform.position, targetPosition);
		if (distance < 0.05) {
			targetPosition = originalPosition + new Vector3(Random.Range(-radiusMovement,radiusMovement), Random.Range(-radiusMovement,radiusMovement),
																Random.Range(-radiusMovement,radiusMovement));
			currentVelocity = Random.Range(velocity/2, velocity);
		} else transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentVelocity*distance*Time.deltaTime);
	}
}
