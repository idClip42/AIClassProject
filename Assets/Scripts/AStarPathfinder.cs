using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour 
{
	public bool debugLines;

	List<AStarNode> open;
	List<AStarNode> closed;

	AStarNode[] nodes;

	public static AStarPathfinder pathfinder;


	/*
	public AStarNode startNodeManual;
	public AStarNode endNodeManual;
	public bool findPath;
*/


	int remainingNodeCount;




	void Start()
	{
		pathfinder = this;

		nodes = GetComponentsInChildren<AStarNode>();
		for(int n = 0; n < nodes.Length; ++n)
		{
			nodes[n].DebugLinesOn = debugLines;
			nodes[n].GetComponent<MeshRenderer>().enabled = false;
		}
	}



	/*
	void Update()
	{
		if(findPath == true)
		{
			findPath = false;
			GetPath(startNodeManual, endNodeManual);
		}
	}
*/



	public Stack<Transform> GetPath(AStarNode start, AStarNode end)
	{
		open = new List<AStarNode>();
		closed = new List<AStarNode>();
		remainingNodeCount = nodes.Length;
		foreach(AStarNode node in nodes)
			node.Reset();

		start.StartToCurrent = 0;
		if(RecursiveAStarStep(start, end) == true)
		{
			Debug.Log("Got a path");
			return ReturnPath(start, end);
		}
		else
		{
			Debug.Log("Didn't got a path");
			return null;
		}
	}

	bool RecursiveAStarStep(AStarNode currentNode, AStarNode endNode)
	{
		InfiniteLoopStopper();

		closed.Add(currentNode);
		AddAdjacentsToOpen(currentNode, endNode);
		AStarNode nextNode = GetShortestNode();
		if(nextNode == null)
			return false;	// No path found
		if(nextNode == endNode)
			return true; // Path found
		open.Remove(nextNode);

		return RecursiveAStarStep(nextNode, endNode);
	}

	void AddAdjacentsToOpen(AStarNode node, AStarNode end)
	{
		AStarNode[] nodes = node.adjacentNodes;
		for(int n = 0; n < nodes.Length; ++n)
		{
			if(!closed.Contains(nodes[n]))
			{
				float costSoFar = node.StartToCurrent;
				float hereToThere = nodes[n].GetDistBetween(node);
				float thereToEnd = nodes[n].GetDistBetween(end);
			
				if(!open.Contains(nodes[n]) || nodes[n].TotalCost > (hereToThere + costSoFar + thereToEnd)) 
				{
					nodes[n].StartToCurrent = hereToThere + costSoFar;
					nodes[n].CurrentToEnd = thereToEnd;
					nodes[n].PreviousNode = node;

					if(!open.Contains(nodes[n])) 
						open.Add(nodes[n]);
				}

			}
		}
	}

	AStarNode GetShortestNode()
	{
		if(open.Count == 0) return null;
		AStarNode current = open[0];
		for(int n = 1; n < open.Count; ++n)
			if(open[n].TotalCost < current.TotalCost)
				current = open[n];
		return current;
	}


	bool InfiniteLoopStopper()
	{
		--remainingNodeCount;
		if(remainingNodeCount < 0)
		{
			Debug.Log("Force ending infinite recursive loop.");
			return true;
		}
		return false;
	}


	Stack<Transform> ReturnPath(AStarNode start, AStarNode end)
	{
		Stack<Transform> nodeStack = new Stack<Transform>();
		AStarNode currentNode = end;

		while(currentNode != null)
		{
			nodeStack.Push(currentNode.transform);
			currentNode = currentNode.PreviousNode;
		}

		return nodeStack;
	}


}
