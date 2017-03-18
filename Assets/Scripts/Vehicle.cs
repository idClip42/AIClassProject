using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Vehicle.cs - Represents an Individual Flocker
public class Vehicle : MonoBehaviour {

    //external references
    private Rigidbody myRigidBody;
    private GameManager gm;             //the singleton script

    //flocking attributes
    private Transform currentTarget;      //where the flocker is navigating to
    private Vector3 desiredVelocity;    //used for steering calculations
    private bool isPathing;             //is the flocker currently navigating somewhere




    //called once at start of program
    //initialize fields
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        desiredVelocity = new Vector3();
        myRigidBody = this.GetComponent<Rigidbody>();
        //currentTarget = this.transform;
		currentTarget = GameObject.FindGameObjectWithTag("AStarAgent").transform;
        isPathing = true;
	}

	
	//fixedupdate is called once per physics tick
    //may or may not be once per frame
	void FixedUpdate ()
    {
        //CheckArrival();
        UpdateFlocker();
	}


    //sets the flocker's current target & flags it as pathing
    //PARAMS: Vector3 for target
    public void SetCurrentTarget(Transform target)
    {
        currentTarget = target;
        isPathing = true;
    }

    private void CheckArrival()
    {
        //If we're within range of current target, we're done pathing
        if (Vector3.Distance(this.transform.position, currentTarget.position) < gm.ArrivalBoundary)
        {
            //currentTarget = transform;
            //isPathing = false;
        }
    }

    //updates the flocker
    private void UpdateFlocker()
    {
		
        //if we're not pathing, make us arrive (and don't calc any steering)
        //if (!isPathing)
		//If we're within range of current target, we're done pathing
		if (Vector3.Distance(this.transform.position, currentTarget.position) < gm.ArrivalBoundary)
        {
            //decelerate to the object until we're almost stopped (drag will take care of the rest)
            if(myRigidBody.velocity.sqrMagnitude > 1) myRigidBody.AddForce(myRigidBody.velocity * gm.ArrivalDrag * -1, ForceMode.Acceleration);
            return;
        }



        //Calculate steering
        CalcSteeringForces();

        //aim the flocker forward by lineraly interpolating toward velocity
        this.transform.forward = Vector3.Lerp(
            this.transform.forward,
            myRigidBody.velocity,
            0.1f
        );
    }

    //calculates the steering forces on this object
    //when finished, adds the force to this flocker's rigidbody
    private void CalcSteeringForces()
    {
        //create a new vector to represent the seek force
        Vector3 seekForce = new Vector3();

        //seeks to current target
        seekForce += Seek(currentTarget.position) * gm.SeekingWeight;

        //flocking behaviors, weighted for smoother simulation
        //the weights are coming from public variables in the game manager
        seekForce += Separation() * gm.SeparationWeight;
        seekForce += Cohesion() * gm.CohesionWeight;
        seekForce += Alignment() * gm.AlignmentWeight;

        //avoid obstacles
        seekForce += AvoidObstacle() * gm.ObstacleAvoidanceWeight;


        //limit the steering force before applying
        seekForce = Vector3.ClampMagnitude(seekForce, gm.FlockerMaxForce);
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
        myRigidBody.AddForce(steeringForce, ForceMode.Acceleration);
    }

    //returns a seeking force to a position
    //PARAMS: Vector3 for target position
    Vector3 Seek(Vector3 targetPos)
    {
        //gets a normalized vector between object and target, multiplied by max speed of an object
        Vector3 desired = targetPos - transform.position;
        desired.Normalize();
        desired *= gm.FlockerMaxSpeed;
        
        //gets the steering vector (desired - velocity)
        Vector3 steer = desired - myRigidBody.velocity;
        return steer;
    }

    //returns steering force to avoid obstacles
    //PARAMS: A GameObject representing the obstacle to avoid,
    //        and a float for the distance at which the flocker
    //        should start to try and avoid the obstacle.
    Vector3 AvoidObstacle()
    {
        desiredVelocity = Vector3.zero;

        //get a "ray" from us in the direction of our velocity
        //Ray lookAhead = new Ray(this.transform.position, myRigidBody.velocity.normalized);
        Vector3[] directions = new Vector3[] { myRigidBody.velocity, Quaternion.AngleAxis(45, Vector3.up) * myRigidBody.velocity, Quaternion.AngleAxis(-45, Vector3.up) * myRigidBody.velocity };

        for(int i=0; i < directions.Length; i++)
        {
            Ray lookAhead = new Ray(this.transform.position, directions[i]);
            RaycastHit hit;

            //sends the ray out and sees if it hits anything
            if (Physics.Raycast(lookAhead, out hit, gm.ObstacleAvoidanceDistance))
            {
                //only calculate if it's an obstacle
                if (hit.collider.gameObject.tag != "Obstacle") continue;

                Vector3 pointOnCollider = hit.point;

                //calculate vector between
                Vector3 between = pointOnCollider - this.transform.position;

                desiredVelocity -= between;
            }
        }

        desiredVelocity.Normalize();
        desiredVelocity *= gm.FlockerMaxSpeed;
        return desiredVelocity;
    }

    //returns a steering force to make separation between others
    Vector3 Separation()
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
            if (Vector3.Distance(this.transform.position, flocker.transform.position) < gm.SeparationDistance)
            {
                vtc = this.transform.position - flocker.transform.position;
                //increment steering vector
                desiredVelocity += (vtc);
            }
        }

        //normalize the final steering vector
        desiredVelocity.Normalize();
        desiredVelocity *= gm.FlockerMaxSpeed;
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
