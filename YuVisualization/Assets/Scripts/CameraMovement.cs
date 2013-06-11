using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {
	public Vector3 limits;
	public float touchSensitivity = 1.0f;
	public float mouseSensitivity = 1.0f;
	public float velocity = 1.0f;
	
	private Vector3 _target;
	private Vector3 _focusPosition = Vector3.zero;
	
	void Start () {
		_target = transform.position;
		limits.x -= Camera.main.orthographicSize*Screen.width/Screen.height;
		limits.z -= Camera.main.orthographicSize;
	}
	
	void Update () {
		Vector2 deltaMovement = GetDeltaMovement();
		if (deltaMovement.magnitude > 0.1f) {
			_target = DeltaToAbsolute(deltaMovement);
		}
		
		transform.position = Vector3.MoveTowards(transform.position, _target, Time.deltaTime*velocity);
	}
	
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(Vector3.zero, limits*10);
	}
	
	Vector2 GetDeltaMovement() {
		Vector3 movement = Vector3.zero;
# if UNITY_EDITOR || UNITY_STANDALONE
		if (Input.GetMouseButton(0)) {
			if (_focusPosition.magnitude == 0.0f) {
				_focusPosition = Input.mousePosition;	
			}
			movement = (Input.mousePosition - _focusPosition)*mouseSensitivity;	
		} else if (Input.GetMouseButtonUp(0)) {
			_focusPosition = Vector3.zero;	
		}
#else
		if (Input.touchCount == 1) {
			Touch t = Input.GetTouch(0);
			if (_focusPosition.magnitude == 0.0f) {
				_focusPosition = t.position;	
			}
			movement = ((Vector3)t.position - _focusPosition)*touchSensitivity;
		} else {
			_focusPosition = Vector3.zero;	
		}
#endif
		
		return new Vector2(movement.x, movement.y);
	}
	
	Vector3 DeltaToAbsolute(Vector2 deltaMovement) {
		Vector3 position = transform.position + new Vector3(deltaMovement.x, 0, deltaMovement.y)*Time.deltaTime;
		
		position.x = Mathf.Clamp (position.x, limits.x, -limits.x);
		position.z = Mathf.Clamp (position.z, limits.z, -limits.z);
		
		return position;
	}
}
