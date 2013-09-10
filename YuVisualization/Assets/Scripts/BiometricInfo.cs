using UnityEngine;
using System.Collections;
using System.Collections.Generic;



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
	private string _breathingSample = "";
	
	[HideInInspector]
	public float heartRate = 0.0f;
	public delegate void BreathingRawListener(int[] breathing);
	public BreathingRawListener _breathingRawListener = null;
	
	
	// GSR
	private List<int> _rawGSR = new List<int>();
	private int _GSRBufferSize = 20;
	private int _currentGSR = 0;
	private float _sumGSR = 0;
	public int _countGSR = 0;
	public int _maxGSR = -1;
	public int _minGSR = 10000; //find int library for maxInt 
	public float _medianGSR = 0;
	
	//IBI
	public bool beat = false;
	
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
		_handler.SetAddressHandler("/GSR", GSRMessage);
		_handler.SetAddressHandler("/IBI", IBIMessage);
		_handler.SetAddressHandler("/BPM", BPMMessage);
		_handler.SetAddressHandler("/Beat", BeatMessage);
	}
	
	public void Update() {
		_info = _handler.GetInformation();
		
		if (Input.GetKeyDown(KeyCode.D)) {
			_visible = !_visible;
			plotter.Visible = _visible;
		}
	}
	
	public void LateUpdate() {
		beat = false;	
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
	
	public void GSRMessage(OscMessage oscMessage)
 	{
		int gsrValue = oscMessageToInt(oscMessage);
 		_rawGSR.Add(gsrValue);
		_sumGSR += gsrValue;
		_countGSR += 1;
		_medianGSR = _sumGSR/_GSRBufferSize;
		if (ValidValue(gsrValue)) {
			_maxGSR = (int)Mathf.Max (_maxGSR, gsrValue);
			_minGSR = (int)Mathf.Min (_minGSR, gsrValue);
		}
		
		
		// We delete values outside the window
		int extraElements = _rawGSR.Count - _GSRBufferSize;
		if (extraElements > 0) {
			for (int i = 0; i < extraElements; ++i) {
				_sumGSR -= _rawGSR[i];	
			}
			_rawGSR.RemoveRange(0, extraElements);
		}
	}
	
	private bool ValidValue(float currentValue) {
		return _countGSR < 10 || _maxGSR - _minGSR == 0 || Mathf.Abs (currentValue - _medianGSR) < (_maxGSR - _minGSR);
	}
	
	public void IBIMessage(OscMessage oscMessage) {
		int ibiValue = oscMessageToInt(oscMessage);
		//Debug.Log (ibiValue);
		GSRMessage(oscMessage);
		//Debug.Log (ibiValue);
	}
	
	public void BPMMessage(OscMessage oscMessage) {
		int bpmValue = oscMessageToInt(oscMessage);
		//Debug.Log (bpmValue);
	}
	
	public void BeatMessage(OscMessage oscMessage) {
		Debug.Log ("beatttt");
		beat = true;
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
		return int.Parse(oscMessage.Values[0] + "");
	}
	
	private float oscMessageToFloat(OscMessage oscMessage) {
		return float.Parse(oscMessage.Values[0] +"");
	}
	
	private int[] oscMessageToIntArray(OscMessage oscMessage) {
		string[] split = (oscMessage.Values[0] +"").Trim().Split(" "[0]);
		int[] result = new int[split.Length];
		
		for (var i = 0; i < split.Length; ++i) {
			result[i] = int.Parse(split[i]);
		}
		return result;
	}
}
