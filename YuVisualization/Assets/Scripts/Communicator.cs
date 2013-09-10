using UnityEngine;
using System.Collections;

public class Communicator : MonoBehaviour {
	public Path[] paths;
	public BiometricInfo biometrics;
	
	private float _timeBetweenTouch = 1.0f;
	private FishMovement _fish;
	private float _lastTouch;
	
	public bool playForever = false;
	// Fake data
	private bool _fakeData = false;
	private float _fakeSpeed = 0.5f;
	
    void Start() {
       _timeBetweenTouch = 0.0f;
	   _fish = GameObject.FindObjectOfType(typeof(FishMovement)) as FishMovement;
		biometrics.plotter.breathingListener = _fish.EmitBubbles;
    }
	
	void Update () {
		
		if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
			_fakeData = !_fakeData;	
		}
		float extraSpeed = 0.0f;
		if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
			extraSpeed = 0.1f;
		}
		if (Input.GetKeyUp(KeyCode.KeypadMinus)) {
			extraSpeed = -0.1f;
		}
		
		_fakeSpeed = Mathf.Clamp01(_fakeSpeed + extraSpeed);
		
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
		
		// We emit bubles if heartbeat detected 
		if (biometrics.beat) {
			_fish.EmitBubbles();	
		}
		
		// Velocity mapped with heartRate example
		if (biometrics.heartRate > 50) {
			_fish.velocity = Mathf.Lerp(0.0f, 20.0f,Mathf.Max (0, biometrics.heartRate - 80.0f)/40.0f);	
		}
		
		// GSR mapping
		if (biometrics._countGSR > 10) {
			float currentGSR = (_fakeData? _fakeSpeed : biometrics.NormalizedGSR);
			
			
			// Send data back to processing
			biometrics.Send("fake", _fakeData + "");
			biometrics.Send("fakespeed", _fakeSpeed + "");
			biometrics.Send("speed", biometrics.NormalizedGSR + "");
			
			if (!float.IsNaN(currentGSR)) {
				_fish.velocity = Mathf.Lerp(0.01f, 10.0f, currentGSR);
			}
			//Debug.Log(currentGSR + "        " + _fish.velocity + "|" + biometrics._minGSR+ " " +biometrics._maxGSR+ " " + biometrics._medianGSR);
		}
	}
}
