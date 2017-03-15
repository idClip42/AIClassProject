using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//inherits from base AStarAgent, as flockers will also follow the A* path, but calculate their targets differently
public class Vehicle : AStarAgent {
    private GameManager gm;
    private List<float> tooCloseFlockers;
    private Rigidbody myRigidBody;
    private Vector3 desiredVelocity;    //used for steering calculations
    private float maxSpeed;
    private float maxForce;
    private Vector3 desiredAcceleration;

    //called once at start of program
    //initialize fields
	override protected void Start()
    {
        base.Start();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        tooCloseFlockers = new List<float>();
        myRigidBody = this.GetComponent<Rigidbody>();
        desiredVelocity = new Vector3();
        maxSpeed = gm.FlockerMaxSpeed;
        maxForce = gm.FlockerMaxForce;

	}

	
	//fixedupdate is called once per physics tick
    //may or may not be once per frame
    //overrides base class to account for overridden pathfinding update
	override protected void FixedUpdate () {
        PathfindingUpdate();
        MovePlayer();
        Animate();
	}

    //a special pathfinding update for Flockers
    protected override void PathfindingUpdate()
    {
        // If there is no nodeStack, do nothing
        if (nodeStack == null) return;

        // Gets the squared distance between agent and node and the squared node radius
        // This is to avoid square roots
        float distSqr = Vector3.SqrMagnitude(currentNodeTarget.position - transform.position);
        float radiusSqr = Mathf.Pow(gm.TargetChangeDistance, 2);

        // If the agent is within range of the node, gets a new target node
        if (distSqr < radiusSqr)
        {
            if (nodeStack.Count > 0)
            {
                currentNodeTarget = nodeStack.Pop();
                target = currentNodeTarget.transform.position;
            }
            else
            {
                currentNodeTarget = null;
                nodeStack = null;
                target = finalDestination;
            }
        }

        //if there's no more nodes to go to, just get a new path
        //if(nodeStack == null)
        //{
        //    AssignFlockerPath(AStarPathfinder.pathfinder.RandomNode.transform.position);
        //}
    }

    protected override void MovePlayer()
    {
        //(input movement of flockers obtained from CalcSteeringForces()
        CalcSteeringForces();

        velocity += desiredAcceleration * Time.fixedDeltaTime;

        Vector3.ClampMagnitude(velocity, gm.FlockerMaxSpeed);

        vertVelocity += Physics.gravity * Time.fixedDeltaTime;
        if (vertVelocity.y < Physics.gravity.y) vertVelocity = Physics.gravity;

        controller.Move((velocity + vertVelocity) * Time.fixedDeltaTime);

        desiredAcceleration *= 0;
    }

    //calculates the steering forces on this object
    //RETURNS: Overall Steering Force
    private void CalcSteeringForces()
    {
        //create a new vector to represent the seek force
        Vector3 seekForce = new Vector3();

        //seeks to current target (see base class)
        seekForce += Seek(target) * gm.SeekingWeight;

        //flocking behaviors, weighted for smoother simulation
        //the weights are coming from public variables in the game manager
        seekForce += Separation(gm.SeparationDistance) * gm.SeparationWeight; //30
        seekForce += Cohesion() * gm.CohesionWeight; //1.8
        seekForce += Alignment() * gm.AlignmentWeight;    //1.8

        //obstacle avoidance - weight it
        foreach (GameObject obst in gm.Obstacles)
        {
            seekForce += AvoidObstacle(obst, gm.ObstacleAvoidanceDistance) * gm.ObstacleAvoidanceWeight;
        }

        //limit the steering force before applying
        seekForce = Vector3.ClampMagnitude(seekForce, maxForce);

        ApplyForce(seekForce);  
    }

    //gives the flockers a new path to follow
    //this should only be called once
    public void AssignFlockerPath(Vector3 t)
    {
        SetPath(t + Vector3.one);

        foreach(GameObject f in gm.Flockers)
        {
            f.GetComponent<Vehicle>().SetFlockerPath(this.nodeStack, this.currentNodeTarget, this.target, this.finalDestination);
        }
    }






    /// <summary>
    /// HELPER METHODS
    /// These methods are used in the basic code above to perform specific functions
    /// like modules, so that we can keep our calcSteeringForces method mostly clean
    /// and easy to understand.
    /// </summary>





    //sets flocker path
    //TAKES - flocker information from the 1st flocker
    //nodeStack, currentNode, target, finaldestination
    public void SetFlockerPath(Stack<Transform> ns, Transform cnt, Vector3 tar, Vector3 fd)
    {
        nodeStack = ns;
        currentNodeTarget = cnt;
        target = tar;
        finalDestination = fd;
    }


    //applies a force to this object's velocity (to move character controller - see base class AStarAgent)
    void ApplyForce(Vector3 steeringForce)
    {
        //velocity += steeringForce;
        this.desiredAcceleration += steeringForce;
        
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
        Vector3 steer = desired - velocity;
        steer.y = 0;
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
        return (gm.AverageFlockDirection - velocity);
    }

    //seeks the flocker to the centroid for cohesion
    Vector3 Cohesion()
    {
        return Seek(gm.Centroid);
    }
}
