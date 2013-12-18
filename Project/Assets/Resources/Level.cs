using UnityEngine;
using System.Collections;

public interface Level
{
	int MaxPlayers { get; }
	int MaxAIPlayers { get; }
	int NumberOfCubes { get; }
	int FieldBorderCoordinates{ get; }
	void MapStartLocation(int playerId, out Vector3 location, out Quaternion orientation);
}

