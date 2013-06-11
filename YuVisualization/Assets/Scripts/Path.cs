using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Path : MonoBehaviour {
	private Color _color;
	
	public void Start() {
		_color = new Color(Random.Range (0.0f,1.0f),Random.Range (0.0f,1.0f),Random.Range (0.0f,1.0f));
		Visibility(false);	
	}
	
	public void OnDrawGizmosSelected() {
		Path[] paths = FindObjectsOfType(typeof(Path)) as Path[];
		
		foreach (Path path in paths) {
			if (path.gameObject.GetInstanceID() != gameObject.GetInstanceID()) {
				path.Visibility(false);
			}
		}
		Visibility(true);
		
		// We draw line between each of the elements in the path
		Transform points = transform.GetChild(0).transform;
		Vector3 prevPoint = Vector3.zero;
		foreach(Transform go in points) {
			if (prevPoint != Vector3.zero) {
				Debug.DrawLine(prevPoint, go.transform.position, _color);
			} else {
				Gizmos.DrawCube(go.transform.position, Vector3.one*2);
			}
			prevPoint = go.transform.position;
		}
	}
	
	public void Visibility(bool visible) {
		transform.GetChild(0).gameObject.SetActive(visible);
	}
	
	public Vector3[] ToArray() {
		Transform child = transform.GetChild(0);
		Vector3[] points = new Vector3[child.childCount];
		for (int i = 0; i < child.childCount; ++i) {
			points[i] = child.GetChild(i).transform.position;
		}
		
		return points;
	}
	
	public List<Vector3> ToList() {
		Transform child = transform.GetChild(0);
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < child.childCount; ++i) {
			list.Add(child.GetChild(i).transform.position);
		}
		
		return list;
	}
}
