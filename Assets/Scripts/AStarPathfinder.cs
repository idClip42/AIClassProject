using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour 
{
	public static AStarPathfinder pathfinder;	// The public pathfinder, easily accessible

	public bool debugLines;						// Whether the scene will visualize the nodes and paths

	AStarNode[] nodes;							// The list of all nodes
	List<AStarNode> open;						// The list of open nodes (for A*)
	List<AStarNode> closed;						// The list of closed nodes (for A*)

	int remainingNodeCount;						// How many nodes have not been put in the closed list
												// Used to ensure that the algorithm does not loop infinitely
												// if there are problems in the code

	void Awake()
	{
		pathfinder = this;						// Initializes this pathfinder as public static object

												// Gets all the nodes in the scene
		nodes = GetComponentsInChildren<AStarNode>();
		for(int n = 0; n < nodes.Length; ++n)
		{
			nodes[n].DebugLinesOn = debugLines;	// Turns on debug lines for each node (or not)
			nodes[n].GetComponent<MeshRenderer>().enabled = false;
												// Turns off the mesh renderer so world is not filled with white spheres
		}
	}


	/// <summary>
	/// Gets a path from one node to another
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="start">The starting node</param>
	/// <param name="end">The ending node</param>
	public Stack<Transform> GetPath(AStarNode start, AStarNode end)
	{
		Reset();								// Resets all the nodes and lists

		start.StartToCurrent = 0;				// Sets the start node's cost so far to 0
									
												// Starts the A* process
												// If it returns true, a path has been found
												// and is then returns
		if(RecursiveAStarStep(start, end) == true)
		{
			Debug.Log("Found a path");
			return ReturnPath(start, end);
		}
		else
		{
			Debug.Log("Didn't find a path");	// Otherwise, nothing is returned
			return null;
		}
	}

	/// <summary>
	/// A recursive method that is a step in the A* algorithm
	/// </summary>
	/// <returns><c>true</c>, if a path is found, <c>false</c> otherwise.</returns>
	/// <param name="currentNode">The current path node to be examined</param>
	/// <param name="endNode">The end goal</param>
	bool RecursiveAStarStep(AStarNode currentNode, AStarNode endNode)
	{
		InfiniteLoopStopper();					// Prevents any recursive screw-ups, just in case

		closed.Add(currentNode);				// Adds the current node to the closed list
												// It will not be examined again
												// Valid adjacent nodes are added to the open list
		AddAdjacentsToOpen(currentNode, endNode);
		AStarNode nextNode = GetShortestNode();	// The next currentNode is the node with the
												// lowest cost to the end in the open list
		if(nextNode == endNode)					// If the next node is the end, we've found our path
			return true;
		if(nextNode == null)					// If there is no next node, no path has been found
			return false;
		open.Remove(nextNode);					// The next node is removed from the open list
												// (to be added to the closed list)

		return RecursiveAStarStep(nextNode, endNode);
												// This method is called again for the next node
	}

	/// <summary>
	/// Adds adjacent nodes of the input node to the open list
	/// Also gives them cost info
	/// </summary>
	/// <param name="node">The current node to check the adjacents of</param>
	/// <param name="end">The end node</param>
	void AddAdjacentsToOpen(AStarNode node, AStarNode end)
	{
		AStarNode[] nodes = node.adjacentNodes;	// Gets the adjacent nodes
		for(int n = 0; n < nodes.Length; ++n)
		{
			if(!closed.Contains(nodes[n]))		// If the adjacent nodes aren't in the closed list
			{
				float costSoFar = node.StartToCurrent;
				float hereToThere = nodes[n].GetDistBetween(node);
				float thereToEnd = nodes[n].GetDistBetween(end);
												// We find their costs relative to the current node
			
				if(!open.Contains(nodes[n]) || nodes[n].TotalCost > (hereToThere + costSoFar + thereToEnd)) 
				{
					nodes[n].StartToCurrent = hereToThere + costSoFar;
					nodes[n].CurrentToEnd = thereToEnd;
					nodes[n].PreviousNode = node;
												// If the total cost from the current node is less than
												// what the node currently has,
												// it is given the new cost values

					if(!open.Contains(nodes[n])) 
						open.Add(nodes[n]);		// If it is not in the open list, it is added
				}

			}
		}
	}

	/// <summary>
	/// Gets the node in the open list with the lowest cost
	/// </summary>
	/// <returns>The node with the lowest cost, or null if list is empty</returns>
	AStarNode GetShortestNode()
	{
		if(open.Count == 0) return null;
		AStarNode current = open[0];
		for(int n = 1; n < open.Count; ++n)
			if(open[n].TotalCost < current.TotalCost)
				current = open[n];
		return current;
	}

	/// <summary>
	/// Ensures that the recursive method doesn't keep going forever
	/// </summary>
	/// <returns><c>true</c>, loop must be stopped, <c>false</c> otherwise.</returns>
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

	/// <summary>
	/// Returns an ordered stack of transforms that holds the path of nodes
	/// </summary>
	/// <returns>The path stack</returns>
	/// <param name="start">Start node</param>
	/// <param name="end">End node</param>
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


	/// <summary>
	/// Resets the nodes and the lists for a new path
	/// </summary>
	void Reset()
	{
		open = new List<AStarNode>();
		closed = new List<AStarNode>();
		remainingNodeCount = nodes.Length;
		foreach(AStarNode node in nodes)
			node.Reset();
	}


}
