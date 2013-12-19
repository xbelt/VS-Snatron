using System;
using System.Linq;
using Assets;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
public class MainController : MonoBehaviour
{
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

		// TODO register events for game and round control
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
		//_state = State.Lobby;
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
		_game.numberOfAIPlayers = 0;
		int rounds = 5; // TODO take from GameConfig
		_networkControl.broadCastStartGame (rounds);
		//GameObject.Find ("Network").networkView.RPC ("StartGame", RPCMode.All);
	}
	
	// A QuickGame is a normal "NetworkGame" with only the server playing, which is only 1 round
	private void StartQuickGame()
	{
		print ("GUI:StartQuickGame()");
		_game.setPlayer(0, _gui.PlayerName, false);
		_game.numberOfAIPlayers = 4;
		_networkControl.InitServer (0);
		_networkControl.broadCastStartGame (1);
	}
	
	// Disconnect from a running game
	private void LeaveGame()
	{
		_networkControl.Disconnect ();
		_game.StopGame ();
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

	// TODO : Game and Round Control

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
		Debug.Log ("GUI:OnConnectedToRemoteServer");
		_gui.ShowLobby (() => {return _game.Players; });
		// TODO ?
		// id:That's us!
		// needed only when game starts for now
	}
	
	private void OnConnectionError(String message)
	{
		Debug.Log ("GUI:OnConnectionError: " + message);
		_game.NewGame ();
		_networkControl.Disconnect ();
		_gui.ShowStartScreen (() => {return _networkControl.Servers;});
		// TODO show some error message?
		// TODO Clean up stuff?
	}
	
	// This is indirectly called through RPC StartGame event
	private void OnGameStarted(int rounds)
	{
		print ("GUI:OnGameStarted()");
		_gui.ShowGame ();
		_game.StartGame (_networkControl.PlayerID, rounds);
	}
	
	private void OnGameEnded()
	{
		print ("GUI:OnGameEnded()");
		// TODO make sure all players are disconnected
		// and they all show the main menu
		
		GameObject.Find("Network").networkView.RPC("StopGame", RPCMode.All); //TODO avoiding RPCMode.all
		_gui.ShowStartScreen (() => {return _networkControl.Servers;});
	}
	
	private void OnRoundStarted(int round) // TODO probably not necessary
	{
		Debug.Log ("GUI:OnRoundStarted");
		//_game.NewRound ();
	}
	
	private void OnRoundEnded(int round) // TODO probably not necessary
	{
		Debug.Log ("GUI:OnRoundEnded");
		//_game.NewRound ();
	}
	
	private void OnPlayerJoined(int id, String name, bool isAI)
	{
		Debug.Log ("GUI:OnPlayerJoined");
		_game.setPlayer (id, name, isAI);
	}
	
	private void OnPlayerLeft(int id)
	{
		Debug.Log ("GUI:OnPlayerLeft");
		_game.removePlayer (id);
	}
	
	private void OnPlayerKilled(int id)
	{
		Debug.Log ("GUI:OnPlayerKilled");
		_game.playerDied (id);
	}

	#endregion

}