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
    public GUIStyle buttonGUIStyle;
    public GUIStyle labelGUIStyle;
    public GUIStyle layoutGUIStyle;
    public GUIStyle textFieldGUIStyle;

    private Vector2 _scrollPosition = Vector2.zero;
    private bool drawGUI = true;
    private int FieldBorderCoordinates = 200;

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
#if UNITY_Android
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
#else
        HeightPixels = Screen.height;
        WidthPixels = Screen.width;
#endif
        SetFontSize(HeightPixels / 50);
        SetTextColor(Color.white);
    }

    private void SetTextColor(Color color)
    {
        buttonGUIStyle.normal.textColor = color;
        labelGUIStyle.normal.textColor = color;
        layoutGUIStyle.normal.textColor = color;
        textFieldGUIStyle.normal.textColor = color;
    }

    private void SetFontSize(int size)
    {
        buttonGUIStyle.fontSize = size < 12 ? 12 : size;
        labelGUIStyle.fontSize = size < 12 ? 12 : size;
        layoutGUIStyle.fontSize = size < 12 ? 12 : size;
        textFieldGUIStyle.fontSize = size < 12 ? 12 : size;
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
        else if (drawGUI){
            HandleStartScreenGUI();
        }
    }

    private void HandleStartScreenGUI() {
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Host", buttonGUIStyle))
        {
            _hostServerGui = true;
        }
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Race", buttonGUIStyle))
        {
            StartQuickGame();
        }

        GUI.Label(new Rect(17 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Player Name:", labelGUIStyle);
        _playerName = GUI.TextField(new Rect(21 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), _playerName);
        GUILayout.BeginArea(new Rect(5 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
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
    }

    private void HandleHostingGUI() {
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Play", buttonGUIStyle))
        {
            _waitingScreenOn = true;
            _hostServerGui = false;
            _isSearching = false;
            ServerHoster.HostServer(config.HostName);
            Network.InitializeServer(config.NumberOfPlayers, Protocol.GamePort, false);
            Network.sendRate = 15;
        }

        GUI.Label(new Rect(1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "HostName", labelGUIStyle);
        config.HostName = GUI.TextField(new Rect(5 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), config.HostName, 20, textFieldGUIStyle);
        GUI.Label(new Rect(1 / 30f * WidthPixels, 5 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "# of opponents", labelGUIStyle);
        Int32.TryParse(Regex.Replace(
            GUI.TextField(new Rect(5 / 30f * WidthPixels, 5 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), config.NumberOfPlayers.ToString(), textFieldGUIStyle), "[^.0-9]", ""),
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
        //if(Network.isClient)
        if (Network.isServer && Network.connections.Length > 0) {

            StartNetworkGame();
            ServerHoster.IsHosting = false;
            _hostServerGui = false;
            _waitingScreenOn = false;
            drawGUI = false;
            InstantiateGameBorders();
            
        }
        /*if (Network.maxConnections == Network.connections.Length) {
            StartNetworkGame();
        }*/

        if (GUI.Button(new Rect(25, 75, 100, 30), "Race")) {
            StartQuickGame();
        }
    }

    void OnConnectedToServer() {
        print("Connected");
        StartNetworkGame();
    }

    [RPC]
    private void StartNetworkGame() {
        _isSearching = false;
        ServerHoster.IsHosting = false;
        Debug.Log("Before passing: " + (tron == null));
        var game = new Game(Network.maxConnections + 1, Resources.Load<Transform>("Player"), Resources.Load<Transform>("Lines"));
        Destroy(gameObject);
        game.StartGame();
    }

    private void StartQuickGame() {
        _isSearching = false;
        ServerHoster.IsHosting = false;
        Network.InitializeServer(1, Protocol.GamePort, false);
        var game = new Game(1, Resources.Load<Transform>("Player"), Resources.Load<Transform>("Lines"));
        InstantiateGameBorders();
        Destroy(gameObject);
        game.StartGame();
    }

    private void InstantiateGameBorders()
    {
        var leftWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var frontWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var rightWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var backWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        leftWall .transform.localScale = new Vector3(1, 5, 1);
        leftWall .GetComponent<WallBehaviour>().start =     new Vector3(-FieldBorderCoordinates, 2.5f, -FieldBorderCoordinates);
        leftWall .GetComponent<WallBehaviour>().updateWall( new Vector3( FieldBorderCoordinates, 2.5f, -FieldBorderCoordinates));
        frontWall .transform.localScale = new Vector3(1, 5, 1);
        frontWall.GetComponent<WallBehaviour>().start =     new Vector3( FieldBorderCoordinates, 2.5f, -FieldBorderCoordinates);
        frontWall.GetComponent<WallBehaviour>().updateWall( new Vector3( FieldBorderCoordinates, 2.5f,  FieldBorderCoordinates));
        rightWall .transform.localScale = new Vector3(1, 5, 1);
        rightWall.GetComponent<WallBehaviour>().start =     new Vector3( FieldBorderCoordinates, 2.5f,  FieldBorderCoordinates);
        rightWall.GetComponent<WallBehaviour>().updateWall( new Vector3(-FieldBorderCoordinates, 2.5f,  FieldBorderCoordinates));
        backWall .transform.localScale = new Vector3(1, 5, 1);
        backWall .GetComponent<WallBehaviour>().start =     new Vector3(-FieldBorderCoordinates, 2.5f,  FieldBorderCoordinates);
        backWall .GetComponent<WallBehaviour>().updateWall( new Vector3(-FieldBorderCoordinates, 2.5f, -FieldBorderCoordinates));
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
            Debug.Log((_playerPrefab == null) + " playerPrefab");
            var player = Network.Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity, 0) as Transform;
            var cam = GameObject.Find("Main Camera");
            cam.AddComponent<SmoothFollow>().target = player;

            Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
            Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
        }

        public void StartGame() {
            SpawnPlayer();
        }
    }
}