using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishMovement : MonoBehaviour {
	// Contains list of positions of the fish path
	private List<Vector3> path = new List<Vector3>();
	
	// Bones that we will be rotating
	private Transform headBone;
	private Transform bodyBone;
	private Transform tailBone;
	
	// Quaternion that transform from localBone rotation to globalBone rotation
	private Quaternion generalToBone;
	
	// Current position of the fish in the interpolated path
	private float currentPos = -1.0f;
	
	// Rotation smoothing variables
	private SmoothQuaternion fishRotation;
	private SmoothQuaternion bodyRotation;
	private SmoothQuaternion headRotation;
	private float rotationFactor = 3.0f*0.9f;
	
	// Line that shows the path in debug mode
	private LineRenderer lineRenderer;
	private float touchTime = 0.0f;
	
	private bool debug = false;
	
	// Values of movement when fish is Idle
	public float bodyMovement = 15.0f; 			// how much you want to rotate the body
	public float bodyMovementVelocity = 10.0f; 	// how fast you want to rotate it
	public float tailMovement = 15.0f;
	public float tailMovementVelocity = 10.0f;
	
	// Values of movement when fish is moving
	public EasingType easing = EasingType.Quintic; 	// which easing do you want to use
	public float bodyRotationVelocity = 1.0f; 		// how fast do you want to interpolate fish rotations
	public float velocity = 0.5f;					// how fast does the fish moves to follow the path
	
	
	// Path gameObject
	public bool replayPaths = false;
	
	public AudioClip[] audios;
	
	// Fish effects
	public ParticleSystem bubbleParticleSystem;
	public int bubbles = 10;
	
	
	void Start () {
		// Model bones
		headBone = transform.FindChild("Fish/Armature/Bone_001");
		bodyBone = transform.FindChild("Fish/Armature/Bone_001/Bone_002/Bone_003");
		tailBone = transform.FindChild("Fish/Armature/Bone_001/Bone_002/Bone_003/Bone_004");
		
		// Bone rotations to lerp with
		bodyRotation = transform.rotation;
		bodyRotation.Duration = bodyRotationVelocity;
		fishRotation = transform.rotation;
		fishRotation.Duration = bodyRotationVelocity/2;
		headRotation = transform.rotation;
		headRotation.Duration = bodyRotationVelocity/5;
		
		generalToBone = Quaternion.Inverse(transform.rotation)*bodyBone.rotation;
		path.Add(transform.position);
		
		lineRenderer = Camera.mainCamera.gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetWidth(0.0f,0.0f);
	}
	
	void Update () {
		if (touchTime > 0) touchTime -= Time.deltaTime;
		if (touchTime <= 0 && ((Input.acceleration.magnitude > 2.2f) || Input.GetKeyDown(KeyCode.N))) {
			touchTime = 1.0f;
			if (RenderSettings.ambientLight.r == 148.0f/255.0f) {
				RenderSettings.ambientLight = Color.white*0.7f;
				if (Application.loadedLevel == 0) {
					audio.clip = audios[1];
					audio.Play();
				}
			} else {
				Application.LoadLevel(1 - Application.loadedLevel);
			}
		}
		
		if (!replayPaths) {
			//=======================
			//== Input processing ===
			//=======================
			
			// If Left mouse button or touch, we add a new position to the path
			if (Input.GetMouseButtonDown(0) || (Input.touchCount == 1 && touchTime < 0)) {
				Vector2 contactPoint;
				if (Input.touchCount > 0) {
					touchTime = 1.0f;
					contactPoint = Input.touches[0].position;
				} else contactPoint = Input.mousePosition;
				
				Vector3 pos = Camera.main.ScreenToWorldPoint(contactPoint);
				pos.y = transform.position.y;
				
				path.Add(pos);
			
			// If Right mouse button or touch with 2 fingers, we play the path
			} else if (Input.GetMouseButtonDown(1) || Input.touchCount == 2) {
				currentPos = 0;
			}
		}
		
		
		
		
		
		//=======================
		//==== Path playing =====
		//=======================
		if (currentPos >= 0) {
			// We have finished following the path
			if (currentPos >= 1) {
				currentPos = -1;
				path.Clear();
				path.Add(transform.position);
				
			// We are still following the path
			} else {
				Quaternion q = new Quaternion();
				// http://whydoidoit.com/2012/04/06/unity-curved-path-following-with-easing/
				transform.position = Spline.MoveOnPath(path.ToArray(), transform.position, ref currentPos, ref q, velocity, 100, easing, true, true);
				
				// We change Duration of the interpolation in order to fit movement velocity
				bodyRotation.Duration = bodyRotationVelocity*rotationFactor/velocity;
				headRotation.Duration = bodyRotation.Duration/2;
				fishRotation.Duration = bodyRotation.Duration/3;
					
				fishRotation.Value = q;
				bodyRotation.Value = q;
				headRotation.Value = q;
				
				transform.rotation = fishRotation;
			}
		}
		
		
		//=======================
		//=== Bone rotations ====
		//=======================
		headBone.rotation = headRotation*generalToBone;
		
		if (bodyRotation.IsComplete) {
			bodyBone.Rotate(0,0,(-bodyMovement + Mathf.PingPong(Time.time*bodyMovementVelocity,bodyMovement*2))*Time.deltaTime);	
		} else bodyBone.rotation = bodyRotation*generalToBone;
		
		tailBone.Rotate(0,0,(-tailMovement + Mathf.PingPong(Time.time*tailMovementVelocity,tailMovement*2))*Time.deltaTime);
		
		if (!replayPaths) {
			//=======================
			//======== Debug ========
			//=======================
			if (Input.GetMouseButtonDown(2) || (Input.touchCount > 2 && touchTime < 0)) {
				touchTime = 1.0f;
				debug = !debug;	
				if (debug) lineRenderer.SetWidth(0.1f, 0.5f);
				else lineRenderer.SetWidth(0.0f, 0.0f);
			}
			
			if (debug && path.Count > 0) {
				lineRenderer.SetVertexCount(path.Count);
				int i = 0;
				foreach (Vector3 pos in path) {
					lineRenderer.SetPosition(i, pos);
					i++;
				}
			}
		}
	}
	
	public void EmitBubbles() {
		bubbleParticleSystem.Emit(bubbles);	
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		foreach (Vector3 pos in path) {
			//Gizmos.DrawSphere(pos,0.5f);	
		}
	}
	
	public bool Finished {
		get {
			return currentPos == -1;	
		}
	}
	
	public void PlayPath(List<Vector3> path, float velocity) {
		this.path = path;
		this.velocity = velocity;
		this.currentPos = 0;
	}
}
