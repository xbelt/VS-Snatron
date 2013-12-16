using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets;
using UnityEngine;
using System.Collections;

public class NetworkControl : MonoBehaviour {
	// Local Player
	private int PlayerID;
	public static string PlayerName = "Player";
	private static readonly ServerDiscoverer discoverer = new ServerDiscoverer ();
	public static List<Server> Servers { get { return discoverer.Servers; } }
	// Hosting
	public static string HostName{ get { return PlayerName + "'s Game"; } }
    public static bool PlayerIsAlive { get; set; }

    private readonly Dictionary<string,int> _ip2playerId = new Dictionary<string, int> ();

	// Client

   	private static int _currentPlayerID = 0;

    public static void StartListeningForNewServers() {
		discoverer.StartListeningForNewServers ();
	}
	
	public static void StopSearching() {
		discoverer.StopSearching ();
	}
	
	public static void AnnounceServer() {
		StopSearching ();
        ServerHoster.HostServer(HostName);
        Network.InitializeServer(Game.MaxPlayers, Protocol.GamePort, false);
        Network.sendRate = 30;
    }

    public static void StopAnnouncingServer() {
        ServerHoster.IsHosting = false;
    }

    public static void Connect(string ip, int port) {
        Network.Connect(ip, port);
    }

	public static void Disconnect() {
		Network.Disconnect ();
	}

    void OnGUI()
    {
		// Marko: Is this necessary??
        //GUI.Label(new Rect(100, 100, 150, 100), string.Join(", ", PlayerId2Username.Select((x) => x.Key + ": " + x.Value).ToArray()));
    }

    private static int NextPlayerID() {
        return ++_currentPlayerID;
    }

    [RPC]
	private void SetPlayer(string playerName, int playerID)
    {
        Debug.Log("Received SetPlayer RPC");
		Game.Instance.setPlayer (playerID, playerName);
    }

    [RPC]
    private void SetPlayerID(int id)
    {
        Debug.Log("received RPC from server(hopefully from server)"); 
        PlayerID = id;
		GetComponent<NetworkView>().RPC("SetPlayer", RPCMode.AllBuffered, PlayerName, id);
    }

	// Called on Server when a player connects : Assign player id to connected player
    void OnPlayerConnected(NetworkPlayer player)
    {
		int playerId = Game.Instance.getFirstFreePlayerId ();
		_ip2playerId.Add (player.ipAddress, playerId);

        GetComponent<NetworkView>().RPC("SetPlayerID", player, Game.Instance.getFirstFreePlayerId());
		GetComponent<NetworkView>().RPC("SetPlayer", player, PlayerName, 0); //add the host, since he's not in the buffer since he is added by GUI_control which uses this via static functions which cannot do RPC </rant>
    }
	
	// Called on Server when a player disconnects : Destroy all objects from that player
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (!_ip2playerId.ContainsKey(player.ipAddress))
			return;

		int playerId;
		_ip2playerId.TryGetValue (player.ipAddress, out playerId);

		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
		// remove from player lists
		_ip2playerId.Remove(player.ipAddress);
		GetComponent<NetworkView>().RPC("SetPlayer", RPCMode.AllBuffered, null, playerId);
	}
	
	public delegate void GameStarted();
	public delegate void GameEnded();

	public GameStarted OnGameStarted;
	public GameEnded OnGameEnded;

    [RPC]
    public void StartGame() {
		StopAnnouncingServer ();
		Game.NewGame().StartGame (PlayerID);; //TODO move all direct interaction out of network control
		if (OnGameStarted != null)
			OnGameStarted ();
    }

	[RPC]
	public void StopGame() {
		Disconnect ();
		Game.Instance.StopGame ();
		resetPreGameValues ();
		if (OnGameEnded != null)
			OnGameEnded ();
	}


	private void resetPreGameValues() {
		PlayerID = 0;
		StartListeningForNewServers ();
		// TODO some more?
	}
}
