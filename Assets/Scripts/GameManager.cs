
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    //a prefab for generating tree box colliders
    public GameObject TreeBoxPrefab;

    //FLOCKING PARAMS - TO BE CHANGED IN THE INSPECTOR
    //These can be manipulated by key presses for project 2
    [Header("Flocking Parameters")]
    public float ArrivalBoundary;               //the distance from target when we begin to arrive
    public float ObstacleAvoidanceDistance;     //the "safe distance" away from obstacles before we start to steer
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
    //private AStarNode target;

    //--flocking
    private Vector3 centroid = new Vector3();
    private Vector3 avgFlockDir = new Vector3();

    //property for obstacles/flockers
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

        //store all flockers (should have tag "Flocker") in an array
        flockers = GameObject.FindGameObjectsWithTag("Flocker");
        //Debug.Log("There are " + flockers.Length + " objects in the flocker array.");

        //add all "obstacles"
        //these can be walls, houses, the A* character, whatever.
        //I feel like this array will get large quickly - binning may help down the road
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        Debug.Log("There are " + obstacles.Length + " objects in the obstacle array.");
    }
	
    //update method is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.P)) GetNewTarget();

        //calculates the centroid and flock direction
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

    //gets a random AStarNode to flock to
    //since the flockers extend from AStarAgents, they should
    //be able to handle this correctly
    private void GetNewTarget()
    {

        foreach(GameObject f in flockers)
        {
            f.GetComponent<Vehicle>().SetCurrentTarget(
                GameObject.FindGameObjectWithTag("Player").transform.position);
        }

        //AStarNode randomNode = AStarPathfinder.pathfinder.RandomNode;

        //assigns path to the first flocker, which sends it to the rest
        //(the way I do this right now is a bit janky, but only because
        //all of the flockers can't find a path due to closed nodes)
        //if (flockers.Length > 0)flockers[0].GetComponent<Vehicle>().AssignFlockerPath(randomNode.transform.position);
    }
}
