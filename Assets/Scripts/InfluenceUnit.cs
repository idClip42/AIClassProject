using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceUnit : MonoBehaviour {

    //the strength of this unit, with accessor
    private int _strength;
    public int Strength { get; }


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //sets up this unit to be the strength passed in
    public void InitUnit(int str)
    {
        //identify the strength and set color
        switch (str)
        {
            case 1:     //white
                //prefab is white by default
                break;
            case 2:     //blue
                this.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case 3:     //yellow
                this.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case 4:     //black
                this.GetComponent<Renderer>().material.color = Color.black;
                break;
            default:    //any other number that's passed in
                Debug.Log("Strength of unit is out of bounds, setting to 1...");
                str = 1;
                break;
        }

        //set Strength (will be changed to 1 if out of bounds) and position
        _strength = str;
        this.transform.position = setUpPosition();
    }


    //helper method to set up position of unit
    private Vector3 setUpPosition()
    {
        //see if the mouse is over the terrain
        RaycastHit hit;
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if we hit the terrain
        if(Physics.Raycast(r, out hit, 1000))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = hit.distance;

            //get the mouse position in world space and adjust Y to terrain height
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            worldMousePos.y = GameObject.FindGameObjectWithTag("Terrain")
                .GetComponent<Terrain>().SampleHeight(worldMousePos)
                + 1.1f;

            //return mouse pos w/ terrain height adjustment
            return worldMousePos;
        }

        //if we get here, we didn't hit it, so return 0
        return Vector3.zero;
    }
}
