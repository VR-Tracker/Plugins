using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class VRTracker : MonoBehaviour {

	private WebSocket myws;
	private Vector3 position;
	private Vector3 orientation;
	private bool orientationEnabled = false;
	public Transform CameraTransform; 
	public Vector3 offset; 
	// Use this for initialization
	void Start () {
		openWebsocket ();
	}
	
	// Update is called once per frame
	void Update () {
		CameraTransform.transform.position = position;
		//if (orientationEnabled)
		//	camera.transform.rotation = orientation;
	}


	private void OnOpenHandler(object sender, System.EventArgs e) {
		Debug.Log("VR Tracker : connection established");
		//new WaitForSeconds(3);
		myws.SendAsync ("cmd=mac&uid=ABC123", OnSendComplete);
		assignATag ();
	}
	
	private void OnMessageHandler(object sender, MessageEventArgs e) {
		//Debug.Log ("VR Tracker : " + e.Data);
		if (e.Data.Contains ("cmd=position")) {
			string[] datas = e.Data.Split ('&');
			foreach (string data in datas){
				string[] datasplit = data.Split ('=');

				// Position
				if(datasplit[0] == "x"){
					position.x = float.Parse(datasplit[1]) + offset.x;
				}
				else if(datasplit[0] == "z"){
					position.y = float.Parse(datasplit[1]) + offset.y;
				}
				else if(datasplit[0] == "y"){
					position.z = float.Parse(datasplit[1]) + offset.z;
				}

				// Orientation
				else if(datasplit[0] == "ox"){
					orientation.x = float.Parse(datasplit[1]);
				}
				else if(datasplit[0] == "oy"){
					orientation.y = float.Parse(datasplit[1]);
				}
				else if(datasplit[0] == "oz"){
					orientation.z = float.Parse(datasplit[1]);
				}
			}

		} else if (e.Data.Contains ("cmd=specialcmd")) {
			string[] datas = e.Data.Split ('&');
			string uid = null;
			string command = null;
			foreach (string data in datas){
				string[] datasplit = data.Split ('=');
				
				// Tag UID sending the special command
				if(datasplit[0] == "uid"){
					uid = datasplit[1];
				}
								
				// Special Command
				else if(datasplit[0] == "data"){
					command = datasplit[1];
				}
			}
			if(uid != null && command != null)
				receiveSpecialCommand(uid, command);
			
		} else if (e.Data.Contains ("cmd=taginfos")) {
			string[] datas = e.Data.Split ('&');

			string uid = null;
			string status = null;
			int battery = 0;

			foreach (string data in datas){
				string[] datasplit = data.Split ('=');
				
				// Tag UID sending its informations
				if(datasplit[0] == "uid"){
					uid = datasplit[1];
				}
				// Tag status (“lost”, “tracking”, “unassigned”)
				else if(datasplit[0] == "status"){
					status = datasplit[1];
				}
				// Tag battery
				else if(datasplit[0] == "battery"){
					battery = int.Parse(datasplit[1]);
				}
			}
			if(uid != null && status != null)
				receiveTagInformations(uid, status, battery);
			
		}
		else if (e.Data.Contains ("cmd=error")) {
			// TODO Parse differnt kinds of errors
			myws.SendAsync ("cmd=mac&uid=ABC123", OnSendComplete);
			assignATag ();

		} else {
			Debug.Log ("VR Tracker : Unknown data received : " + e.Data);
		}
	}
	
	private void OnCloseHandler(object sender, CloseEventArgs e) {
		Debug.Log("VR Tracker : connection closed for this reason: " + e.Reason);
	}
	
	private void OnSendComplete(bool success) {
		Debug.Log("VR Tracker : Send Complete");
	}

	/*
	 * Opens the websocket connection with the Gateway
	 */
	private void openWebsocket(){
		Debug.Log("VR Tracker : opening websocket connection");
		myws = new WebSocket("ws://vrtracker.local:7777/user/");
		myws.OnOpen += OnOpenHandler;
		myws.OnMessage += OnMessageHandler;
		myws.OnClose += OnCloseHandler;
		myws.ConnectAsync();	
	}

	/*
	 * Close the ebsocket connection to the Gateway
	 */
	private void closeWebsocket(){
		Debug.Log("VR Tracker : closing websocket connection");
		this.myws.Close();
	}


	/* 
	 * Send your Unique ID, it can be your MAC address for 
	 * example but avoid the IP. It will be used by the Gateway
	 * to identify you over the network. It is necessary on multi-gateway
	 * configuration 
	 */
	private void sendMyUID(string uid){
		myws.SendAsync(uid, OnSendComplete);
	}

	/* 
	 * Asks the gateway to assign a specific Tag to this device.  
	 * Assigned Tags will then send their position to this device.
	 */
	public void assignTag(string TagID){
		myws.SendAsync("cmd=tagassign&uid=" + TagID, OnSendComplete);
	}

	/* 
	 * Asks the gateway to assign a Tag to this device.  
	 * Assigned Tags will then send their position to this device.
	 */
	public void assignATag(){
		myws.SendAsync("cmd=assignatag", OnSendComplete);
	}

	/* 
	 * Asks the gateway to UNassign a specific Tag from this device.  
	 * You will stop receiving updates from this Tag.
	 */
	public void unAssignTag(string TagID){
		myws.SendAsync("cmd=tagunassign&uid=" + TagID, OnSendComplete);
	}

	/* 
	 * Asks the gateway to UNassign all Tags from this device.  
	 * You will stop receiving updates from any Tag.
	 */
	public void unAssignAllTags(){
		myws.SendAsync("cmd=tagunassignall", OnSendComplete);
	}

	/* 
	 * Ask for informations on a specific Tag
	 */
	public void getTagInformations(string TagID){
		myws.SendAsync("cmd=taginfos&uid=" + TagID, OnSendComplete);
	}

	/*
	 * Enable or Disable orientation detection for a Tag
	 */
	public void TagOrientation(string TagID, bool enable){
		string en = "";
		if (enable) {
			orientationEnabled = true;
			en = "true";
		} else {
			orientationEnabled = false;
			en = "false";
		}

		myws.SendAsync("cmd= orientation&orientation=" + en + "&uid=" + TagID, OnSendComplete);
	}

	/*
	 * Set a specific color on the Tag
	 * R (0-255)
	 * G (0-255)
	 * B (0-255)
	 */
	public void setTagColor(string TagID, int red, int green, int blue){
		myws.SendAsync("cmd= color&r=" + red + "&g=" + green + "&b=" + blue + "&uid=" + TagID, OnSendComplete);
	}


	/* 
	 * Send special command to a Tag
	 */
	public void sendTagCommand(string TagID, string command){
		myws.SendAsync("cmd=specialcmd&uid=" + TagID + "&data=" + command, OnSendComplete);
	}

	/* 
	 * Send User device battery level to the Gateway
	 * battery (0-100)
	 */
	public void sendUserBattery(int battery){
		myws.SendAsync("cmd=usrbattery&battery=" + battery, OnSendComplete);
	}

	/*
	 * Executed on reception of a special command 
	 */
	public void receiveSpecialCommand(string TagID, string data){
		// TODO: You can do whatever you wants with the special command, have fun !
	}

	/*
	 * Executed on reception of  tag informations
	 */
	public void receiveTagInformations(string TagID, string status, int battery){
		// TODO: You can do whatever you wants with the Tag informations
	}

	/* 
	 * Ensure the Websocket is correctly closed on application quit
	 */
	void OnApplicationQuit() {
		closeWebsocket ();
	}

}
