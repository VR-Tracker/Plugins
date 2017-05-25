using UnityEngine;
using System.Collections;
using System;

public class VRTrackerTag : MonoBehaviour {

	public Vector3 positionOffset; 
	public Vector3 orientationOffset; 
	public string status;
	public int battery;

	private Vector3 position;
	private Vector3 orientation;
	private Vector3 orientationBegin;
	private int initialOrientationSetCounter = 0;

	public int orientationEnbaled = 0;

	public event Action OnDown;                                 // Called when Fire1 is pressed.
	public event Action OnUp;                                   // Called when Fire1 is released.

	public string UID = "Enter Your Tag UID";

	Boolean commandReceived = false;
	String command;

	// Use this for initialization
	void Start () {
	}

	void sayHello(){
		Debug.Log ("Hello, I'm " + UID);
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position =  this.position + positionOffset;
		this.transform.rotation = Quaternion.Euler (orientation);

		if (commandReceived) {
			commandReceived = false;
			if (command.Contains ("triggeron")) {
				if (OnDown != null)
					OnDown ();
			}
			//else if (command.Contains ("triggeroff"))
			//	OnUp ();
		}
		
	}

	public void updatePosition(Vector3 position){
		this.position = position + positionOffset;
	}

	public void updateOrientation(Vector3 orientation){
		if (initialOrientationSetCounter == 50) {
			orientationBegin.y = orientation.y;
			initialOrientationSetCounter++;
		} else if (initialOrientationSetCounter < 50) {
			initialOrientationSetCounter++;
		} else {
			this.orientation = orientation + orientationOffset - orientationBegin;
		}

	}

	public void onSpecialCommand(string data){
		commandReceived = true;
		command = data;
		Debug.Log ("VR Tracker : special command - " + data);

	}
}
