using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour 
{

	public float speed = 10;			// The speed at which the player walks,
	public float acceleration = 50;		// The rate of the player's acceleration
	private CharacterController controller;       // The player's character controller
    private Vector3 velocity;                  // The velocity the Character Controller will move at every frame
    private Vector3 vertVelocity;              // The vertical velocity, which won't be limited to one speed
    private Animator anim;                        // The character model of the current player character

    private Stack<Transform> nodeStack;           // Stack of path nodes to follow
    private Transform currentNodeTarget;      // The node currently targeted
    private Vector3 target;                       // Where the character is currently moving
    private Vector3 finalDestination;			// The input location that agent is ultimately approaching








	/// <summary>
	/// Sets the A* path from the current location to the given destination
	/// </summary>
	/// <param name="destination">Where the agent will go</param>
	protected void SetPath(Vector3 destination)
	{
		// Starts by finding the closest path node
		// to the current position
		// and to the destination
		AStarNode[] nodes = AStarPathfinder.pathfinder.GetComponentsInChildren<AStarNode>();
		if(nodes.Length == 0) return;
		AStarNode closestStartNode = nodes[0];
		float closestStartDistance = float.MaxValue;
		AStarNode closestEndNode = nodes[0];
		float closestEndDistance = float.MaxValue;
		for(int n = 0; n < nodes.Length; ++n)
		{
			float x = Mathf.Abs(nodes[n].transform.position.x - transform.position.x);
			float y = Mathf.Abs(nodes[n].transform.position.y - transform.position.y);
			float z = Mathf.Abs(nodes[n].transform.position.z - transform.position.z);
			float manhDist = x + y + z;

			if(manhDist < closestStartDistance)
			{
				closestStartNode = nodes[n];
				closestStartDistance = manhDist;
			}

			x = Mathf.Abs(nodes[n].transform.position.x - destination.x);
			y = Mathf.Abs(nodes[n].transform.position.y - destination.y);
			z = Mathf.Abs(nodes[n].transform.position.z - destination.z);
			manhDist = x + y + z;

			if(manhDist < closestEndDistance)
			{
				closestEndNode = nodes[n];
				closestEndDistance = manhDist;
			}
		}


		// Gets and returns the path
		nodeStack = AStarPathfinder.pathfinder.GetPath(closestStartNode, closestEndNode);


		if(nodeStack == null)
		{
			if(closestStartNode == closestEndNode)
				target = destination;
			return;
		}
		currentNodeTarget = nodeStack.Pop();
		target = currentNodeTarget.transform.position;
		finalDestination = destination;
	}

	/// <summary>
	/// Called each FixedUpdate, this checks if the agent has reached the target node
	/// and if so, gets the next target
	/// </summary>
	protected virtual void PathfindingUpdate()
	{
		// Upon user input, will target and follow player
		// for testing purposes
		// Because this is via fixedUpdate, pressing E will be finicky
		if(Input.GetKeyDown(KeyCode.E))
		{
			SetPath(GameObject.FindGameObjectWithTag("Player").transform.position);
		}




		// If there is no nodeStack, do nothing
		if(nodeStack == null) return;

		// Gets the squared distance between agent and node and the squared node radius
		// This is to avoid square roots
		float distSqr = Vector3.SqrMagnitude(currentNodeTarget.position - transform.position);
		float radiusSqr = Mathf.Pow(currentNodeTarget.localScale.x/2, 2);

		// If the agent is within range of the node, gets a new target node
		if(distSqr < radiusSqr)
		{
			if(nodeStack.Count > 0)
			{
				currentNodeTarget = nodeStack.Pop();
				target = currentNodeTarget.transform.position;
			} else {
				currentNodeTarget = null;
				nodeStack = null;
				target = finalDestination;
			}
		}
	}

    //makes ONLY the A* agent move to player
	void OnGUI()
	{
		GUI.Box(new Rect(0,0,200,50), "");
		if(GUI.Button(new Rect(40,10,120,30), "A* Go To Player"))
		{
			GameObject.Find("AStarAgent").GetComponent<AStarAgent>().SetPath(GameObject.FindGameObjectWithTag("Player").transform.position);
		}
	}
































	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		// Gets animator
		controller = GetComponent<CharacterController>();
		if(controller == null) Debug.LogError("Player needs a Character Controller");

		// Initializes velocity
		velocity = Vector3.zero;
		vertVelocity = Vector3.zero;

		// Gets animator
		anim = GetComponentInChildren<Animator>();

		// Initializes target position as current position.
		target = transform.position;
	}

	/// <summary>
	/// Updates at a fixed rate based on Physics
	/// </summary>
	void FixedUpdate () 
	{
		MovePlayer();     
		Animate();
		PathfindingUpdate();
	}

	/// <summary>
	/// Moves the player.
	/// </summary>
	protected virtual void MovePlayer()
	{

        // Adds input movement to the velocity
        velocity += Vector3.ClampMagnitude(
            Vector3.ProjectOnPlane(target - transform.position, Vector3.up) * acceleration,
            acceleration) * Time.fixedDeltaTime;

        // Limits velocity to set speed
        velocity = Vector3.ClampMagnitude(velocity, speed);

		// Drag
		velocity -= velocity * 0.1f;

		vertVelocity += Physics.gravity * Time.fixedDeltaTime;
		if(vertVelocity.y < Physics.gravity.y) vertVelocity = Physics.gravity;

		// Moves the player with the new velocity
		controller.Move((velocity + vertVelocity) * Time.fixedDeltaTime);

	}

	/// <summary>
	/// Animate the player model.
	/// </summary>
	protected void Animate()
	{
		if(anim == null) 
			return;

		// Turns the player model in the correct direction
		anim.transform.forward = Vector3.Lerp(
			anim.transform.forward,
			velocity,
			0.1f
		);

		// Gives speed to animator
		anim.SetFloat("Speed", velocity.magnitude);
	}
}
