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
    public static int PlayerRank { get; set; }

    public static bool PlayerIsAlive = true;

    public static readonly Dictionary<string, int> _ip2playerId = new Dictionary<string, int> ();
    public static readonly Dictionary<int, bool> ID2AliveState = new Dictionary<int, bool> ();
	// Client

   	private static int _currentPlayerID;

    public static void StartListeningForNewServers() {
		Discoverer.StartListeningForNewServers ();
	}
	
	public static void StopSearching() {
		Discoverer.StopSearching ();
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

    private static void Disconnect() {
		Network.Disconnect ();
	}

    [RPC]
	private void SetPlayer(string playerName, int playerID)
    {
        Debug.Log("Received SetPlayer RPC");
		Game.Instance.setPlayer (playerID, playerName);
        ID2AliveState.Add(playerID, true);
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
        ID2AliveState.Add(playerId, true);

        GetComponent<NetworkView>().RPC("SetPlayerID", player, Game.Instance.getFirstFreePlayerId());
		GetComponent<NetworkView>().RPC("SetPlayer", player, PlayerName, 0); //add the host, since he's not in the buffer since he is added by GUI_control which uses this via static functions which cannot do RPC </rant>
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
		Game.Instance.StartGame (PlayerID); //TODO move all direct interaction out of network control
        ID2AliveState.Add(PlayerID, true);
        GameObject.Find("GUIObject").GetComponent<GUI_Control>()._state = GUI_Control.State.Game;
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
