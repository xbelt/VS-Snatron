/*TODO:
 * make Padding of TextField dependant of screen resolution
 * Correctly send userID's
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Assets;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
public class GUI_Control : MonoBehaviour
{
   	private string _playerName = "Player";

	private NetworkControl networkControl;

    public Transform tron;
    public Transform grid;
    public GUIStyle buttonGUIStyle;
    public GUIStyle labelGUIStyle;
    public GUIStyle layoutGUIStyle;
    public GUIStyle textFieldGUIStyle;
    public GUIStyle horizontalScrollbarGUIStyle;

	private GameObject splashScreenLight;
	private GameObject splashScreen;

    private Vector2 _scrollPosition = Vector2.zero;
   
	public enum State { StartScreen, Lobby, Game, GamePaused }

	private State state = State.StartScreen;

    private int WidthPixels { get; set; }
    private int HeightPixels { get; set; }


    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        ChangeWifiSettingsAndroid();
        ReadScreenDimensionsAndroid();
        SetFontSize(HeightPixels / 50);
        SetTextColor(Color.white);
		networkControl = GameObject.Find("Network").GetComponent<NetworkControl> ();		networkControl.OnGameEnded += StopGame;
		networkControl.OnGameStarted += StartGame;
        NetworkControl.StartListeningForNewServers();
		
		splashScreenLight = GameObject.Find ("SplashScreenLight");
		splashScreen = GameObject.Find ("SplashScreen");
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
    }

	#region state transitions
	
	private void StartServer()
	{
		state = State.Lobby;
		NetworkControl.AnnounceServer ();
	}
	
	private void joinGame(String ipAddress, int port){
		state = State.Lobby;
		NetworkControl.Connect(ipAddress, port);
		NetworkControl.StopSearching();
		NetworkControl.StopAnnouncingServer();
	}
	
	private void StartNetworkGame()
	{
		StartGame ();
		GameObject.Find ("Network").networkView.RPC ("StartGame", RPCMode.All);
	}
	
	private void StartQuickGame()
	{
		StartGame ();
		Network.InitializeServer(1, Protocol.GamePort, false);
		GameObject.Find("Network").networkView.RPC("StartGame", RPCMode.All); //TODO avoiding RPCMode.all
		//this is so I don't have to bother with another, static, "StartGame" method
	}

	private void StartGame()
	{
		hideMenuBackground ();
		state = State.Game;
		NetworkControl.StopSearching();
		NetworkControl.StopAnnouncingServer();
	}
	
	public void StopGame()
	{
		// TODO make sure all players are disconnected
		// and they all show the main menu
		
		GameObject.Find("Network").networkView.RPC("StopGame", RPCMode.All); //TODO avoiding RPCMode.all
		state = State.StartScreen;
	}
	
	public void PauseGame()
	{
		// TODO
		state = State.GamePaused;
	}
	
	public void ResumeGame()
	{
		// TODO
		state = State.Game;
	}
	
	public void ExitApp()
	{
		Application.Quit ();
	}

	
	#endregion state transitions
	
	void OnConnectedToServer()
	{
		print("Connected");
		//update server list?
	}
		
	// ReSharper disable once UnusedMember.Local
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch(state){
			case State.Lobby: break;
			case State.Game: PauseGame(); break;
			case State.StartScreen: ExitApp(); break;
			case State.GamePaused: ResumeGame(); break;
			default: StopGame(); break;
			}
		}
	}
	
	#region GUI
	
	// ReSharper disable once UnusedMember.Local
    private void OnGUI()
    {
		switch (state) {
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
			joinGame(item.Ip.ToString(), Protocol.GamePort);
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
                NetworkControl.StopAnnouncingServer();
                StartNetworkGame();
            }
        }

    }

	private void HandleGame(){
		// TODO Draw Player info :
		// * who's still alive?
		// * "YOU WERE KILLED (BY ...?)"
		// * YOU HAVE WON
		// * PAUSE BUTTON?
	}

	private void HandleGamePaused()
	{
        if (GUI.Button(new Rect(9 / 20f * WidthPixels, 19 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
                "Exit", buttonGUIStyle))
        {
            Application.Quit();
        }
		// TODO Add menu for
		// * settings?
		// * Stop game
		// * ...
	}

	private void hideMenuBackground()
	{
		splashScreenLight.SetActive (false);
		splashScreen.SetActive (false);
		splashScreen.renderer.enabled = false;
	}

	private void showMenuBackground()
	{
		splashScreenLight.SetActive (true);
		splashScreen.SetActive (true);
		splashScreen.renderer.enabled = true;
	}
	
    #endregion
}