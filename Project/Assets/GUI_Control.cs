using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Assets;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
public class GUI_Control : MonoBehaviour {

    private readonly List<Server> _servers = new List<Server>();
    private bool _isSearching = true;
    private bool _hostServerGui;
    private bool _waitingScreenOn;

    private string _currentIp = "0.0.0.0";
    private string _playerName = "Player";
    private GameConfiguration config;

    public Transform tron;
    public Transform grid;

    private Vector2 _scrollPosition = Vector2.zero;

    private int WidthPixels { get; set; }
    private int HeightPixels { get; set; }


// ReSharper disable once UnusedMember.Local
    private void Start() {
        ChangeWifiSettingsAndroid();
        config = GameObject.FindGameObjectWithTag("gameConfig").GetComponent<GameConfiguration>();
        ReadScreenDimensionsAndroid();
        StartDiscoverServerThread();
    }

    private void ChangeWifiSettingsAndroid() {}

    private void ReadScreenDimensionsAndroid() {
        try {
            using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (new AndroidJavaClass("android.util.DisplayMetrics")) {
                    using (
                        AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                            activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
                            windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                            displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                        ) {
                        displayInstance.Call("getMetrics", metricsInstance);
                        HeightPixels = metricsInstance.Get<int>("heightPixels");
                        WidthPixels = metricsInstance.Get<int>("widthPixels");
                    }
                }
            }
        }
        catch (Exception) {
            HeightPixels = 600;
            WidthPixels = 800;
        }
    }

    private void StartDiscoverServerThread() {
        (new Thread(() => {
            while (_isSearching) {
                var newServer = ServerDiscoverer.DiscoverServers();
                Debug.Log("Discovered new Server");
                var addServer = true;
                foreach (var server in _servers.Where(server => server.Ip.Equals(newServer.Ip))) {
                    addServer = false;
                }
                if (addServer && newServer != null && newServer.Name != null) {
                    _servers.Add(newServer);
                }
            }
        })
            ).Start();
    }


// ReSharper disable once UnusedMember.Local
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            _hostServerGui = false;
        }
    }

    #region GUI

    // ReSharper disable once UnusedMember.Local
    private void OnGUI() {
        if (_hostServerGui) {
            HandleHostingGUI();
        }
        else if (_waitingScreenOn) {
            HandleWaitingScreen();
        }
        else {
            HandleStartScreenGUI();
        }
    }

    private void HandleStartScreenGUI() {
        if (GUI.Button(new Rect(25, 25, 100, 30), "Host")) {
            _hostServerGui = true;
        }
        if (GUI.Button(new Rect(25, 75, 100, 30), "Race")) {
            StartQuickGame();
        }

        GUI.Label(new Rect(475, 25, 100, 30), "Player Name:");
        _playerName = GUI.TextField(new Rect(600, 25, 100, 30), _playerName);

        GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), GUI.skin.window);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(GUI.skin.box);

        foreach (var item in _servers) {
            if (GUILayout.Button(item.Ip + " " + item.Name, GUI.skin.box, GUILayout.ExpandWidth(true))) {
                Network.Connect(item.Ip.ToString(), Protocol.GamePort);
                _isSearching = false;
                ServerHoster.IsHosting = false;
                _waitingScreenOn = true;
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        /*GUI.Label(new Rect(475, 75, 100, 30), "Server IP");
        _currentIp = GUI.TextField(new Rect(600, 75, 100, 30), _currentIp);
        if (GUI.Button(new Rect(725, 75, 100, 30), "Join")) {
            Network.Connect(_currentIp, Protocol.GamePort);
            _isSearching = false;
            ServerHoster.IsHosting = false;
            _waitingScreenOn = true;
        }*/
    }

    private void HandleHostingGUI() {
        if (GUI.Button(new Rect(25, 25, 100, 30), "Play")) {
            _waitingScreenOn = true;
            _hostServerGui = false;
            ServerHoster.HostServer(config.HostName);
            Network.InitializeServer(config.NumberOfPlayers, Protocol.GamePort, false);
        }

        GUI.Label(new Rect(25, 75, 100, 30), "HostName");
        config.HostName = GUI.TextField(new Rect(150, 75, 100, 30), config.HostName, 20);
        GUI.Label(new Rect(25, 125, 100, 30), "# of opponents");
        Int32.TryParse(Regex.Replace(
            GUI.TextField(new Rect(150, 125, 100, 30), config.NumberOfPlayers.ToString()), "[^.0-9]", ""),
            out config.NumberOfPlayers);
    }

    private void HandleWaitingScreen() {
        if (Network.isServer) {
            GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), GUI.skin.window);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            GUILayout.BeginVertical(GUI.skin.box);

            foreach (var item in Network.connections) {
                GUILayout.Label(item.ipAddress + " " + item.port, GUI.skin.box, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /*GUI.Label(new Rect(25, 175, 100, 30),
            _servers.Find(x => x.Name.Equals(config.HostName)) == null
                ? ""
                : _servers.Find(x => x.Name.Equals(config.HostName)).Ip.ToString());*/
        if (Network.maxConnections == Network.connections.Length) {
            StartNetworkGame();
        }

        if (GUI.Button(new Rect(25, 75, 100, 30), "Race")) {
            StartQuickGame();
        }
    }

    private void StartNetworkGame() {
        _isSearching = false;
        ServerHoster.IsHosting = false;
        var game = new Game(config.NumberOfPlayers, tron, grid);
        game.StartGame();
    }

    private void StartQuickGame() {
        _isSearching = false;
        ServerHoster.IsHosting = false;
        Network.InitializeServer(0, Protocol.GamePort, false);
        var game = new Game(1, tron, grid);
        game.StartGame();
    }


    #endregion


    public class Game {
        private readonly int _numberOfPlayers;
        private readonly Transform _playerPrefab;
        private readonly Transform _gridPrefab;

        public Game(int numberOfPlayers, Transform playerPrefab, Transform gridPrefab) {
            _numberOfPlayers = numberOfPlayers;
            _playerPrefab = playerPrefab;
            _gridPrefab = gridPrefab;
        }

        private void SpawnPlayer() {
            var player = Network.Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity, 0) as Transform;
            GameObject.Find("Main Camera").GetComponent<SmoothFollow>().target = player;
            Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
            Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
        }

        public void StartGame() {
            Destroy(GameObject.Find("SplashScreen"));
            SpawnPlayer();
        }
    }
}