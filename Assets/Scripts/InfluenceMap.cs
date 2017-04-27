using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceMap : MonoBehaviour 
{
	public GameObject influenceCube;		// The gameobjects that will make up the influence map
	public Vector2 dimensions;				// The dimensions of the influence map

	Terrain terrain;						// The terrain the map is on
	Vector3 terrainSize;					// The dimensions of that terrain
	Vector3 terrainOffset;					// The position of the terrain

	Vector2 blockSize;						// The size of a given block, based on terrain size and map dimensions
	int [,] weights;						// An array of weights for each block of the influence map





	void Start () 
	{
		terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>();
		terrainSize = terrain.terrainData.size;
		terrainOffset = terrain.transform.position;
	}






	/// <summary>
	/// Makes a new influence map based on Influence Units in the scene
	/// </summary>
	public void MakeInfluenceMap()
	{
		// Ensures that dimensions are valid
		if(dimensions.x <= 0) dimensions.x = 1;
		if(dimensions.y <= 0) dimensions.y = 1;

		// Finds all Influence Units on map
		GameObject[] guys = GameObject.FindGameObjectsWithTag("Unit");

		// Determines the size of the blocks of the influence map
		blockSize = new Vector2(terrainSize.x/dimensions.x, terrainSize.z/dimensions.y);

		// Removes any map that may currently be present
		KillMap();

		// Gets a new, empty weight map to add influence to
		NewEmptyWeightMap();

		// Adds influence to the map from all Influence Units
		for(int g = 0; g < guys.Length; ++g)
			AddUnitInfluence(guys[g]);

		// Instantiates blocks to make influence map
		for(int y = 0; y < dimensions.y; ++y)
			for(int x = 0; x < dimensions.x; ++x)
				AddBlockToMap(x, y);
	}






	/// <summary>
	/// Adds the influence of a given Influence Unit to the weight map array.
	/// </summary>
	/// <param name="guy">Influence Unit to use with weight map array</param>
	void AddUnitInfluence(GameObject guy)
	{
		// Gets Influence Unit script and its strength value
		InfluenceUnit script = guy.GetComponent<InfluenceUnit>();
		int strength = script.Strength;

		// This will be used to determine which way the Unit's weight goes
		int whichSide = 1;
		if(script.Team == 2) whichSide = -1;

		// Finds which block of the influence map the given unit is in
		Vector2 pos = new Vector2(
			Mathf.Floor((guy.transform.position.x - terrainOffset.x)/blockSize.x),
			Mathf.Floor((guy.transform.position.z - terrainOffset.z)/blockSize.y)
		);

		// Sets an xPos and yPos as ints, to avoid messy code in next block
		int xPos = (int)pos.x;
		int yPos = (int)pos.y;

		// Goes through all blocks within a radius based on the unit's strength
		for(int y = -strength + 1 + yPos; y < strength + yPos; ++y)
			for(int x = -strength + 1 + xPos; x < strength + xPos; ++x)
				// Ensures that this block is a valid block
				if(x >= 0 && 
					x < weights.GetLength(0) && 
					y >= 0 && 
					y < weights.GetLength(1))
				{
					// Adds a weight value to the block, based on manhattan distance from the position
					// Weights are added rather than set
					// so that the influence is determined by all adjacent units from either team
					// One team adds positive influence, the other negative (hence the "whichSide" multiplier)
					int dist = Mathf.Abs(x - xPos) + Mathf.Abs(y - yPos);
					weights[x,y] += Mathf.Clamp(strength - dist, 0, 4) * whichSide;
				}
	}







	/// <summary>
	/// Adds a block to map based on the weight map array
	/// </summary>
	/// <param name="x">The x coordinate of the block to be added</param>
	/// <param name="y">The y coordinate of the block to be added</param>
	void AddBlockToMap(int x, int y)
	{
		// Instantiates a block object
		// Positions it in the correct 2D location on the map
		// Rotates it because the "block" is a Quad primitive and must be rotated to face upwards
		GameObject block = (GameObject) Instantiate(
			influenceCube,
			terrain.transform.position + new Vector3(blockSize.x * (x+0.5f), 100, blockSize.y * (y+0.5f)),
			Quaternion.Euler(90, 0, 0)
		);

		// Scales the block object to fit neatly into the influence map
		block.transform.localScale = Vector3.Scale(
			block.transform.localScale, 
			new Vector3(blockSize.x, blockSize.y, blockSize.y));

		// Positions the block object directly on the terrain,
		// so that visually it matches up with the terrain
		Vector3 pos = block.transform.position;
		pos.y = terrain.SampleHeight(pos) + 0.5f;
		block.transform.position = pos;

		// Changes the material's color
		// Green if Team 1 has more (positive) influence
		// Red if Team2 has more (negative) influence
		// Alpha at 0.5f so that we can still see units and terrain
		Material m = block.GetComponent<MeshRenderer>().material;
		float perc = weights[x,y]/4.0f;
		m.color = new Color(-perc, perc, 0, 0.5f);
	}








	/// <summary>
	/// Creates a new empty weight map
	/// </summary>
	void NewEmptyWeightMap()
	{
		weights = new int[(int)dimensions.x, (int)dimensions.y];

		for(int y = 0; y < weights.GetLength(1); ++y)
			for(int x = 0; x < weights.GetLength(0); ++x)
				weights[x,y] = 0;
	}





	/// <summary>
	/// Removes the current influence map from the scene
	/// </summary>
	public void KillMap()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag("Obstacle");
		for(int n = 0; n < objs.Length; ++n)
			Destroy(objs[n]);
	}
}
