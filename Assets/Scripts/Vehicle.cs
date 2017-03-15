using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Vehicle : MonoBehaviour {
    private GameManager gm;
    private Vector3 currentTarget;
    private List<float> tooCloseFlockers;
    private Rigidbody myRigidBody;
    private Vector3 desiredVelocity;    //used for steering calculations
    private float maxSpeed;
    private float maxForce;
    private Vector3 desiredAcceleration;
    private CharacterController controller;
    private bool isPathing;


    //called once at start of program
    //initialize fields
	void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        tooCloseFlockers = new List<float>();
        desiredVelocity = new Vector3();
        maxSpeed = gm.FlockerMaxSpeed;
        maxForce = gm.FlockerMaxForce;
        controller = this.GetComponent<CharacterController>();
        myRigidBody = this.GetComponent<Rigidbody>();
        currentTarget = this.transform.position;
        isPathing = false;
	}

	
	//fixedupdate is called once per physics tick
    //may or may not be once per frame
    //overrides base class to account for overridden pathfinding update
	void FixedUpdate ()
    {
        CheckArrival();
        Move();
	}

    public void SetCurrentTarget(Vector3 target)
    {
        currentTarget = target;
        isPathing = true;
    }

    //a special pathfinding update for Flockers
    void CheckArrival()
    {
        //If we're within range of current target, we're done pathing
        if (Vector3.Distance(this.transform.position, currentTarget) < gm.ArrivalBoundary)
        {
            currentTarget = transform.position;
            isPathing = false;
        }
    }

    void Move()
    {
        //if we're not pathing, make us arrive (and don't calc any steering)
        if (!isPathing)
        {
            if(myRigidBody.velocity.sqrMagnitude > 1) myRigidBody.AddForce(myRigidBody.velocity * gm.ArrivalDrag * -1);
            return;
        }

        //(input movement of flockers obtained from CalcSteeringForces()
        CalcSteeringForces();

        this.transform.forward = myRigidBody.velocity.normalized;

        desiredAcceleration *= 0;
    }

    //calculates the steering forces on this object
    //RETURNS: Overall Steering Force
    private void CalcSteeringForces()
    {
        //create a new vector to represent the seek force
        Vector3 seekForce = new Vector3();

        //seeks to current target (see base class)
        seekForce += Seek(currentTarget) * gm.SeekingWeight;

        //flocking behaviors, weighted for smoother simulation
        //the weights are coming from public variables in the game manager
        seekForce += Separation(gm.SeparationDistance) * gm.SeparationWeight;
        seekForce += Cohesion() * gm.CohesionWeight;
        seekForce += Alignment() * gm.AlignmentWeight;

        //obstacle avoidance - weight it
        foreach (GameObject obst in gm.Obstacles)
        {
            seekForce += AvoidObstacle(obst, gm.ObstacleAvoidanceDistance) * gm.ObstacleAvoidanceWeight;
        }

        //limit the steering force before applying
        seekForce = Vector3.ClampMagnitude(seekForce, maxForce);
        ApplyForce(seekForce);  
    }



    /// <summary>
    /// HELPER METHODS
    /// These methods are used in the basic code above to perform specific functions
    /// like modules, so that we can keep our calcSteeringForces method mostly clean
    /// and easy to understand.
    /// </summary>


    //applies a force to this object's rigidbody
    void ApplyForce(Vector3 steeringForce)
    {
        //velocity += steeringForce;
        myRigidBody.AddForce(steeringForce);
    }

    //returns a seeking force to a position
    //PARAMS: Vector3 for target position
    Vector3 Seek(Vector3 targetPos)
    {
        //gets a normalized vector between object and target, multiplied by max speed of an object
        Vector3 desired = targetPos - transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        
        //gets the steering vector (desired - velocity)
        Vector3 steer = desired - myRigidBody.velocity;
        return steer;
    }

    //returns steering force to avoid obstacles
    //PARAMS: A GameObject representing the obstacle to avoid,
    //        and a float for the distance at which the flocker
    //        should start to try and avoid the obstacle.
    Vector3 AvoidObstacle(GameObject obst, float safeDist)
    {
        return Vector3.zero;    //stub for now, TODO
    }

    //returns a steering force to make separation between others
    //PARAMS: float for distance which the flockers should maintain between each other.
    Vector3 Separation(float separationDist)
    {
        //forward declaring variables to be used in the loop
        desiredVelocity = Vector3.zero;
        Vector3 vtc;

        //loops through all the flockers
        foreach(GameObject flocker in gm.Flockers)
        {
            //makes sure we are not looking at ourself
            if (this.Equals(flocker.GetComponent<Vehicle>())) continue;

            //only run if we're too close
            if (Vector3.Distance(this.transform.position, flocker.transform.position) < separationDist)
            {
                vtc = this.transform.position - flocker.transform.position;
                //increment steering vector
                desiredVelocity += (vtc);
            }
        }

        //normalize the final steering vector
        desiredVelocity.Normalize();
        desiredVelocity *= maxSpeed;
        return desiredVelocity;
    }


    //returns a Vector to help all flockers have the same direction
    Vector3 Alignment()
    {
        return gm.AverageFlockDirection - myRigidBody.velocity;
    }

    //seeks the flocker to the centroid for cohesion
    Vector3 Cohesion()
    {
        return Seek(gm.Centroid);
    }
}
