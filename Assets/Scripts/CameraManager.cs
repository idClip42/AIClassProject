///CameraManager.cs
///By Alec Bielanos, 2/19/2017
///The purpose of this class is to cycle through the cameras
///Based on input (keyboard 'C')
///This is just for this milestone, this will probably be implemented elsewhere later
///But for now, it's attached to our singleton

using UnityEngine;

public class CameraManager : MonoBehaviour {

    private GameObject playerCamera;

	// Use this for initialization
	void Start () {
        //gets player camera and turns it off at start
        playerCamera = GameObject.Find("PlayerCamera");
        playerCamera.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.C))
            playerCamera.SetActive(!playerCamera.activeSelf);
	}
}
