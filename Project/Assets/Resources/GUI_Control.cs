using System;
using System.Linq;
using Assets;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
public class GUI_Control : MonoBehaviour
{
   	private string _playerName = "Player";

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

		// Init NetworkControl
		_networkControl = GameObject.Find("Network").GetComponent<NetworkControl> ();
		_networkControl.OnGameEnded += StopGame;
		_networkControl.OnGameStarted += StartGame;
		_networkControl.StartListeningForNewServers();

		_splashScreenLight = GameObject.Find ("SplashScreenLight");
		_splashScreen = GameObject.Find ("SplashScreen");
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

	#region state transitions
	
	private void StartServer()
	{
		_state = State.Lobby;
		_networkControl.AnnounceServer ();
	}
	
	private void JoinGame(String ipAddress, int port){
		_state = State.Lobby;
		_networkControl.Connect(ipAddress, port);
		_networkControl.StopSearching();
		_networkControl.StopAnnouncingServer();
	}
	
	private void StartNetworkGame()
	{
		GameObject.Find ("Network").networkView.RPC ("StartGame", RPCMode.All);
	}
	
	private void StartQuickGame()
	{
		Network.InitializeServer(0, Protocol.GamePort, false);
		GameObject.Find("Network").networkView.RPC("StartGame", RPCMode.All); //TODO avoiding RPCMode.all
        Game.Instance.setPlayer(0, _playerName);
		//this is so I don't have to bother with another, static, "StartGame" method
	}

	// This is indirectly called through RPC StartGame
	private void StartGame()
	{
		HideMenuBackground ();
		_state = State.Game;
		_networkControl.StopSearching();
		_networkControl.StopAnnouncingServer();
		
		// int id = Game.Instance.PlayerID;
		// TODO Game.Instance.get player To register event listener
	}

    private void StopGame()
	{
		// TODO make sure all players are disconnected
		// and they all show the main menu
		
		GameObject.Find("Network").networkView.RPC("StopGame", RPCMode.All); //TODO avoiding RPCMode.all
		_state = State.StartScreen;
	}

    private void PauseGame()
	{
		// TODO
		_state = State.GamePaused;
	}

    private void ResumeGame()
	{
		// TODO
		_state = State.Game;
	}

    private void ExitApp()
	{
		Application.Quit ();
	}

    private void LeaveGame()
    {
        Network.Disconnect();
        Game.Instance.StopGame();
        ShowMenuBackground();
        ResetCamerPositionRotation();
        _state = State.StartScreen;

    }
    #endregion state transitions

    private void ResetCamerPositionRotation()
    {
        GameObject.Find("Main Camera").transform.position = new Vector3(-7.621216f, -3.097347f, -11.66232f);
        GameObject.Find("Main Camera").transform.rotation = Quaternion.identity;
    }
	
// ReSharper disable UnusedMember.Local
	void OnConnectedToServer()
	{
		print("Connected");
		//update server list?
	}
		
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch(_state){
			case State.Lobby: break;
			case State.Game: PauseGame(); break;
			case State.StartScreen: ExitApp(); break;
			case State.GamePaused: ResumeGame(); break;
			default: StopGame(); break;
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
        NetworkControl.PlayerName = _playerName;

        GUILayout.BeginArea(new Rect(5 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(horizontalScrollbarGUIStyle);

        foreach (var item in NetworkControl.Servers.Where(item => GUILayout.Button(item.Ip + " " + item.Name, buttonGUIStyle, GUILayout.ExpandWidth(true))))
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
			if (playerName != null) {
				// TODO fix ArgumentException
				GUILayout.Label(id + ": " + playerName, labelGUIStyle, GUILayout.ExpandWidth(true));
			}
		}

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (Network.isServer)
		{
			Game.Instance.setPlayer(0, NetworkControl.PlayerName);
			if (GUI.Button(new Rect(25, 75, 100, 30), "Start", buttonGUIStyle))
            {
				_networkControl.StopAnnouncingServer();
                StartNetworkGame();
            }
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

		if (Game.Instance.countAlivePlayers() == 1 && Network.maxConnections > 0)
	    {
	        player.PlayerHasWon = true;
            GUI.Label(new Rect(9 / 20f * WidthPixels, 17 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "You won!", labelGUIStyle);
	        if (GUI.Button(new Rect(9/20f*WidthPixels, 20/40f*HeightPixels, 1/10f*WidthPixels, 1/20f*HeightPixels),
	            "Back to menu", buttonGUIStyle))
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
	
    #endregion
}