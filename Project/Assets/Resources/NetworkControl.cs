using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets;
using UnityEngine;
using System.Collections;

public class NetworkControl : MonoBehaviour {
    public static bool IsSearching = true;
    public static readonly List<Server> Servers = new List<Server>();
    public static int PlayerID;
    public static readonly Dictionary<int, String> PlayerId2Username = new Dictionary<int, String>();
    private static int _currentPlayerID = 0;
    public static string HostName = "";
    public static string PlayerName = "Player";
    public static int NumberOfPlayers = 0;
    public static int NumberOfCubes = 5;
    private const int FieldBorderCoordinates = 200;
    private static Dictionary<int, Vector3> StartPositions = new Dictionary<int, Vector3>();  

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void StartListeningForNewServers() {
        (new Thread(() =>
        {
            while (IsSearching)
            {
                var newServer = ServerDiscoverer.DiscoverServers();
                Debug.Log("Discovered new Server");
                var addServer = true;
                foreach (var server in Servers.Where(server => server.Ip.Equals(newServer.Ip)))
                {
                    addServer = false;
                }
                if (addServer && newServer != null && newServer.Name != null)
                {
                    Servers.Add(newServer);
                }
            }
        })
            ).Start();
    }

    public static void AnnounceServer() {
        ServerHoster.HostServer(HostName);
        Network.InitializeServer(NumberOfPlayers, Protocol.GamePort, false);
        Network.sendRate = 30;
        InitializeStartPositions();
    }

    private static void InitializeStartPositions() {
        for (int i = 0; i < NumberOfPlayers + 1; i++) {
            double angle = 360/(i + 1) * Math.PI / 180;
            //TODO: check calculations
            StartPositions.Add(i, new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle)));
        }
    }

    public static void StopAnnouncingServer() {
        ServerHoster.IsHosting = false;
    }

    public static void Connect(string ip, int port) {
        Network.Connect(ip, port);
    }
    void OnGUI()
    {

        GUI.Label(new Rect(100, 100, 150, 100), string.Join(", ", PlayerId2Username.Select((x) => x.Key + ": " + x.Value).ToArray()));
       
    }


    public static void InstantiateCubes() {
        List<Transform> cubes = new List<Transform>();
        System.Random random = new System.Random();
        var shader = Shader.Find("Diffuse");
        for(int i = 0; i < NumberOfCubes; ++i) {
            int _x = random.Next(-(int) FieldBorderCoordinates, (int) FieldBorderCoordinates);
            int _z = random.Next(-(int) FieldBorderCoordinates, (int) FieldBorderCoordinates);
            cubes.Add(Network.Instantiate(Resources.Load<Transform>("Cube"), new Vector3((float)_x, 1.5f, (float)_z), Quaternion.identity, 0) as Transform);
            cubes[i].renderer.material.shader = shader;
        }
    }

    public static void InstantiateGameBorders() {
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

    public static void StopSearching() {
        IsSearching = false;
    }

    private static int NextPlayerID() {
        return ++_currentPlayerID;
    }


    
    [RPC]
   private void AddPlayer(string playerName, int playerID)
    {
        Debug.Log("Received addPlayer RPC");
        PlayerId2Username.Add(playerID, playerName);

    }

    [RPC]
    private void SetPlayerID(int id)
    {
        Debug.Log("received RPC from server(hopefully from server)"); 
        PlayerID = id;
        GetComponent<NetworkView>().RPC("AddPlayer", RPCMode.AllBuffered, PlayerName, id);
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        GetComponent<NetworkView>().RPC("SetPlayerID", player, NextPlayerID());
        GetComponent<NetworkView>().RPC("AddPlayer", player, PlayerName, 0); //add the host, since he's not in the buffer since he is added by GUI_control which uses this via static functions which cannot do RPC </rant>
    }
    private class Game
    {
        private readonly Transform _playerPrefab;
        private readonly Transform _gridPrefab;

        public Game(Transform playerPrefab, Transform gridPrefab)
        {
            _playerPrefab = playerPrefab;
            _gridPrefab = gridPrefab;
        }

		public void StartGame()
		{
			SpawnPlayer();
		}

        private void SpawnPlayer()
        {
			Vector3 location = new Vector3();
			Quaternion orientation = new Quaternion ();
			mapStartLocation (PlayerID, ref location, ref orientation);

            var player = Network.Instantiate(_playerPrefab, location, orientation, 0) as Transform;
            var cam = GameObject.Find("Main Camera");
            cam.AddComponent<SmoothFollow>().target = player;

            Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
            Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
        }

		private void mapStartLocation(int playerId,ref Vector3 location, ref Quaternion orientation)
		{
			// This is how players are arranged in a square, by id
			// 4 0 6
			// 3 * 2
			// 7 1 5

			// Orientations:
			// 0,1,2,3 look towards the center * @(0/0/0)
			// 4,5,6,7 look in clockwise direction

            float dist = FieldBorderCoordinates/2;

			switch (playerId) {
			case 0: location.Set(0, 0, -dist); orientation.SetFromToRotation(Vector3.zero, location); break;
            case 1: location.Set(0, 0, dist); orientation.SetFromToRotation(Vector3.zero, location); break;
			case 2:	location.Set( dist, 0, 0); orientation.SetFromToRotation(Vector3.zero, location); break;
			case 3:	location.Set(-dist, 0, 0); orientation.SetFromToRotation(Vector3.zero,location); break;
			case 4:	location.Set(-dist, 0, -dist); orientation.SetFromToRotation(new Vector3(0, 0, -dist),location); break;
			case 5:	location.Set( dist, 0,  dist); orientation.SetFromToRotation(new Vector3(0, 0, dist), location); break;
			case 6:	location.Set( dist, 0, -dist); orientation.SetFromToRotation(new Vector3( dist, 0, 0), location); break;
			case 7:	location.Set(-dist, 0,  dist); orientation.SetFromToRotation(new Vector3( -dist, 0, 0), location); break;
			}
		}
    }

    [RPC]
    public void StartGame() {
        Destroy(GameObject.Find("SplashScreenLight"));
        Destroy(GameObject.Find("SplashScreen"));
        if (Network.isServer)
        {
            InstantiateGameBorders();
            InstantiateCubes();
        }
        var game = new Game(Resources.Load<Transform>("Player"), Resources.Load<Transform>("Lines"));
        game.StartGame();
    }
}
