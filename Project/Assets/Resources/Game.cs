using Assets.Resources;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game
{
	// TODO put into game config
	private static readonly int GAME_LENGTH = 5;

	// The only game instance
	private static Game _instance = new Game();
	public static Game Instance { get { return _instance; } }

	private Level _level;
	public Level Level { get {return _level;} }
	
	private Spawner _spawner;
	public Spawner Spawner { get { return _spawner; } }

	private Transform _playerPrefab;
	private Transform _gridPrefab;

	private int _localPlayerId;
	public int PlayerID { get {return _localPlayerId;} }

	private bool _gameStarted;
    public bool GameStarted {
        get { return _gameStarted; }
    }

	//public PlayerModel LocalPlayerModel { get { return _players [PlayerID]; } }
	private PlayerModel[] _players;
	public PlayerModel[] Players { get { return _players; } }

	private int _nOfActivePlayers;
	public int NofActivePlayers { get {	return _nOfActivePlayers; }	}
	
	private int _nOfLivingPlayers;
	public int NofLivingPlayers { get { return _nOfLivingPlayers; } }
    public int numberOfAIPlayers;
    public int numberOfLivingKIPlayers;

	private int _roundsToPlay;
	public int RoundsToPlay { get { return _roundsToPlay; } }

    private Game() {
		_level = new BasicLevelModel ();
		_spawner = new Spawner (_level);
		_players = new PlayerModel[_level.MaxPlayers];
		NewGame ();
	}

	// Start the game and give the local player control of the tron with localPlayerId
	// rounds tells how many rounds one game last
	public void StartGame(int localPlayerId, int rounds)
	{
		_roundsToPlay = rounds;
		_localPlayerId = localPlayerId;
		_spawner.LocalPlayerId = localPlayerId;
		// TODO make sure local player and all other human players are contained in _players
		// And alive!

		Debug.Log ("Players starting game: " + _players.ToString());
		_spawner.SpawnLocalPlayer(OnLocalKill);

		if (Network.isServer)
		{
			_spawner.InstantiateLevelObjects();
			AddAIPlayers ();
		}
		
		_gameStarted = true;
		if (HasLocalPlayerWon()) { // that is, when the player is alone
			_gameStarted = false;
			EndGame();
		}
	}

	void AddAIPlayers ()
	{
		for (int i = 0; i < numberOfAIPlayers; i++) {
			int id = getFirstFreePlayerId ();
			setPlayer (id, "AI " + (i + 1), true);
			_spawner.SpawnAI (id, OnLocalKill);
		}
	}

    public void StopGame()
	{
		clearGameObjects ();
		NewGame ();
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
	// Reset the game instance
	public void NewGame()
	{
		_nOfActivePlayers = 0;
		_nOfLivingPlayers = 0;
		_roundsToPlay = GAME_LENGTH;
		_gameStarted = false;
		
		for (int i = 0; i < _level.MaxPlayers; i++)
		{
			_players[i] = null;
		}
	}
	
	public void NewRound()
	{
		_roundsToPlay--;
		_nOfLivingPlayers = _nOfActivePlayers;
		for (int i = 0; i < _level.MaxPlayers; i++)
		{
			if (_players[i] == null)
				continue;
			_players[i].score += _nOfActivePlayers - _players[i].rank;
			_players[i].startRound();
		}
	}

	private void OnLocalKill(int playerId)
	{
		_players [playerId].isAlive = false;
		_spawner.Kill (playerId);
	}
	
	private void OnRemoteKill(int playerId)
	{
		// TODO necessary? (not yet called)
		_players [playerId].isAlive = false;
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

	public bool isRoundOver()
	{
		if (!_gameStarted)
			return false;
		if (_nOfActivePlayers > 1)
			return _nOfLivingPlayers <= 1;
		if (numberOfAIPlayers > 0) {
			return numberOfLivingKIPlayers <= 0;
		}
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
	public bool hasWon(int playerId) {
		return NofLivingPlayers <= 1 && numberOfLivingKIPlayers <= 0;
	}
	
	// Does he have the highest score?
	public bool HasLocalPlayerWon()
	{
		return hasWon (PlayerID);
	}

	#region game initialization

	// Called when both local player and remote player was spawned? TODO really?
	public void setPlayer(int playerId, string playerName, bool isAI) {
        MonoBehaviour.print("Game:SetPlayer()");
		PlayerModel player = new PlayerModel (playerId, playerName);
		player.isAI = isAI;
		_players [playerId] = player;
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
	    var gameWalls = GameObject.FindGameObjectsWithTag("gameWall");
	    foreach (var gameWall in gameWalls) {
	        MonoBehaviour.Destroy(gameWall);
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
	    var powerUp0s = GameObject.FindGameObjectsWithTag("powerUp0");
	    foreach (var powerUp0 in powerUp0s) {
	        MonoBehaviour.Destroy(powerUp0);
	    }
	    var powerUp1s = GameObject.FindGameObjectsWithTag("powerUp1");
	    foreach (var powerUp1 in powerUp1s) {
	        MonoBehaviour.Destroy(powerUp1);
	    }
	}
}

