using System.Collections.Generic;
using Assets;
using UnityEngine;

public class NetworkControl : MonoBehaviour {
	// Local Player
    public int PlayerID;
	public static string PlayerName = "Player";
	private static readonly ServerDiscoverer Discoverer = new ServerDiscoverer ();
	public static IEnumerable<Server> Servers { get { return Discoverer.Servers; } }
	// Hosting
	public static string HostName{ get { return PlayerName + "'s Game"; } }
	// Client
	public readonly Dictionary<string, int> _ip2playerId = new Dictionary<string, int> ();

   	private int _currentPlayerID;

    public void StartListeningForNewServers() {
		Discoverer.StartListeningForNewServers ();
	}
	
	public void StopSearching() {
		Discoverer.StopSearching ();
	}
	
	public void AnnounceServer() {
		StopSearching ();
        ServerHoster.HostServer(HostName);
        Network.InitializeServer(Game.MaxPlayers, Protocol.GamePort, false);
        Network.sendRate = 30;
    }

    public void StopAnnouncingServer() {
        ServerHoster.IsHosting = false;
    }

    public void Connect(string ip, int port) {
        Network.Connect(ip, port);
    }

    private void Disconnect() {
		Network.Disconnect ();
	}

    [RPC]
	private void SetPlayer(string playerName, int playerId)
    {
        Debug.Log("Received SetPlayer RPC");
		Game.Instance.setPlayer (playerId, playerName);
    }

    [RPC]
	private void SetPlayerID(int playerId)
    {
        Debug.Log("received RPC from server(hopefully from server)"); 
		PlayerID = playerId;
		GetComponent<NetworkView>().RPC("SetPlayer", RPCMode.AllBuffered, PlayerName, playerId);
    }

	public delegate void PlayerConnectedEvent();

	// Called on Server when a player connects : Assign player id to connected player
    void OnPlayerConnected(NetworkPlayer player)
    {
		int playerId = Game.Instance.getFirstFreePlayerId ();
        GetComponent<NetworkView>().RPC("SetPlayerID", player, Game.Instance.getFirstFreePlayerId());
		GetComponent<NetworkView>().RPC("SetPlayer", player, PlayerName, 0); //add the host, since he's not in the buffer since he is added by GUI_control which uses this via static functions which cannot do RPC </rant>
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
		GetComponent<NetworkView>().RPC("SetPlayer", RPCMode.AllBuffered, null, playerId);
	}
	
	public delegate void GameStarted();
	public delegate void GameEnded();

	public GameStarted OnGameStarted;
	public GameEnded OnGameEnded;

    [RPC]
    public void StartGame() {
		StopAnnouncingServer ();
		Game.Instance.StartGame (PlayerID);
        if (OnGameStarted != null)
			OnGameStarted (); 
    }

	[RPC]
	public void StopGame() {
		Disconnect ();
		Game.Instance.StopGame ();
		ResetPreGameValues ();
		if (OnGameEnded != null)
			OnGameEnded ();
	}

    [RPC]
    public void KillPlayer(int id)
    {
        ID2AliveState[id] = false;
    }

	private void ResetPreGameValues() {
		PlayerID = 0;
		StartListeningForNewServers ();
	    PlayerIsAlive = true;
	    // TODO some more?
	}
}
