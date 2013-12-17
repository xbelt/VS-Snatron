using Assets.Resources;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game
{
	public static readonly int MaxPlayers = 8;
	// TODO put into game config
	private static readonly int GAME_LENGTH = 5;

	// The only game instance
	private static Game _instance = new Game();
	public static Game Instance { get { return _instance; } }

	private Transform _playerPrefab;
	private Transform _gridPrefab;

	private int _localPlayerId;
	public int PlayerID { get {return _localPlayerId;} }

	private bool _gameStarted;

	private Drive _localPlayer;
	public Drive LocalPlayer { get {return _localPlayer;} }

	//public PlayerModel LocalPlayerModel { get { return _players [PlayerID]; } }
	private PlayerModel[] _players = new PlayerModel[MaxPlayers];

	private int _nOfActivePlayers;
	public int NofActivePlayers { get {	return _nOfActivePlayers; }	}
	
	private int _nOfLivingPlayers;
	public int NofLivingPlayers { get { return _nOfLivingPlayers; } }
    public int numberOfKIPlayers;

	private int _roundsToPlay;
	public int RoundsToPlay { get { return _roundsToPlay; } }

	public int NumberOfCubes { get {return 5;} }
	public int FieldBorderCoordinates { get { return 200; } }
    public double IndestructibleTimeLeft { get; set; } // TODO move to somewhere else

    private Game() { 
		NewGame ();
	}

	// Start the game and give the local player control of the tron with localPlayerId
	// rounds tells how many rounds one game last
	public void StartGame(int localPlayerId, int rounds)
	{
		_localPlayerId = localPlayerId;
		_playerPrefab = Resources.Load<Transform>("Player" + _localPlayerId);
		_gridPrefab = Resources.Load<Transform>("Lines");
		_roundsToPlay = rounds;
		SpawnPlayer();
		if (Network.isServer)
		{
			InstantiateGameBorders();
			InstantiateCubes();
		    SpawnKI();
		}
		
		
		_gameStarted = true;
	}


    public void StopGame()
	{
		clearGameObjects ();
		NewGame ();
	}

	#region game initialization

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
		
		MonoBehaviour.Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
        MonoBehaviour.Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
		
		_localPlayer = player.GetComponent<Drive> ();
		if (OnLocalPlayerSpawn != null)
			OnLocalPlayerSpawn (_localPlayer);
	}
	
    private void SpawnKI() {
        for (int i = PlayerID + 1; i < PlayerID + 1 + numberOfKIPlayers; i++) {
            _playerPrefab = Resources.Load<Transform>("Player" + i);
            Vector3 location;
            Quaternion orientation;
            mapStartLocation(i, out location, out orientation);

            var player = Network.Instantiate(_playerPrefab, location, orientation, 0) as Transform;
            MonoBehaviour.Destroy(player.gameObject.GetComponent<Drive>());
            player.gameObject.AddComponent<KIControler>();
            player.gameObject.GetComponent<KIControler>().KIId = i;
        }
    }
	// Called when both local player and remote player was spawned? TODO really?
	public void setPlayer(int playerId, string playerName) {
        MonoBehaviour.print("Game:SetPlayer()");
		_players [playerId] = new PlayerModel (playerId, playerName);
		_nOfActivePlayers++;
		_nOfLivingPlayers++; // TODO probably not necessary
	}

	public void removePlayer(int playerId) {
        MonoBehaviour.print("Game:removePlayer()");
		if (_players [playerId].isAlive)
			_nOfLivingPlayers--;
		_nOfActivePlayers--;
		_players [playerId] = null;
	}

	// Called by server to assign ids to joining players
	public int getFirstFreePlayerId() {
		int res = -1;
		foreach (PlayerModel player in _players) {
			res++;
			if (player == null)
				break;
		}
		return res;
	}

	#endregion

	#region game and round logic

	// Reset the game instance
	public void NewGame()
	{
		_nOfActivePlayers = 0;
		_nOfLivingPlayers = 0;
		_roundsToPlay = GAME_LENGTH;
		_gameStarted = false;
		
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

	public void playerDied(int playerId)
	{
		PlayerModel player = _players [playerId];
		if (player == null)
			return;
		player.rank = _nOfLivingPlayers;
		player.isAlive = false;
		_nOfLivingPlayers--;
		
		if (isRoundOver ()) {
			EndRound();
		}
	}

	private void EndRound()
	{
		// TODO something else?
		clearGameObjects ();

		if (isGameOver ()) {
			EndGame ();
		}
	}
	
	private void EndGame()
	{
		StopGame ();
	}

	public bool isRoundOver()
	{
		if (!_gameStarted)
			return false;
		if (_nOfActivePlayers > 1)
			return _nOfLivingPlayers <= 1;
		else
			return isAlive (PlayerID);
	}
	
	public bool isGameOver()
	{
		if (!_gameStarted)
			return false;
		return isRoundOver () && RoundsToPlay == 0;
	}
	
	// Does the player have the highest score?
	// Condition: for this to become true, the game must be over
	// => nobody wins until the end of the game
	public bool hasWon(int playerId)
	{
		if (!isGameOver ())
			return false;

		int score = _players [playerId].score;
		for (int i = 0; i < MaxPlayers; i++) {
			if (_players[i] == null)
				continue;
			if (_players[i].score > score)
				return false;
		}
		return true;
	}
	
	// Does he have the highest score?
	public bool HasLocalPlayerWon()
	{
		return hasWon (PlayerID);
	}

	#endregion

	#region player accessors

	public bool isActivePlayer(int playerId)
	{
		return _players [playerId] != null;
	}

	public string getPlayerName(int playerId) {
		PlayerModel player = _players [playerId];
		if (player == null)
			return null;
		return player.name;
	}

	public bool isAlive(int playerId)
	{
		if (!isActivePlayer(playerId))
			return false;
		return _players[playerId].isAlive;
	}

	public int Rank(int playerId)
	{
		if (!isActivePlayer(playerId))
			return 0;
		return _players[playerId].rank;
	}

	public int Score(int playerId)
	{
		if (!isActivePlayer(playerId))
			return 0;
		return _players[playerId].score;
	}

	#endregion

	private void clearGameObjects()
	{
		var walls = GameObject.FindGameObjectsWithTag("wall");
		foreach (var wall in walls)
		{
            MonoBehaviour.Destroy(wall);
		}
		var lines = GameObject.FindGameObjectsWithTag("line");
		foreach (var line in lines)
		{
            MonoBehaviour.Destroy(line);
		}
		var cubes = GameObject.FindGameObjectsWithTag("cube");
		foreach (var cube in cubes)
		{
            MonoBehaviour.Destroy(cube);
		}
		var players = GameObject.FindGameObjectsWithTag("tron");
		foreach (var player in players)
		{
            MonoBehaviour.Destroy(player);
		}
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

	private class PlayerModel
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
}

