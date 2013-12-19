using System.Collections.Generic;
using Assets;
using UnityEngine;

public class NetworkInterface : MonoBehaviour {

	// Local Player
	public static string PlayerName = "Player";
	private int _localPlayerID;
	public int PlayerID { get { return _localPlayerID; } }
	public static string HostName{ get { return PlayerName + "'s Game"; } }

	private readonly ServerDiscoverer Discoverer = new ServerDiscoverer ();
	public IEnumerable<Server> Servers { get { return Discoverer.Servers; } }
	private readonly Dictionary<string, int> _ip2playerId = new Dictionary<string, int> ();

	// Server Events
	public delegate void ServerStartedEvent(); // OK
	public ServerStartedEvent OnServerStarted;
	public delegate void MessageEvent(string msg); // ok
	public MessageEvent OnConnectionError;

	// Game/Round Events
	public delegate void GameEvent(int round); // ok
	public GameEvent OnGameStarted;
	public GameEvent OnRoundStarted;
	public GameEvent OnRoundEnded;
	public delegate void GameEndedEvent(); // ok
	public GameEndedEvent OnGameEnded;
	
	// Player List Events
	public delegate void PlayerJoinedEvent (int id, string name); // ok
	public PlayerJoinedEvent OnPlayerJoined;
	public delegate void PlayerChangeEvent(int id); // ok
	public PlayerChangeEvent OnConnectedToRemoteServer;
	public PlayerChangeEvent OnPlayerLeft;
	public PlayerChangeEvent OnPlayerSpawned;
	public PlayerChangeEvent OnPlayerKilled;

	#region Server Code

	private void InitNetworkInterface()
	{
		_localPlayerID = 0;
		_ip2playerId.Clear ();
		StartListeningForNewServers ();
		// TODO some more?
	}

	public void AnnounceServer() {
		StopSearching ();
		ServerHoster.HostServer(HostName);
		InitServer(Game.Instance.Level.MaxPlayers);
		if (OnServerStarted != null)
			OnServerStarted ();
	}
	
	public void StopAnnouncingServer() {
		ServerHoster.IsHosting = false;
	}

	public void InitServer(int maxPlayers)
	{
		print ("NET:InitServer()");
		Network.InitializeServer(maxPlayers, Protocol.GamePort, false);
		Network.sendRate = 30;
	}

	// Called on Server when a player connects : Assign player id to connected player
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected");
		int playerId = Game.Instance.getFirstFreePlayerId ();
		// Assign the new player a unique id
		AssignPlayerID (playerId, player);
		//add the host, since he's not in the buffer since he is added by GUI_control
		sendServerName (player);

		_ip2playerId.Add (player.ipAddress, playerId);
	}
	
	// Called on Server when a player disconnects : Destroy all objects from that player (Why would we do that? isn't it crappy if the walls tdissapear if one loses connection)
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (!_ip2playerId.ContainsKey(player.ipAddress))
			return;

		int playerId;
		_ip2playerId.TryGetValue (player.ipAddress, out playerId);

		Network.RemoveRPCs(player);
		//Network.DestroyPlayerObjects(player);
		// remove from player lists
		_ip2playerId.Remove(player.ipAddress);
		broadCastPlayerLeft (playerId);
	}

	#endregion

	#region Client Code

	void OnFailedToConnect(NetworkConnectionError e)
	{
		InitNetworkInterface ();
		if (OnConnectionError != null)
			OnConnectionError (e.ToString());
	}

	void OnConnectedToServer()
	{
		print("Connected");
		//update server list?
	}

	#endregion

	public void broadCastStartGame(int rounds)
	{
		print ("NET:broadCastStartGame()");
		GetComponent<NetworkView>().RPC("StartGame", RPCMode.AllBuffered, rounds);
	}

    [RPC]
    public void StartGame(int rounds)
	{
		print ("RPC:StartGame()");
		StopAnnouncingServer ();
		StopSearching();
		//Game.Instance.StartGame (PlayerID);
        if (OnGameStarted != null)
			OnGameStarted (rounds); 
	}
	
	public void broadCastStopGame()
	{
		Debug.Log ("NET:broadCastStopGame()");
		GetComponent<NetworkView>().RPC("StopGame", RPCMode.AllBuffered);
	}

	[RPC]
	public void StopGame() 
	{
		Debug.Log ("RPC:StopGame()");
		Disconnect ();
		//Game.Instance.StopGame ();
		InitNetworkInterface ();
		if (OnGameEnded != null)
			OnGameEnded ();
	}
	
	public void broadCastStartRound(int round)
	{
		Debug.Log ("NET:broadCastStartRound()");
		GetComponent<NetworkView>().RPC("StartRound", RPCMode.AllBuffered, round);
	}
	
	[RPC]
	public void StartRound(int round)
	{
		Debug.Log ("RPC:StartRound()");
		if (OnRoundStarted != null)
			OnRoundStarted (round);
	}
	
	public void broadCastEndRound(int round)
	{
		Debug.Log ("NET:broadCastEndRound()");
		GetComponent<NetworkView>().RPC("EndRound", RPCMode.AllBuffered, round);
	}
	
	[RPC]
	public void EndRound(int round)
	{
		Debug.Log ("RPC:EndRound()");
		if (OnRoundEnded != null)
			OnRoundEnded (round);
	}
	
	public void broadCastSpawnPlayer(int playerId)
	{
		Debug.Log ("NET:broadCastSpawnPlayer()");
		GetComponent<NetworkView>().RPC("SpawnPlayer", RPCMode.AllBuffered, playerId);
	}
	
	[RPC]
	public void SpawnPlayer(int playerId)
	{
		Debug.Log ("RPC:SpawnPlayer()");
		//Game.Instance.playerDied(playerId);
		if (OnPlayerSpawned != null)
			OnPlayerSpawned (playerId);
	}
	
	public void broadCastKillPlayer(int playerId)
	{
		Debug.Log ("NET:broadCastKillPlayer()");
		GetComponent<NetworkView>().RPC("KillPlayer", RPCMode.AllBuffered, playerId);
	}

    [RPC]
	public void KillPlayer(int playerId)
	{
		Debug.Log ("RPC:KillPlayer()");
		//Game.Instance.playerDied(playerId);
		if (OnPlayerKilled != null)
			OnPlayerKilled (playerId);
	}
	
	public void AssignPlayerID(int playerId, NetworkPlayer target)
	{
		Debug.Log ("NETAssignPlayerID()");
		GetComponent<NetworkView>().RPC("AssignLocalPlayerID", target, playerId);
	}

	// The server sends this call to a connecting player to assign him his personal id.
	[RPC]
	private void AssignLocalPlayerID(int playerId)
	{
		Debug.Log("RPC: AssignLocalPlayerID()"); 
		_localPlayerID = playerId;
		// Tell everybody who we are
		broadCastPlayerJoined (PlayerName, playerId);
		if (OnConnectedToRemoteServer != null)
			OnConnectedToRemoteServer (playerId);
	}
	
	private void broadCastPlayerJoined(string playerName, int playerId)
	{
		Debug.Log("NET: broadCastPlayerJoined()"); 
		GetComponent<NetworkView>().RPC("PlayerJoined", RPCMode.AllBuffered, playerName, playerId);
	}

	private void sendServerName(NetworkPlayer target)
	{
		Debug.Log("NET: sendServerName()"); 
		GetComponent<NetworkView>().RPC("PlayerJoined", target, PlayerName, PlayerID);
	}
	
	[RPC]
	private void PlayerJoined(string playerName, int playerId)
	{
		Debug.Log("RPC:PlayerJoined");
		//Game.Instance.setPlayer (playerId, playerName);
		if (OnPlayerJoined != null)
			OnPlayerJoined (playerId, playerName);
	}
	
	public void broadCastPlayerLeft(int playerId)
	{
		Debug.Log ("NET:broadCastPlayerLeft()");
		GetComponent<NetworkView>().RPC("PlayerLeft", RPCMode.AllBuffered, playerId);
	}

	[RPC]
	private void PlayerLeft(int playerId)
	{
		Debug.Log ("RPC:PlayerLeft" + playerId);

		if (OnPlayerLeft != null)
			OnPlayerLeft (playerId);
	}

	#region Server Discovery control and joining
	
	public void StartListeningForNewServers() {
		Debug.Log ("NET:StartListeningForNewServers()");
		Discoverer.StartListeningForNewServers ();
	}
	
	public void StopSearching() {
		Debug.Log ("NET:StopSearching()");
		Discoverer.StopSearching ();
	}

	// Connect to a remote server
	public void JoinGame(string ip, int port) {
		Debug.Log ("NET:JoinGame()");
		StopSearching();
		StopAnnouncingServer();
		Network.Connect(ip, port);
	}
	
	public void Disconnect() {
		Debug.Log ("NET:Disconnect()");
		Network.Disconnect ();
		InitNetworkInterface ();
	}
	
	#endregion
}
