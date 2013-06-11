using UnityEngine;
using System.Collections;



public class BiometricInfo : MonoBehaviour {
	public string OSCHost = "127.0.0.1";
	public int SendToPort = 7773;
	public int ListenerPort = 7772;
	public Plotter plotter;
	
	private Osc _handler;
	private float _respirationRate = 0.0f;
	private float _skinTemperature = 0.0f;
	private int _posture = 0;
	private float _peakAcceleration = 0.0f;
	private string _info = "EMPTY";
	private bool _visible = false;
	
	[HideInInspector]
	public float heartRate = 0.0f;
	public delegate void BreathingRawListener(int[] breathing);
	public BreathingRawListener _breathingRawListener = null;
	
	// Use this for initialization
	void Start () {
		UDPPacketIO udp = GetComponent<UDPPacketIO>();
		udp.init(OSCHost, SendToPort, ListenerPort);
		_handler = GetComponent<Osc>();
		_handler.init(udp);
		
		_handler.SetAddressHandler("/controller/HEART_RATE", HeartRateMessage);
		_handler.SetAddressHandler("/controller/RESPIRATION_RATE", RespirationMessage);
		_handler.SetAddressHandler("/controller/SKIN_TEMPERATURE", SkinTemperatureMessage);
		_handler.SetAddressHandler("/controller/POSTURE", PostureMessage);
		_handler.SetAddressHandler("/controller/PEAK_ACCLERATION", PeakAccelerationMessage);
		_handler.SetAddressHandler("/controller/BREATHING_RAW", BreathingRawMessage);
	}
	
	public void Update() {
		_info = _handler.GetInformation();
		
		if (Input.GetKeyDown(KeyCode.D)) {
			_visible = !_visible;
			plotter.Visible = _visible;
		}
	}
	
	public void OnGUI() {
		if (!_visible) return;	
		GUILayout.BeginVertical("box");
		GUILayout.Label(heartRate+"");
		GUILayout.Label(_respirationRate+"");
		GUILayout.Label(_skinTemperature+"");
		GUILayout.Label(_posture+"");
		GUILayout.Label(_peakAcceleration+"");
		GUILayout.Label(_info);
		GUILayout.EndVertical();
	}
	
	public void HeartRateMessage(OscMessage oscMessage)
 	{
 		heartRate = oscMessageToFloat(oscMessage);
	}
	
	public void RespirationMessage(OscMessage oscMessage)
 	{
 		_respirationRate = oscMessageToFloat(oscMessage);
	}
	
	public void SkinTemperatureMessage(OscMessage oscMessage)
 	{
 		_skinTemperature = oscMessageToFloat(oscMessage);
	}
	
	public void PostureMessage(OscMessage oscMessage)
 	{
 		_posture = int.Parse((string)oscMessage.Values[0]);
	}
	
	public void PeakAccelerationMessage(OscMessage oscMessage)
 	{
 		_peakAcceleration = oscMessageToFloat(oscMessage);
	}
	
	public void BreathingRawMessage(OscMessage oscMessage)
 	{
		int[] breathing = oscMessageToIntArray(oscMessage);
		plotter.Stream(breathing);
	}
	
	public void SetBreathingRawListener(BreathingRawListener breathingRawListener) {
		_breathingRawListener = breathingRawListener;
	}
	
	private int oscMessageToInt(OscMessage oscMessage) {
		return int.Parse((string)oscMessage.Values[0]);
	}
	
	private float oscMessageToFloat(OscMessage oscMessage) {
		return float.Parse((string)oscMessage.Values[0]);
	}
	
	private int[] oscMessageToIntArray(OscMessage oscMessage) {
		string[] split = ((string)oscMessage.Values[0]).Trim().Split(" "[0]);
		int[] result = new int[split.Length];
		
		for (var i = 0; i < split.Length; ++i) {
			result[i] = int.Parse(split[i]);
		}
		return result;
	}
	
	
}
