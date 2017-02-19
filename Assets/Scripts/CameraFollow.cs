///CameraFollow.cs
///By Alec Bielanos, 2/19/2017
///The purpose of this class is to follow a target from behind (3rd person view)
///The distance at which to follow and the object to follow should be set in the inspector
///This class should be attached to the camera that will follow the object

using UnityEngine;

public class CameraFollow : MonoBehaviour {

    //define/modify publics in the inspector
    public GameObject camTarget;
    public float followDistance;

    private Vector3 followOffset;


	// Use this for initialization
	void Start () {
        followOffset = Vector3.zero;
	}
	
	// FixedUpdate once per physics tick (may or may not occur once per frame)
	void FixedUpdate () {

        //get a vector representing the distance between camera and target
        //followOffset = camTarget.transform.forward * followDistance * -1;
        followOffset = Vector3.forward * followDistance * -1;

        //apply that to this position (with relation to target position)
        this.transform.position = camTarget.transform.position + followOffset;

        //give some y offset
        this.transform.Translate(0, 5, 0);

        //rotate cam down to look at player
        this.transform.LookAt(camTarget.transform);
	}
}
