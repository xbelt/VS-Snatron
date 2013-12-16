using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
	public static readonly int MaxPlayers = 8;
	// The only game instance
	private static Game _instance = NewGame();

	public static Game Instance { get { return _instance; } }
	
	public static Game NewGame()
	{
		if (_instance != null)
			_instance.StopGame ();
		_instance = new Game ();
		return _instance;
	}

	private Transform _playerPrefab;
	private Transform _gridPrefab;

	private int _localPlayerId;
	public int PlayerID { get {return _localPlayerId;} }
	private readonly string[] PlayerId2UserName = new string[MaxPlayers];

	public int NumberOfCubes { get {return 5;} }
	public int FieldBorderCoordinates { get { return 200; } }

	private Game() { }

	// Start the game and give the local player control of the tron with localPlayerId
	public void StartGame(int localPlayerId)
	{
		if (Network.isServer)
		{
			InstantiateGameBorders();
			InstantiateCubes();
		}
		
		_localPlayerId = localPlayerId;
		_playerPrefab = Resources.Load<Transform>("Player" + _localPlayerId);
		_gridPrefab = Resources.Load<Transform>("Lines");

		SpawnPlayer();
	}
	
	public void StopGame()
	{
	    var walls = GameObject.FindGameObjectsWithTag("wall");
	    foreach (var wall in walls)
	    {
	        Destroy(wall);
	    }
	    var lines = GameObject.FindGameObjectsWithTag("line");
	    foreach (var line in lines)
	    {
	        Destroy(line);
	    }
	    var cubes = GameObject.FindGameObjectsWithTag("cube");
	    foreach (var cube in cubes)
	    {
	        Destroy(cube);
	    }
	    var players = GameObject.FindGameObjectsWithTag("tron");
	    foreach (var player in players)
	    {
	        Destroy(player);
	    }
	}

	private void InstantiateGameBorders() {
		var leftWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var frontWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var rightWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var backWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var wallColor = new Color(204 / 255f, 204 / 255f, 204 / 255f, 1f);
		var shader = Shader.Find("Diffuse");
		if (leftWall != null)
		{
			leftWall.transform.localScale = new Vector3(1, 5, 1);
			leftWall.GetComponent<WallBehaviour>().start = new Vector3(-FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates);
			leftWall.GetComponent<WallBehaviour>().updateWall(new Vector3(FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates));
			leftWall.renderer.material.shader = shader;
		}
		if (frontWall != null)
		{
			frontWall.transform.localScale = new Vector3(1, 5, 1);
			frontWall.GetComponent<WallBehaviour>().start = new Vector3(FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates);
			frontWall.GetComponent<WallBehaviour>().updateWall(new Vector3(FieldBorderCoordinates, 1.5f, FieldBorderCoordinates));
			frontWall.renderer.material.shader = shader;
		}
		if (rightWall != null)
		{
			rightWall.transform.localScale = new Vector3(1, 5, 1);
			rightWall.GetComponent<WallBehaviour>().start = new Vector3(FieldBorderCoordinates, 1.5f, FieldBorderCoordinates);
			rightWall.GetComponent<WallBehaviour>().updateWall(new Vector3(-FieldBorderCoordinates, 1.5f, FieldBorderCoordinates));
			rightWall.renderer.material.shader = shader;
		}
		if (backWall != null)
		{
			backWall.transform.localScale = new Vector3(1, 5, 1);
			backWall.GetComponent<WallBehaviour>().start = new Vector3(-FieldBorderCoordinates, 1.5f, FieldBorderCoordinates);
			backWall.GetComponent<WallBehaviour>().updateWall(new Vector3(-FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates));
			backWall.renderer.material.shader = shader;
		}
	}
	
	private void InstantiateCubes() {
		List<Transform> cubes = new List<Transform>();
		System.Random random = new System.Random();
		var shader = Shader.Find("Diffuse");
		for(int i = 0; i < NumberOfCubes; ++i) {
			int _x = random.Next(-(int) FieldBorderCoordinates, (int) FieldBorderCoordinates);
			int _z = random.Next(-(int) FieldBorderCoordinates, (int) FieldBorderCoordinates);
			cubes.Add(Network.Instantiate(Resources.Load<Transform>("Cube"), new Vector3((float)_x, 1.5f, (float)_z), Quaternion.identity, 0) as Transform);
			cubes[i].renderer.material.shader = shader;
		}
	}
	
	private void SpawnPlayer()
	{
		Vector3 location;
		Quaternion orientation;
		mapStartLocation (PlayerID, out location, out orientation);
		
		var player = Network.Instantiate(_playerPrefab, location, orientation, 0) as Transform;
		var cam = GameObject.Find("Main Camera");
		cam.AddComponent<SmoothFollow>().target = player;

		MonoBehaviour.Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
		MonoBehaviour.Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
	}

	public void setPlayer(int playerId, string playerName) {
		PlayerId2UserName[playerId] = playerName;
	}

	public string getPlayerName(int playerId) {
		return PlayerId2UserName [playerId];
	}

	public int getFirstFreePlayerId() {
		int res = -1;
		foreach (string name in PlayerId2UserName) {
			res++;
			if (name == null)
				break;
		}
		return res;
	}
	
	private void mapStartLocation(int playerId, out Vector3 location, out Quaternion orientation)
	{
		// This is how players are arranged in a square, by id
		// 4 0 6
		// 3 * 2
		// 7 1 5
		
		// Orientations:
		// 0,1,2,3 look towards the center * @(0/0/0)
		// 4,5,6,7 look in clockwise direction
		
		float dist = FieldBorderCoordinates/4;
		
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

