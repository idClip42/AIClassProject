using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCameraController : MonoBehaviour {

    //set the sensitivity of camera movement in the inspector
    public float CAMERA_MOVE_SPEED;

    //prefab that we will use for units (capsule) - color & strength are set at time of instantiation
    public GameObject UNIT_PREFAB;


    //the last gameobject that we instantiated (so we can access its color and strength)
    private GameObject _lastAdded;

    // Use this for initialization
    void Start () {
        if (!UNIT_PREFAB) Debug.LogError("No Unit Prefab has been assigned in the inspector!");
	}
	
	// Update is called once per frame
	void Update () {
		

        //GETKEY for camera movement


        //W Pressed, move +Z
        if(Input.GetKey(KeyCode.W))
        {
            this.transform.Translate(new Vector3(0, CAMERA_MOVE_SPEED * Time.deltaTime, 0));
        }
        //S Pressed, move -Z
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.Translate(new Vector3(0, -CAMERA_MOVE_SPEED * Time.deltaTime, 0));
        }
        //D Pressed, move +X
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.Translate(new Vector3(CAMERA_MOVE_SPEED * Time.deltaTime, 0, 0));
        }
        //A Pressed, move -X
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Translate(new Vector3(-CAMERA_MOVE_SPEED * Time.deltaTime, 0, 0));
        }
        //E Pressed, move +Y
        if (Input.GetKey(KeyCode.E))
        {
            this.transform.Translate(new Vector3(0, 0, CAMERA_MOVE_SPEED * Time.deltaTime));
        }
        //Q Pressed, move -Y
        if (Input.GetKey(KeyCode.Q))
        {
            this.transform.Translate(new Vector3(0, 0, -CAMERA_MOVE_SPEED * Time.deltaTime));
        }


        //KEYDOWN for instantiating new units & MISC


        //Z Pressed, instantiate 1-strength unit (White)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _lastAdded = GameObject.Instantiate(UNIT_PREFAB);
            _lastAdded.GetComponent<InfluenceUnit>().InitUnit(1);
        }
        //X Pressed, instantiate 2-strength unit (Blue)
        else if (Input.GetKeyDown(KeyCode.X))
        {
            _lastAdded = GameObject.Instantiate(UNIT_PREFAB);
            _lastAdded.GetComponent<InfluenceUnit>().InitUnit(2);
        }
        //C Pressed, instantiate 3-strength unit (Yellow)
        else if (Input.GetKeyDown(KeyCode.C))
        {
            _lastAdded = GameObject.Instantiate(UNIT_PREFAB);
            _lastAdded.GetComponent<InfluenceUnit>().InitUnit(3);
        }
        //V Pressed, instantiate 4-strength unit (Black)
        else if (Input.GetKeyDown(KeyCode.V))
        {
            _lastAdded = GameObject.Instantiate(UNIT_PREFAB);
            _lastAdded.GetComponent<InfluenceUnit>().InitUnit(4);
        }



        //F Pressed, re-generate influence map
        if(Input.GetKeyDown(KeyCode.F))
        {
            //stub
        }

        //R Pressed, restart simulation
        if(Input.GetKeyDown(KeyCode.R))
        {
            //stub
        }
    }


}//end FPCameraController.cs
