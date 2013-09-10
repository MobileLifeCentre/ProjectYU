using UnityEngine;
using System.Collections;

public class Communicator : MonoBehaviour {
	public Path[] paths;
	public BiometricInfo biometrics;
	
	private float _timeBetweenTouch = 1.0f;
	private FishMovement _fish;
	private float _lastTouch;
	
	public bool playForever = false;
	
    void Start() {
       _timeBetweenTouch = 0.0f;
	   _fish = GameObject.FindObjectOfType(typeof(FishMovement)) as FishMovement;
		biometrics.plotter.breathingListener = _fish.EmitBubbles;
    }
	
	void Update () {
		// Example of how to set path to replay
		if (_fish.replayPaths) {
			if (_lastTouch < 0) {
				if (Input.GetKeyDown(KeyCode.Alpha1) || Input.touchCount == 2) {
					_lastTouch = _timeBetweenTouch;
					_fish.PlayPath(paths[2].ToList(), 3);
				} else if (Input.GetKeyDown(KeyCode.Alpha2)  || Input.touchCount == 3) {
					_lastTouch = _timeBetweenTouch;
					_fish.PlayPath(paths[1].ToList(), 8);
				} else if (Input.GetKeyDown(KeyCode.Alpha3)  || Input.touchCount == 4) {
					_lastTouch = _timeBetweenTouch;
					_fish.PlayPath(paths[2].ToList(), 5);
				} else if (Input.GetKeyDown(KeyCode.Alpha4)  || Input.touchCount == 5) {
					_lastTouch = _timeBetweenTouch;
					_fish.PlayPath(paths[1].ToList(), 8);
				}
			} else _lastTouch -= Time.deltaTime;
		}
		
		if (playForever) {
			if (_fish.Finished) {
				_fish.PlayPath(paths[Random.Range(0, paths.Length)].ToList(), 3);	
			}
		}
		
		if (biometrics.beat) {
			Debug.Log ("beat "+ Time.time);
			_fish.EmitBubbles();	
		}

		if (biometrics.heartRate > 50) {
			_fish.velocity = Mathf.Lerp(0.0f, 20.0f,Mathf.Max (0, biometrics.heartRate - 80.0f)/40.0f);	
		}
		
		if (biometrics._countGSR > 10) {
			float currentGSR = (biometrics._medianGSR-biometrics._minGSR)/(biometrics._maxGSR-biometrics._minGSR);
			if (!float.IsNaN(currentGSR)) _fish.velocity = Mathf.Lerp(0.01f, 10.0f, 1 - currentGSR);
			//Debug.Log(currentGSR + "        " + _fish.velocity + "|" + biometrics._minGSR+ " " +biometrics._maxGSR+ " " + biometrics._medianGSR);
		}
	}
}
