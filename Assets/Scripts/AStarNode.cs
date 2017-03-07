using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : MonoBehaviour 
{
	public AStarNode[] adjacentNodes;

	float startToCurrent = float.MaxValue;
	public float StartToCurrent { get { return startToCurrent; } set { startToCurrent = value; } }

	float currentToEnd = float.MaxValue;
	public float CurrentToEnd { get { return currentToEnd; } set { currentToEnd = value; } }

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
	public AStarNode PreviousNode { get { return previousNode; } set { previousNode = value; } }

	public float GetDistBetween(AStarNode node)
	{
		// Uses manhattan distance for speed
		float x = Mathf.Abs(node.transform.position.x - transform.position.x);
		float y = Mathf.Abs(node.transform.position.y - transform.position.y);
		float z = Mathf.Abs(node.transform.position.z - transform.position.z);
		return x + y + z;
	}




	bool debugLinesOn = false;
	public bool DebugLinesOn { set { debugLinesOn = value; } }

	void Update()
	{
		if(debugLinesOn == true)
		{
			for(int n = 0; n < adjacentNodes.Length; ++n)
				Debug.DrawLine(
					transform.position, 
					adjacentNodes[n].transform.position, 
					Color.magenta);
			if(previousNode != null)
				Debug.DrawLine(
					transform.position + Vector3.one, 
					previousNode.transform.position + Vector3.one + Vector3.up * 2, 
					Color.green);
		}
	}




	void Start()
	{

		// Ensures that all node connections go both ways

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


	public void Reset()
	{
		startToCurrent = float.MaxValue;
		currentToEnd = float.MaxValue;
		previousNode = null;
	}
}