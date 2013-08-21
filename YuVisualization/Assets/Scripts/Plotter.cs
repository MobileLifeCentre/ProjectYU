using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plotter : MonoBehaviour {
	public bool Visible {
		get {
			return _graph.renderer.enabled;	
		}
		set {
			_graph.renderer.enabled = value;
		}	
	}
	
	private List<PlotPoint> _data;
	private List<int> _rawData;
	private LineRenderer _graph;
	// TO-DO make resolution be adaptative to current values
	private Vector2 _resolution = new Vector2(2000, 600);
	private Vector3 _leftDown;
	private Vector3 _rightUp;
	private Vector2 _worldSize;
	private int _lastProcessed = 0;
	private bool _redraw = false;
	
	// Values needed to process data and generate breath counts
	private int _averageWindowSize = 10;
	private int _peakDistance = 10;
	private int _lastIndex = 0;
	private int _lastPeak = 0;
	private int _numBreaths = 0;
	
	public struct PlotPoint 
	{
	   public float point;
	   public int peak;
	}
	
	public delegate void BreathingListener();
	public BreathingListener breathingListener; 
	
	void Start () {
		_graph = gameObject.AddComponent<LineRenderer>();	
		_graph.SetWidth(0.1f,0.1f);
		Visible = false;
		
		_data = new List<PlotPoint>();
		_rawData = new List<int>();
		_lastIndex = _averageWindowSize/2;
		_lastProcessed = _averageWindowSize/2;
		
		_leftDown = Camera.mainCamera.ScreenToWorldPoint(new Vector3(0,0,1.5f));
		_rightUp = Camera.mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,1.5f));
		_worldSize = new Vector2(_rightUp.x - _leftDown.x, _rightUp.z - _leftDown.z);
	}
	
	public void Stream(int[] data) {
		// We add the new values
		for(int i = 0; i < data.Length; ++i) {
			if (data[i] < 1000) { // cheap high-pass filter
				_rawData.Add(data[i]);
			}
		}
		
		// smoothing signal by averaging
		int diff = (_rawData.Count - _lastProcessed) - _averageWindowSize/2;
		for (int j = 0; j < diff; ++j) {
			float total = 0;
			for (int i = -_averageWindowSize/2; i < _averageWindowSize/2; ++i) {
				total += _rawData[_lastProcessed+i];
			}
			_lastProcessed++;
			
			PlotPoint p = new PlotPoint();
			p.point = total/_averageWindowSize;
			p.peak = 1;
			_data.Add(p);
		}
		
		// We delete values outside the window
		int extraElements = _data.Count - (int)_resolution.x;
		if (extraElements > 0) {
			_data.RemoveRange(0, extraElements);
			_lastIndex -= extraElements;
			_lastPeak -= extraElements;
		}
		
		extraElements = _rawData.Count - (int)_resolution.x;
		if (extraElements > 0) {
			_rawData.RemoveRange(0, extraElements);
			_lastProcessed -= extraElements;
		}
		
		_redraw = true;
	}
	
	private void Update() {
		if (_redraw) {
			RenderLine();
			ProcessData();
		}
	}
	 
	// Process Data to detect Breaths using raw or smoothed data
	private void ProcessData() {
		int diff = (_data.Count - _lastIndex) - _averageWindowSize/2;
		while (diff > 0) {
			diff--;
			// Improvement: look at derivate sign
			
			// simple Gaussian peak detection
			// can be optimized by reusing sums using a queue/list
			if (_lastIndex - _lastPeak > _peakDistance) {
				float prev = 0;
				float post = 0;
				for (int j = 0; j+1 < _averageWindowSize/2; ++j) {
					prev += (_data[_lastIndex-j].point - _data[_lastIndex-j - 1].point);
					post += (_data[_lastIndex+j+1].point - _data[_lastIndex+j].point);
				}
				
				if (prev > 0 && post < 0) {
					PlotPoint point = _data[_lastIndex];
					point.peak = 0;
					_data[_lastIndex] = point;
					_lastPeak = _lastIndex;
					_numBreaths++;
					if (breathingListener != null) breathingListener();
					Debug.Log ("BREATH");
				}
				/*if (prev < 0 && post > 0) {
					PlotPoint point = _data[_lastIndex];
					point.peak = 100;
					_lastPeak = _lastIndex;
					_data[_lastIndex] = point;
				}*/
			}
			_lastIndex++;
		}
	}
	
	void RenderLine() {
		_graph.SetVertexCount(_data.Count);
		
		for (int i = 0; i < _data.Count; ++i) {
			float point = _data[i].point;
			int multiplier = _data[i].peak;
			
			_graph.SetPosition(i, _leftDown + new Vector3(_worldSize.x/_resolution.x*i,0,_worldSize.y/_resolution.y*multiplier*point));
		}
		_redraw = false;
	}
	
}
