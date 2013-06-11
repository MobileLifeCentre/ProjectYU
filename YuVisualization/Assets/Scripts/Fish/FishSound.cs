using UnityEngine;
using System.Collections;

public class FishSound : MonoBehaviour {
	public AudioClip[] sounds;
	public float soundRatio = 5.0f;
	private float time;
	
	
	// Use this for initialization
	void Start () {
		ResetTime ();
	}
	
	// Update is called once per frame
	void Update () {
		if (time < 0) {
			ResetTime ();
			//audio.PlayOneShot(audio.clip);
		} else {
			time -= Time.deltaTime;	
		}
	}
			
	void ResetTime() {
		time = Random.Range (soundRatio/2, soundRatio);		
	}
}
