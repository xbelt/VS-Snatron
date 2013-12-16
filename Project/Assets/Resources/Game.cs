using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
	public static readonly int MaxPlayers = 8;
	// The only game instance
	private static Game _instance = new Game();
	public static Game Instance { get { return _instance; } }

	public class PlayerModel
	{
		public readonly int id;
		public readonly string name;
		public int rank; // for one round
		public int score; // for a series of rounds
		public bool isAlive;

		public PlayerModel(int id, string name)
		{
			this.id = id;
			this.name = name;
			startGame ();
		}

		public void startGame()
		{
			isAlive = true;
			score = 0;
			rank = 0;
		}

		public void startRound()
		{
			isAlive = true;
			rank = 1;
		}
	}

	private Transform _playerPrefab;
	private Transform _gridPrefab;

	private int _localPlayerId;
	public int PlayerID { get {return _localPlayerId;} }

	private Drive _localPlayer;
	public Drive LocalPlayer { get {return _localPlayer;} }

	public PlayerModel LocalPlayerModel { get { return _players [PlayerID]; } }
	private PlayerModel[] _players = new PlayerModel[MaxPlayers];

	//private readonly string[] userNames = new string[MaxPlayers];
	//private readonly bool[] aliveStates = new bool[MaxPlayers];

	private int _nOfLivingPlayers;
	private int _nOfActivePlayers;

	// 0 if player id is not active
	// Ranks 1 - MaxPlayer for active players
	//private readonly int[] _ranks = new int[MaxPlayers];
	//public int[] Ranks { get {return _ranks;} }

	//private readonly int[] _scores = new int[MaxPlayers];
	//public int[] Scores { get {return _ranks;} }

	// TODO put into game config
	private static readonly int GAME_LENGTH = 5;
	private int _roundsToPlay;
	public int RoundsToPlay { get { return _roundsToPlay; } }

	public int NumberOfCubes { get {return 5;} }
	public int FieldBorderCoordinates { get { return 200; } }
    public double IndestructibleTimeLeft { get; set; }


    private Game() { 
		NewGame ();
	}
	
	// Reset the game instance
	public void NewGame()
	{
		_nOfActivePlayers = 0;
		_nOfLivingPlayers = 0;
		_roundsToPlay = GAME_LENGTH;

		for (int i = 0; i < MaxPlayers; i++)
		{
			_players[i] = null;
		}
	}

	public void NewRound()
	{
		_roundsToPlay--;
		_nOfLivingPlayers = _nOfActivePlayers;
		for (int i = 0; i < MaxPlayers; i++)
		{
			if (_players[i] == null)
				continue;
			_players[i].score += _nOfActivePlayers - _players[i].rank;
			_players[i].startRound();
		}
	}

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

		NewGame ();
	}

	public delegate void LocalPlayerSpawn(Drive player);
	public LocalPlayerSpawn OnLocalPlayerSpawn;
	
	private void SpawnPlayer()
	{
		Vector3 location;
		Quaternion orientation;
		mapStartLocation (PlayerID, out location, out orientation);
		
		var player = Network.Instantiate(_playerPrefab, location, orientation, 0) as Transform;
		var cam = GameObject.Find("Main Camera");
		cam.AddComponent<SmoothFollow>().target = player;

		Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
		Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
	
		_localPlayer = player.GetComponent<Drive> ();
		if (OnLocalPlayerSpawn != null)
			OnLocalPlayerSpawn (_localPlayer);
	}

	public bool isActivePlayer(int playerId)
	{
		return _players [playerId] != null;
	}

	public int countActivePlayers()
	{
		return _nOfActivePlayers;
	}

	// Note: indices do not correspond to playerId
	public int countAlivePlayers()
	{
		return _nOfLivingPlayers;
	}

	public bool isAlive(int playerId)
	{
		if (!isActivePlayer(playerId))
			return false;
		return _players[playerId].isAlive;
	}
	
	public void playerDied(int playerId)
	{
		PlayerModel player = _players [playerId];
		if (player == null)
			return;
		player.rank = countAlivePlayers ();
		player.isAlive = false;
		_nOfLivingPlayers--;
		
		if (isRoundOver ()) {
			EndRound();
		}
	}

	private void EndRound()
	{
		if (isGameOver ()) {
			EndGame ();
		}
	}

	private void EndGame()
	{
		StopGame ();
	}

	// TODO not really necessary?
	public bool hasWon(int playerId)
	{
		return isAlive (playerId) && countAlivePlayers () == 1; 
	}

	public bool isRoundOver()
	{
		if (_nOfActivePlayers > 1)
			return _nOfLivingPlayers <= 1;
		else
			return isAlive (PlayerID);
	}

	public bool isGameOver()
	{
		return isRoundOver () && RoundsToPlay;
	}

	// Called when both local player and remote player was spawned? TODO really?
	public void setPlayer(int playerId, string playerName) {
		_players [playerId] = new PlayerModel (playerId, playerName);
		_nOfActivePlayers++;
		_nOfLivingPlayers++; // TODO probably not necessary
	}

	public string getPlayerName(int playerId) {
		PlayerModel player = _players [playerId];
		if (player == null)
			return null;
		return player.name;
	}

	public int getFirstFreePlayerId() {
		int res = -1;
		foreach (PlayerModel player in _players) {
			res++;
			if (player == null)
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
		var cubes = new List<Transform>();
		var random = new System.Random();
		var shader = Shader.Find("Diffuse");
		for(var i = 0; i < NumberOfCubes; ++i) {
			var x = random.Next(-FieldBorderCoordinates, FieldBorderCoordinates);
			var z = random.Next(-FieldBorderCoordinates, FieldBorderCoordinates);
			cubes.Add(Network.Instantiate(Resources.Load<Transform>("Cube"), new Vector3(x, 1.5f, z), Quaternion.identity, 0) as Transform);
			cubes[i].renderer.material.shader = shader;
		}
	}
}

