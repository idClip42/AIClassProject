using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : MonoBehaviour 
{
	/// <summary>
	/// The nodes adjacent to this one
	/// </summary>
	public AStarNode[] adjacentNodes;


	float startToCurrent = float.MaxValue;
	/// <summary>
	/// The cost from the start node to this one, along a path
	/// </summary>
	/// <value>The cost from the start node to this one</value>
	public float StartToCurrent { 
		get { return startToCurrent; }
		set { startToCurrent = value; }
	}


	float currentToEnd = float.MaxValue;
	/// <summary>
	/// The cost from this node to the end node, via Manhattan Distance
	/// </summary>
	/// <value>The cost from this node to the end node</value>
	public float CurrentToEnd { 
		get { return currentToEnd; }
		set { currentToEnd = value; }
	}


	/// <summary>
	/// The sum of startToCurrent and currentToEnd
	/// (unless one or both is unchanged from MaxValue)
	/// </summary>
	/// <value>The total cost.</value>
	public float TotalCost
	{ 
		get 
		{
			if(startToCurrent == float.MaxValue || currentToEnd == float.MaxValue)
				return float.MaxValue;
			return startToCurrent + currentToEnd;
		}
	}

	AStarNode previousNode = null;
	/// <summary>
	/// The previous node to this one in the path
	/// </summary>
	/// <value>The previous node.</value>
	public AStarNode PreviousNode { 
		get { return previousNode; } 
		set { previousNode = value; } 
	}


	/// <summary>
	/// Gets the Manhattan Distance between this and another node
	/// </summary>
	/// <returns>The Manhattan Distance between the two nodes.</returns>
	/// <param name="node">The other node</param>
	public float GetDistBetween(AStarNode node)
	{
		float x = Mathf.Abs(node.transform.position.x - transform.position.x);
		float y = Mathf.Abs(node.transform.position.y - transform.position.y);
		float z = Mathf.Abs(node.transform.position.z - transform.position.z);
		return x + y + z;
	}




	bool debugLinesOn = false;
	/// <summary>
	/// Whether debug lines will show node connections and paths
	/// </summary>
	/// <value><c>true</c> if debug lines on; otherwise, <c>false</c>.</value>
	public bool DebugLinesOn { set { debugLinesOn = value; } }

	void Update()
	{
		if(debugLinesOn == true)				// If the debug lines are on
		{
			for(int n = 0; n < adjacentNodes.Length; ++n)
				Debug.DrawLine(					// Purple lines are drawn between all nodes
					transform.position, 
					adjacentNodes[n].transform.position, 
					Color.magenta);
			if(previousNode != null)			// Green lines are drawn to each node's previous node
				Debug.DrawLine(
					transform.position + Vector3.one, 
					previousNode.transform.position + Vector3.one + Vector3.up * 2, 
					Color.green);
		}
	}




	void Start()
	{

		// Ensures that all node connections go both ways
		// by adding node to adjacent lists of adjacent nodes

		for(int n = 0; n < adjacentNodes.Length; ++n)
		{
			bool containsThis = false;

			for(int m = 0; m < adjacentNodes[n].adjacentNodes.Length; ++m)
				if(adjacentNodes[n].adjacentNodes[m] == this)
					containsThis = true;

			if(containsThis == false)
			{
				AStarNode[] newAdj = new AStarNode[adjacentNodes[n].adjacentNodes.Length + 1];
				for(int m = 0; m < adjacentNodes[n].adjacentNodes.Length; ++m)
					newAdj[m] = adjacentNodes[n].adjacentNodes[m];
				newAdj[newAdj.Length-1] = this;
				adjacentNodes[n].adjacentNodes = newAdj;
			}
		}




		// Moves the pathnodes down to ground level

		Terrain terrain = Terrain.activeTerrain;
		Vector3 position = transform.position;
		position.y = terrain.SampleHeight(position);
		transform.position = position;


	}

	/// <summary>
	/// Resets the distance values and previous node
	/// </summary>
	public void Reset()
	{
		startToCurrent = float.MaxValue;
		currentToEnd = float.MaxValue;
		previousNode = null;
	}
}