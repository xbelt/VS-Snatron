using System;
using System.Linq;
using Assets;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
public class GUI_Control : MonoBehaviour
{
   	private string _playerName = "Player";
    private string _serverIP = "0.0.0.0";

	private NetworkControl _networkControl;

    public Transform tron;
    public Transform grid;
    public GUIStyle buttonGUIStyle;
    public GUIStyle labelGUIStyle;
    public GUIStyle layoutGUIStyle;
    public GUIStyle textFieldGUIStyle;
    public GUIStyle horizontalScrollbarGUIStyle;

	private GameObject _splashScreenLight;
	private GameObject _splashScreen;
	private GameObject _player;

    private Vector2 _scrollPosition = Vector2.zero;

    public enum State { StartScreen, Lobby, Game, GamePaused }

    private State _state = State.StartScreen;

    private int WidthPixels { get; set; }
    private int HeightPixels { get; set; }


    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        ChangeWifiSettingsAndroid();
        ReadScreenDimensionsAndroid();
        SetFontSize(HeightPixels / 50);
        SetTextColor(Color.white);

		_splashScreenLight = GameObject.Find ("SplashScreenLight");
		_splashScreen = GameObject.Find ("SplashScreen");
		
		initNetworkControl ();
    }

    private void ChangeWifiSettingsAndroid() { }

    private void ReadScreenDimensionsAndroid()
    {
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
        textFieldGUIStyle.padding.top = ((int)(1/20f*HeightPixels) - buttonGUIStyle.fontSize)/2;
        textFieldGUIStyle.padding.left = 6;
    }

	#region Network Control event listeners<
	
	private void initNetworkControl()
	{
		_networkControl = GameObject.Find("Network").GetComponent<NetworkControl> ();
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

	private void OnServerStarted()
	{
		Debug.Log ("GUI:OnServerStarted");
		Game.Instance.setPlayer(0, NetworkControl.PlayerName);
		_state = State.Lobby;
	}

	private void OnConnectedToRemoteServer(int id)
	{
		Debug.Log ("GUI:OnConnectedToRemoteServer");
		_state = State.Lobby;
		// id:That's us!
		// needed only when game starts for now
	}
	
	private void OnConnectionError(String message)
	{
		Debug.Log ("GUI:OnConnectionError: " + message);
		Game.Instance.NewGame ();
		_networkControl.Disconnect (); // TODO is it a problem when we are not connected?
		_state = State.StartScreen;
		// TODO show some error message
	}
	
	// This is indirectly called through RPC StartGame event
	private void OnGameStarted(int rounds)
	{
		print ("GUI:OnGameStarted()");
		HideMenuBackground ();
		_state = State.Game;
		Game.Instance.StartGame (_networkControl.PlayerID, rounds);
	}
	
	private void OnGameEnded()
	{
		print ("GUI:OnGameEnded()");
		// TODO make sure all players are disconnected
		// and they all show the main menu
		
		GameObject.Find("Network").networkView.RPC("StopGame", RPCMode.All); //TODO avoiding RPCMode.all
		_state = State.StartScreen;
	}
	
	private void OnRoundStarted(int round) // TODO probably not necessary
	{
		Debug.Log ("GUI:OnRoundStarted");
		//Game.Instance.NewRound ();
	}
	
	private void OnRoundEnded(int round) // TODO probably not necessary
	{
		Debug.Log ("GUI:OnRoundEnded");
		//Game.Instance.NewRound ();
	}
	
	private void OnPlayerJoined(int id, String name)
	{
		Debug.Log ("GUI:OnPlayerJoined");
		Game.Instance.setPlayer (id, name);
	}
	
	private void OnPlayerLeft(int id)
	{
		Debug.Log ("GUI:OnPlayerLeft");
		Game.Instance.removePlayer (id);
	}
	
	private void OnPlayerKilled(int id)
	{
		Debug.Log ("GUI:OnPlayerKilled");
		Game.Instance.playerDied (id);
	}

	#endregion

	#region state transitions
	
	private void StartServer()
	{
		print ("GUI,Server:StartServer()");
		//_state = State.Lobby;
		_networkControl.AnnounceServer ();
	}
	
	private void JoinGame(String ipAddress, int port){
		print ("GUI,Client:StartNetworkGame()");
		//_state = State.Lobby;
		_networkControl.JoinGame(ipAddress, port);
	}

	// Called by server host
	private void StartNetworkGame()
	{
		print ("GUI,Server:StartNetworkGame()");
		_networkControl.StopAnnouncingServer();
		int rounds = 5; // TODO take from GameConfig
		_networkControl.broadCastStartGame (rounds);
		//GameObject.Find ("Network").networkView.RPC ("StartGame", RPCMode.All);
	}

	// A QuickGame is a normal "NetworkGame" with only the server playing, which is only 1 round
	private void StartQuickGame()
	{
		print ("GUI:StartQuickGame()");
		Game.Instance.setPlayer(0, _playerName);
		_networkControl.InitServer (0);
		_networkControl.broadCastStartGame (1);
		//GameObject.Find("Network").networkView.RPC("StartGame", RPCMode.All); //TODO avoiding RPCMode.all
	}

	// Disconnect from a running game
	private void LeaveGame()
	{
		_networkControl.Disconnect ();
		Game.Instance.StopGame ();
		ShowMenuBackground ();
		ResetCamera();
		_state = State.StartScreen;
	}

    private void ExitApp()
	{
		_networkControl.StopAnnouncingServer ();
		_networkControl.Disconnect ();
		Application.Quit ();
	}

    #endregion state transitions

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch(_state){
			case State.StartScreen: ExitApp(); break;
			case State.Game:
			case State.Lobby:
			default: LeaveGame(); break;
			}
		}
	}
// ReSharper restore UnusedMember.Local
	
	#region GUI
	
	// ReSharper disable once UnusedMember.Local
    private void OnGUI()
    {
		switch (_state) {
				case State.StartScreen:
						HandleStartScreenGUI ();
						break;
				case State.Lobby:
						HandleWaitingScreen ();
						break;
				case State.Game:
						HandleGame ();
						break;
				case State.GamePaused:
						HandleGamePaused ();
						break;
				}
    }

    private void HandleStartScreenGUI()
    {
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Host", buttonGUIStyle))
        {
			StartServer();
		}
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Race", buttonGUIStyle))
        {
            StartQuickGame();
        }

        GUI.Label(new Rect(17 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Player Name:", labelGUIStyle);
        _playerName = GUI.TextField(new Rect(21 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), _playerName, textFieldGUIStyle);

        GUI.Label(new Rect(17 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Server IP:", labelGUIStyle);
        _serverIP = GUI.TextField(new Rect(21 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), _serverIP, textFieldGUIStyle);

        if (GUI.Button(new Rect(17/30f*WidthPixels, 5/20f*HeightPixels, 3/10f*WidthPixels, 1/20f*HeightPixels), "Join",
            buttonGUIStyle)) {
            JoinGame(_serverIP, Protocol.GamePort);
        }
        
        NetworkControl.PlayerName = _playerName;

        GUILayout.BeginArea(new Rect(5 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(horizontalScrollbarGUIStyle);

        foreach (var item in _networkControl.Servers.Where(item => GUILayout.Button(item.Ip + " " + item.Name, buttonGUIStyle, GUILayout.ExpandWidth(true))))
        {
			JoinGame(item.Ip.ToString(), Protocol.GamePort);
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void HandleWaitingScreen()
    {
        GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), layoutGUIStyle);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(layoutGUIStyle);

		for (int id = 0; id < Game.MaxPlayers; id++) {
			var playerName = Game.Instance.getPlayerName(id);
    		// TODO fix ArgumentException
			GUILayout.Label(id + ": " + playerName, labelGUIStyle, GUILayout.ExpandWidth(true));
		}

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (Network.isServer)
		{
			if (GUI.Button(new Rect(25, 75, 100, 30), "Start", buttonGUIStyle))
            {
                StartNetworkGame();
            }
            GUI.Label(new Rect(25, 125, 100, 30), "IP: " + Network.player.ipAddress, labelGUIStyle);
        }



    }

	private void HandleGame() {
		Drive player = Game.Instance.LocalPlayer;
		bool isAlive = Game.Instance.isAlive (Game.Instance.PlayerID);
		bool hasWon = Game.Instance.hasWon (Game.Instance.PlayerID);

		if (!isAlive) {
			if (!hasWon)
			{
				GUI.Label(new Rect(9/20f*WidthPixels, 19/40f*HeightPixels, 1/10f*WidthPixels, 1/20f*HeightPixels),
				          "You are dead!", labelGUIStyle);
			}else
			{
				LeaveGame();
			}
		}

        var i = 0;

		for (int j = 0; j < Game.MaxPlayers; j++)
		{
			if (Game.Instance.isActivePlayer(j))
			{
				GUI.Label(new Rect(1 / 20f * WidthPixels,
				                   (1 + 3 * i) / 40f * HeightPixels,
				                   1 / 10f * WidthPixels,
				                   1 / 20f * HeightPixels),
				          Game.Instance.getPlayerName(j) + ": " 
				          	+ (Game.Instance.isAlive(j) ? "Alive" : "Dead"),
				          labelGUIStyle);
				i++;
			}
		}

		if (Game.Instance.HasLocalPlayerWon() )
	    {
	        GUI.Label(new Rect(9 / 20f * WidthPixels,
			                   17 / 40f * HeightPixels,
			                   1 / 10f * WidthPixels,
			                   1 / 20f * HeightPixels),
			          "You won!",
			          labelGUIStyle);
	        if (GUI.Button(new Rect(9/20f*WidthPixels,
			                        20/40f*HeightPixels,
			                        1/10f*WidthPixels,
			                        1/20f*HeightPixels),
			               "Back to menu",
			               buttonGUIStyle))
	        {
				// TODO restart game
	            LeaveGame();
	        }
	    }

	    if (player.isIndestructible)
	    {
            GUI.Label(new Rect(9 / 20f * WidthPixels, 19 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
                "Indestructible for " + Game.Instance.IndestructibleTimeLeft.ToString("0.0") + "s", labelGUIStyle);
	    }
		// TODO Draw Player info :
		// * who's still alive?
		// * "YOU WERE KILLED (BY ...?)"
		// * YOU HAVE WON
		// * PAUSE BUTTON? -> no since pause makes no sense in multiplayer
	}

	private void HandleGamePaused()
	{
        if (GUI.Button(new Rect(9 / 20f * WidthPixels, 17 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
                "Exit", buttonGUIStyle))
        {
            Application.Quit();
        }
        if (GUI.Button(new Rect(9 / 20f * WidthPixels, 20 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
            Network.isServer ? "End game" : "Leave game", buttonGUIStyle))
        {
            LeaveGame();
        }
	}

    private void HideMenuBackground()
	{
		_splashScreenLight.SetActive (false);
		_splashScreen.SetActive (false);
		_splashScreen.renderer.enabled = false;
	}

	private void ShowMenuBackground()
	{
		_splashScreenLight.SetActive (true);
		_splashScreen.SetActive (true);
		_splashScreen.renderer.enabled = true;
	}

	private void ResetCamera()
	{
		GameObject.Find("Main Camera").transform.position = new Vector3(-7.621216f, -3.097347f, -11.66232f);
		GameObject.Find("Main Camera").transform.rotation = Quaternion.identity;
	}
	
    #endregion
}