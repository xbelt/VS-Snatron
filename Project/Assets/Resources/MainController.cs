using System;
using System.Linq;
using Assets;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
using System.Threading;
using System.Collections;


public class MainController : MonoBehaviour
{
	// TODO : make this configurable.
	private bool _withAiPlayers = true;
	private int _singlePlayerRounds = 3;


	public UserInterface _gui;
	public NetworkInterface _networkControl;
	private Game _game;

    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        ChangeWifiSettingsAndroid();

		initNetworkControl ();
		initGame ();
		initGui ();
    }

    private void ChangeWifiSettingsAndroid() { } // TODO what was this for?

	private void initGui()
	{
		_gui.OnStartServerRequest += StartServer;
		_gui.OnStartNetworkGameRequest += StartNetworkGame;
		_gui.OnStartQuickGameRequest += StartQuickGame;
		_gui.OnJoinGameRequest += JoinGame;
		_gui.OnLeaveGameRequest += LeaveGame;
		_gui.OnCloseAppRequest += ExitApp;

		_gui.ShowStartScreen (() => {return _networkControl.Servers;});
	}
	
	private void initGame()
	{
		_game = Game.Instance;
		
		_game.OnLocalDeath += OnLocalDeath;
		_game.OnOnePlayerLeft += OnOnePlayerLeft;
		_game.OnLastHumanDied += OnLastHumanDied;
		_game.OnLastRoundEnded += OnLastRoundEnded;
	}

	private void initNetworkControl()
	{
		_networkControl.OnServerStarted += OnServerStarted;
		_networkControl.OnConnectedToRemoteServer += OnConnectedToRemoteServer;
		_networkControl.OnConnectionError += OnConnectionError;
		_networkControl.OnGameEnded += OnGameEnded;
		_networkControl.OnGameStarted += OnGameStarted;
		_networkControl.OnRoundStarted += OnRoundStarted;
		_networkControl.OnRoundEnded += OnRoundEnded;
		_networkControl.OnPlayerJoined += OnPlayerJoined;
		_networkControl.OnPlayerLeft += OnPlayerLeft;
		_networkControl.OnPlayerKilled += OnPlayerKilled;
		
		_networkControl.StartListeningForNewServers();
	}

	#region GUI Event Handlers

	private void StartServer()
	{
		print ("GUI,Server:StartServer()");
		_game.setPlayer (0, _gui.PlayerName, false);
		_networkControl.AnnounceServer ();
	}
	
	private void JoinGame(String ipAddress){
		print ("GUI,Client:StartNetworkGame()");
		//_state = State.Lobby;
		_networkControl.JoinGame(ipAddress, Protocol.GamePort);
	}
	
	// Called by server host
	private void StartNetworkGame()
	{
		print ("GUI,Server:StartNetworkGame()");
		_networkControl.StopAnnouncingServer();
		AddAIPlayers (_game.Level.MaxPlayers - _game.NofPlayers);
		int rounds = 5; // TODO take from GameConfig
		_networkControl.broadCastBeginGame (rounds);
	}
	
	// A QuickGame is a normal "NetworkGame" with only the server playing, which is only 1 round
	private void StartQuickGame()
	{
		print ("GUI:StartQuickGame()");
		_networkControl.StopSearching ();
		_game.setPlayer(0, _gui.PlayerName, false);
		_networkControl.InitServer (0);
		AddAIPlayers (_game.Level.MaxPlayers - 1);
		_networkControl.broadCastBeginGame (_singlePlayerRounds);
	}

	private void AddAIPlayers(int count)
	{
		if (!_withAiPlayers)
			return;

		for (int i = 1; i <= count; i++) {
			int id = _game.getFirstFreePlayerId ();
			_game.setPlayer(id, "AI " + id, true);
			_networkControl.broadCastPlayerJoined("AI " + i, id, true);
		}
	}
	
	// Disconnect from a running game
	private void LeaveGame()
	{
		_networkControl.Disconnect ();
		_game.LeaveGame ();
		_gui.ShowStartScreen (() => {return _networkControl.Servers;});
	}
	
	private void ExitApp()
	{
		_networkControl.StopAnnouncingServer ();
		_networkControl.Disconnect ();
		Application.Quit ();
	}

	#endregion

	#region Game Event Handlers

	void OnLocalDeath (int playerId)
	{
		_networkControl.KillPlayer (playerId);
	}
	
	void OnOnePlayerLeft ()
	{
		Debug.Log ("Main:On One Human Left");
		if (Network.isServer) {
			_networkControl.EndRound (_game.CurrentRound);
		}
	}
	
	void OnLastHumanDied ()
	{
		Debug.Log ("MC:On Last Human Died");
		if (Network.isServer && !_game.isGameOver) {
			foreach (PlayerModel player in _game.Players)
			{
				if (player != null && player.isAI == true && player.isAlive)
					_networkControl.KillPlayer(player.id);
			}
		}
	}
	
	void OnLastRoundEnded ()
	{
		if (Network.isServer) {
			_networkControl.broadCastEndGame();
		}
	}

	#endregion
		
	#region Network Control event handlers

	private void OnServerStarted()
	{
		Debug.Log ("GUI:OnServerStarted");
		_game.setPlayer(0, NetworkInterface.PlayerName, false);
		_gui.ShowLobby (() => {return _game.Players; });
	}

	private void OnConnectedToRemoteServer(int id)
	{
		Debug.Log ("GUI:OnConnectedToRemoteServer:" + id);
		_game.setPlayer (id, _gui.PlayerName, false); // TODO maybe not needed
		_gui.ShowLobby (() => {return _game.Players; });
		// TODO ?
		// id:That's us!
		// needed only when game starts for now
	}
	
	private void OnConnectionError(String message)
	{
		Debug.Log ("GUI:OnConnectionError: " + message);
		_game.Spawner.ClearMyObjects ();
		_game.InitGame ();
		_networkControl.Disconnect ();
		_gui.ShowStartScreen (() => {return _networkControl.Servers;});
		// TODO show some error message?
	}

	private const int TimeToShowInitGameScreen = 3;

	// This is indirectly called through RPC StartGame event
	private void OnGameStarted(int rounds)
	{
		print ("GUI:OnGameStarted()" + " " + _networkControl.PlayerID);
		_gui.ShowInitGame ();
		_game.StartGame (_networkControl.PlayerID, rounds);

		if (Network.isServer) {
			Invoke("DelayedStartRoundHelper", TimeToShowInitGameScreen);
		}
	}

	private void DelayedStartRoundHelper()
	{
		_networkControl.broadcastBeginRound (_game.CurrentRound + 1);
	}
	
	private void OnRoundStarted(int round)
	{
		Debug.Log ("GUI:OnRoundStarted");
		_gui.ShowGame ();
		_game.BeginRound (round);
	}
	
	private void OnRoundEnded(int round)
	{
		Debug.Log ("GUI:OnRoundEnded");
		_gui.ShowBetweenRounds ();
		_game.EndRound ();
	}
	
	private void OnGameEnded()
	{
		print ("GUI:OnGameEnded()");
		_game.EndGame ();
		_gui.ShowRanking (() => _game.Ranking);
	}
	
	private void OnPlayerJoined(int id, String name, bool isAI)
	{
		Debug.Log ("GUI:OnPlayerJoined: " + id + " " + name);
		_game.setPlayer (id, name, isAI);
	}
	
	private void OnPlayerLeft(int id)
	{
		Debug.Log ("GUI:OnPlayerLeft" + " " + id);
		_game.removePlayer (id);
	}
	
	private void OnPlayerKilled(int id)
	{
		Debug.Log ("GUI:OnPlayerKilled" + " " + id);
		_game.OnGlobalKill (id);
	}

	#endregion

}