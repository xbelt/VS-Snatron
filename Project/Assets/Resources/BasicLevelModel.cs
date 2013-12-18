using UnityEngine;
using System.Collections;

public class BasicLevelModel : Level
{
	public int MaxPlayers { get { return 8; } }
	public int MaxAIPlayers { get { return 7; } }
	public int NumberOfCubes { get {return 5;} }
	public int FieldBorderCoordinates { get { return 200; } }

	public void MapStartLocation(int playerId, out Vector3 location, out Quaternion orientation)
	{
		// This is how players are arranged in a square, by id
		// 4 0 6
		// 3 * 2
		// 7 1 5
		
		// Orientations:
		// 0,1,2,3 look towards the center * @(0/0/0)
		// 4,5,6,7 look in clockwise direction
		
		float dist = FieldBorderCoordinates/2;
		
		switch (playerId) {
		case 0: location = new Vector3(0, 0, -dist); 	 orientation = Quaternion.AngleAxis(0, Vector3.up); break;
		case 1: location = new Vector3(0, 0,  dist); 	 orientation = Quaternion.AngleAxis(180, Vector3.up); break;
		case 2:	location = new Vector3( dist, 0, 0); 	 orientation = Quaternion.AngleAxis(270, Vector3.up); break;
		case 3:	location = new Vector3(-dist, 0, 0); 	 orientation = Quaternion.AngleAxis(90, Vector3.up); break;
		case 4:	location = new Vector3(-dist, 0, -dist); orientation = Quaternion.AngleAxis(90, Vector3.up); break;
		case 5:	location = new Vector3( dist, 0,  dist); orientation = Quaternion.AngleAxis(270, Vector3.up); break;
		case 6:	location = new Vector3( dist, 0, -dist); orientation = Quaternion.AngleAxis(0, Vector3.up); break;
		case 7:	location = new Vector3(-dist, 0,  dist); orientation = Quaternion.AngleAxis(180, Vector3.up); break;
		default:location = new Vector3(); 				 orientation = new Quaternion(); break;
		}
	}
}

