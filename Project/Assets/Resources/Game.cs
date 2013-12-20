using Assets.Resources;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game
{
	// Singleton
	private static Game _instance = new Game();
	public static Game Instance { get { return _instance; } }

	private Level _level;
	private Spawner _spawner;

	private Transform _playerPrefab;
	private Transform _gridPrefab;

	private int _localPlayerId;

	//public PlayerModel LocalPlayerModel { get { return _players [PlayerID]; } }
	private PlayerModel[] _players;

	private int _aiPlayers;
    private int _aiPlayerAlive;

	private int _humanPlayers;
	private int _humanPlayersAlive;

	private int _roundsToPlay;
	private int _currentRound;

	private PlayerModel[] _ranking;

	#region Events
	public delegate void GameEvent();
	public GameEvent OnLastRoundEnded;
	public GameEvent OnOnePlayerLeft;
	public GameEvent OnLastHumanDied;
	public GameEvent OnNextRound;
	
	public delegate void PlayerEvent(int playerId);
	public PlayerEvent OnLocalDeath;

	#endregion

    private Game() {
		_level = new BasicLevelModel ();
		_spawner = new Spawner (_level);
		_players = new PlayerModel[_level.MaxPlayers];
		InitGame ();
	}

	/// <summary>
	/// Start the game and give the local player control of the tron with localPlayerId
	/// rounds tells how many rounds one game last
	/// This is called indirectly through rpc
	/// </summary>
	/// <param name="localPlayerId">Local player identifier.</param>
	/// <param name="rounds">Rounds.</param>
	public void StartGame(int localPlayerId, int rounds)
	{
		Debug.Log ("Game: Start Game: " + " " + localPlayerId + " " + rounds + " " + _aiPlayers + " " + _humanPlayers);
		_roundsToPlay = rounds;
		_localPlayerId = localPlayerId;
		_spawner.LocalPlayerId = localPlayerId;
		// TODO make sure local player and all other human players are contained in _players

		Debug.Log ("Players starting game: ");
		foreach (PlayerModel player in _players) {
			if (player == null)
				continue;
			Debug.Log (player.id + " " + player.name);
			player.startGame();
		}
	}

	public void BeginRound(int round)
	{
		_currentRound = round;
		_aiPlayerAlive = 0;
		_humanPlayersAlive = 0;
		foreach (PlayerModel player in _players) {
			if (player == null)
				continue;
			if (player.isAI)
				_aiPlayerAlive++;
			else
				_humanPlayersAlive++;
		}
		//_aiPlayerAlive = _aiPlayers;
		//_humanPlayersAlive = _humanPlayers;

		for (int i = 0; i < _level.MaxPlayers; i++) {
			if (_players[i] != null)
				_players[i].startRound();
		}

		if (Network.isServer) {
			_spawner.InstantiateLevelObjects();
			SpawnAIPlayers ();
		}

		_spawner.SpawnLocalPlayer (OnLocalKill);
	}

	public void EndRound()
	{
		_spawner.ClearMyObjects ();
		
		for (int i = 0; i < _level.MaxPlayers; i++)
		{
			if (_players[i] != null)
				_players[i].score += NofPlayers - _players[i].rank;
		}
		
		if (Network.isServer) {
			// Are we finished, or will we continue playing?
			if (isGameOver)
				OnLastRoundEnded();
			else
				OnNextRound();
		}
	}
	
	public void EndGame()
	{
		_currentRound = 0;
		_ranking = getRanking ();
	}
	
	public void LeaveGame()
	{
		InitGame ();
	}

	/// <summary>
	/// Reset the game instance.
	/// Before calling, make sure there is no active network connection.
	/// </summary>
	public void InitGame()
	{
		_humanPlayers = 0;
		_humanPlayersAlive = 0;
		_aiPlayers = 0;
		_aiPlayerAlive = 0;
		_currentRound = 0;
		
		for (int i = 0; i < _level.MaxPlayers; i++)
		{
			_players[i] = null;
		}
		_spawner.ClearAllObjects ();
	}

	/// This event is triggered by locally spawned objects : need to inform all others about kill
	private void OnLocalKill(int playerId)
	{
		_spawner.Kill (playerId); // "physically kill" (unity engine)
		OnLocalDeath(playerId); // "logically kill" (game logic)
	}

	/// This is in response to the broadcast message fired after a OnLocalKill : everybody executes this.
	public void OnGlobalKill(int playerId)
	{
		PlayerModel player = _players [playerId];
		if (player == null || !player.isAlive)
			return;
		Debug.Log ("Game:On Global Kill");

		player.rank = NofLivingPlayers;
		player.isAlive = false;
		if (player.isAI)
			_aiPlayerAlive--;
		else
			_humanPlayersAlive--;

		Debug.Log ("Players alive " + _humanPlayersAlive + " " + _aiPlayerAlive);

		if (Network.isServer) {
			if (_humanPlayersAlive == 0 && player.isAI == false)
				OnLastHumanDied();
			else if (NofLivingPlayers <= 1)
				OnOnePlayerLeft();
		}
	}



	#region game initialization
	void SpawnAIPlayers ()
	{
		foreach (var player in _players) {
			if (player == null)
				return;
			if (player.isAI)
				_spawner.SpawnAI (player.id, OnLocalKill);
		}
	}

	// Called when both local player and remote player was spawned? TODO really?
	public void setPlayer(int playerId, string playerName, bool isAI) {
        MonoBehaviour.print("Game:SetPlayer()");
		PlayerModel player = new PlayerModel (playerId, playerName);
		player.isAI = isAI;
	    if (isAI) {
	        _aiPlayers++;
	    }
	    else {
	        _humanPlayers++;
	    }

	    _players [playerId] = player;
	}

	public void removePlayer(int playerId) {
        Debug.Log("Game:removePlayer()");
		PlayerModel player = _players [playerId];
		if (player.isAI) {
			_aiPlayers--;
			if (player.isAlive)
				_aiPlayerAlive--;
		} else {
			_humanPlayers--;
			if (player.isAlive)
				_humanPlayersAlive--;
		}

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

	#region Game State Accessors
	
	public Level Level { get {return _level;} }
	public Spawner Spawner { get { return _spawner; } }
	public int PlayerID { get {return _localPlayerId;} }

	public PlayerModel[] Players { get { return _players; } }
	public int RoundsToPlay { get { return _roundsToPlay; } }
	public int NofPlayers { get { return _humanPlayers + _aiPlayers; } }
	public int NofLivingPlayers { get { return _humanPlayersAlive + _aiPlayerAlive; } }
	
	public int CurrentRound { get { return _currentRound; } }
	public bool HasGameStarted { get { return _currentRound > 0; } }
	public bool isRoundOver { get {	return HasGameStarted && NofLivingPlayers <= 1; } }
	public bool isGameOver { get { return isRoundOver && RoundsToPlay == _currentRound; } }

	public bool HasLocalPlayerWon {	get { return hasWon (_localPlayerId); } }
	public bool IsLocalPlayerAlive { get { return _players [_localPlayerId].isAlive; } }

	public PlayerModel[] Ranking { get { return _ranking; } }
	
	// Does the player have the highest score?
	// Condition: for this to become true, the game must be over
	// => nobody wins until the end of the game (after all rounds)
	public bool hasWon(int playerId) {
		return _ranking != null && _ranking [0].id == playerId;
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

	public class ScoreComparer : IComparer<PlayerModel>
	{
		public int Compare(PlayerModel a, PlayerModel b)
		{
			if (a == null && b == null)
				return 0;
			if (a == null)
				return 1;
			if (b == null)
				return -1;
			return a.score.CompareTo(b.score);
		}
	}

	private PlayerModel[] getRanking()
	{
		PlayerModel[] sorted = (PlayerModel[]) _players.Clone();
		System.Array.Sort<PlayerModel> (sorted, new ScoreComparer ());
		return sorted;
	}

	#endregion
}

