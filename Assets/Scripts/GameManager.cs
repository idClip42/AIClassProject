
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The GameManager class, which should be attached to the singleton
//Oversees in-game operations (mostly flocking for now)
public class GameManager : MonoBehaviour {

    //a prefab for generating tree box colliders (not used, but should be used to factor trees into obstacle avoidance later)
    //public GameObject TreeBoxPrefab;

    //FLOCKING PARAMS - TO BE CHANGED IN THE INSPECTOR
    //These can be manipulated by key presses for project 2
    [Header("Flocking Parameters")]
    public float ArrivalBoundary;               //the distance from target when we begin to arrive
    public float ObstacleAvoidanceDistance;     //the distance ahead that the object looks for obstacles
    public float SeparationDistance;            //distance to maintain between flockers

    //weighting - increase weight to factor that force
    //more into the steering calculation
    public float SeekingWeight;
    public float SeparationWeight;
    public float AlignmentWeight;
    public float CohesionWeight;
    public float ObstacleAvoidanceWeight;

    public float FlockerMaxSpeed;               //max speed that the flocker can achieve
    public float FlockerMaxForce;               //max magnitude of force (acceleration) applied to flocker each tick
    public float ArrivalDrag;                   //the amount of drag to apply when inside the arrival zone

    [Header("Debug Objects")]
    public GameObject centroidObj;              //centroid used for cohesion


    //attributes
    private GameObject[] obstacles;
    private GameObject[] flockers;

    //--flocking
    private Vector3 centroid;
    private Vector3 avgFlockDir;


    //properties for obstacles/flockers
    public GameObject[] Obstacles
    {
        get { return obstacles; }
    }
    public GameObject[] Flockers
    {
        get { return flockers; }
    }
    public Vector3 Centroid
    {
        get { return centroid; }
    }
    public Vector3 AverageFlockDirection
    {
        get { return avgFlockDir; }
    }

    //called once for setup purposes
	void Start () {

        centroid = new Vector3();
        avgFlockDir = new Vector3();

        //store all flockers (should have tag "Flocker") in an array
        flockers = GameObject.FindGameObjectsWithTag("Flocker");

        //add all "obstacles"
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

		//GetNewTarget();
    }
	
    //update method is called once per frame
	void Update () {

        //navigates the flock to the player when "p" is pressed
        //if (Input.GetKeyDown(KeyCode.P)) GetNewTarget();

		//controls for turning on or off separation, alignment, or cohesion
		if (Input.GetKeyDown (KeyCode.J)) {
			if (SeparationWeight == 10)
				SeparationWeight = 0;
			else if (SeparationWeight == 0)
				SeparationWeight = 10;
		}
		if (Input.GetKeyDown (KeyCode.K)) {
			if (AlignmentWeight == 1.8f)
				AlignmentWeight = 0;
			else if (AlignmentWeight == 0)
				AlignmentWeight = 1.8f;
		}
		if (Input.GetKeyDown (KeyCode.L)) {
			if (CohesionWeight == 1.8f)
				CohesionWeight = 0;
			else if (CohesionWeight == 0)
				CohesionWeight = 1.8f;
		}

		//controls for adding a new flocker, spawns the flocker on the centroid of he flock then adds it to the flocker array
		if (Input.GetKeyDown (KeyCode.H)) {
			Quaternion rot = new Quaternion (centroidObj.transform.eulerAngles.x, centroidObj.transform.eulerAngles.y, centroidObj.transform.eulerAngles.z, 0);
			Instantiate (Flockers [0], centroidObj.transform.position, rot);
			flockers = GameObject.FindGameObjectsWithTag("Flocker");
		}
        //calculates the centroid and flock direction (used in alignment and cohesion)
        CalcCentroid();
        CalcFlockDirection();
	}

    //calculates the centroid of the flock
    private void CalcCentroid()
    {
        centroid *= 0;

        //loops through flockers, gets average position (centroid)
        foreach (GameObject flocker in flockers)
        {
            centroid += flocker.transform.position;
        }
        centroid /= flockers.Length;
        
        //turn this on if you want to see the centroid as a sphere
        centroidObj.transform.position = centroid;
    }

    //calculates the average flock direction
    //flockers use this for alignment, since the average should be the same for all
    private void CalcFlockDirection()
    {
        avgFlockDir = Vector3.zero;

        //loops through flockers, gets average of forward vectors (flock direction)
        foreach (GameObject flocker in flockers)
        {
            avgFlockDir += flocker.GetComponent<Rigidbody>().velocity.normalized;
        }
        avgFlockDir /= flockers.Length;
        avgFlockDir *= FlockerMaxSpeed;
    }

    //loops through all the flockers and tells them to go
    //to the player's position
    private void GetNewTarget()
    {
        //Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
		Transform playerPos = GameObject.FindGameObjectWithTag("AStarAgent").transform;
        foreach (GameObject f in flockers)
        {
            f.GetComponent<Vehicle>().SetCurrentTarget(playerPos);
        }
    }

	void OnGUI()
	{
		GUI.Box(new Rect(0,50,200,100), "Flocking Controls:" +
			"\nJ - Separation Weight: " + SeparationWeight +
			"\nK - Alignment Weight: " + AlignmentWeight + 
			"\nL - Cohesion Weight: " + CohesionWeight +
			"\nH - Spawn Flocker");
	}

}
