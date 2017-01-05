using UnityEngine;
using System.Collections;

public class VRTrackerTag : MonoBehaviour {

	public Vector3 positionOffset; 
	public Vector3 orientationOffset; 
	public string status;
	public int battery;

	private Vector3 position;
	private Vector3 orientation;

	public int orientationEnbaled = 0;

	public string UID = "Enter Your Tag UID";

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position =  this.position;
		this.transform.rotation = Quaternion.Euler (orientation);
	}

	public void updatePosition(Vector3 position){
		this.position = position + positionOffset;
	}

	public void updateOrientation(Vector3 orientation){
		this.orientation = orientation + orientationOffset;
	}

	public void onSpecialCommand(string data){
		Debug.Log ("VR Tracker : special command - " + data);

	}
}
