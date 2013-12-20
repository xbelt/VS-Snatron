using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Assets;

public class UserInterface : MonoBehaviour
{
	public GameObject _splashScreenLight;
	public GameObject _splashScreen;

	private string _playerName = "Player";
	private string _serverIP = "0.0.0.0";

	public GUIStyle buttonGUIStyle;
	public GUIStyle labelGUIStyle;
	public GUIStyle layoutGUIStyle;
	public GUIStyle textFieldGUIStyle;
	public GUIStyle horizontalScrollbarGUIStyle;
	
	private Vector2 _scrollPosition = Vector2.zero;

	private int WidthPixels { get; set; }
	private int HeightPixels { get; set; }

	public delegate void UserEvent();
	public delegate void JoinGameRequest(string serverIp);

	public UserEvent OnStartServerRequest;
	public UserEvent OnStartNetworkGameRequest;
	public UserEvent OnStartQuickGameRequest;
	public JoinGameRequest OnJoinGameRequest;
	public UserEvent OnLeaveGameRequest;
	public UserEvent OnCloseAppRequest;

	public delegate IEnumerable<Server> GetServerList();
	public delegate PlayerModel[] GetPlayerList();

	private GetServerList _serverSource;
	private GetPlayerList _playerSource;

	private int _round;

	public string PlayerName { get { return _playerName; } }
	public string ServerIp { get { return _serverIP; } }
	
	private enum State { Uninitialized, StartScreen, Lobby, InitGame, Game, BetweenRounds, Ranking, GamePaused }
	private State _state = State.Uninitialized;

	public void ShowStartScreen(GetServerList serverSource) {
		Debug.Log ("UI:Show Start Screen");
		_serverSource = serverSource;
		ShowMenuBackground ();
		ResetCamera();
		_state = State.StartScreen;
	}
	public void ShowLobby(GetPlayerList playerSource) {
		Debug.Log ("UI:Show Lobby");
		_playerSource = playerSource;
		_state = State.Lobby;
	}
	public void ShowInitGame() {
		Debug.Log ("UI:ShowInitGame");
		_round = 1;
		_state = State.InitGame;
	}
	public void ShowGame() {
		Debug.Log ("UI:Show Game");
		HideMenuBackground ();
		_state = State.Game;
	}
	public void ShowBetweenRounds(int round){
		Debug.Log ("UI:Show Between Rounds");
		_round = round;
		ShowMenuBackground ();
		ResetCamera();
		_state = State.BetweenRounds;
	}
	public void ShowRanking(GetPlayerList sortedPlayerSource){
		Debug.Log ("UI:Show Ranking");
		_playerSource = sortedPlayerSource;
		_state = State.Ranking;
	}
	public void ShowGamePaused() {
		Debug.Log ("UI:Show GamePaused");
		_state = State.GamePaused;
	}

	private void Start()
	{
		ReadScreenDimensionsAndroid();
		SetFontSize(HeightPixels / 25);
		SetTextColor(Color.white);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch(_state){
			case State.StartScreen: 	OnCloseAppRequest(); 	break;
			case State.Game: 			ShowGamePaused(); 		break;
			case State.GamePaused: 		ShowGame(); 			break;
			case State.Ranking: 		OnLeaveGameRequest(); 	break;
			case State.Lobby: 			OnLeaveGameRequest(); 	break;
			case State.InitGame:
			case State.Uninitialized:
			case State.BetweenRounds:
			default: 											break;
			}
		}
	}
	
	// ReSharper disable once UnusedMember.Local
	private void OnGUI()
	{
		switch (_state) {
		case State.StartScreen: 	HandleStartScreenGUI ();break;
		case State.Lobby:			HandleWaitingScreen ();	break;
		case State.InitGame:		HandleInitGame();		break;
		case State.Game:			HandleGame ();			break;
		case State.BetweenRounds:	HandleBetweenRounds();	break;
		case State.Ranking:			HandleRanking();		break;
		case State.GamePaused:		HandleGamePaused ();	break;
		case State.Uninitialized: 	HandleUninitialized();	break;
		default:											break;
		}
	}

	public void HandleUninitialized()
	{
		// TODO show loading ...
	}

	public void HandleStartScreenGUI()
	{
		drawHostButton ();
		DrawQuickGameButton ();

		DrawPlayerNameLabelAndInput ();
		DrawServerIpLabelAndInput ();
		DrawJoinServerButton ();
		NetworkInterface.PlayerName = _playerName; // TODO remove this shit static fuck players the died
		DrawServerList ();

	}
	
	public void HandleWaitingScreen()
	{
		DrawPlayerList ();
		
		if (Network.isServer) {
			DrawStartButton ();
			DrawIpAddress ();
		}
		
	}

	public void HandleInitGame()
	{
		DrawRoundNumber ();
	}

	public void HandleGame() {
		bool isAlive = Game.Instance.isAlive (Game.Instance.PlayerID);
		bool hasWon = Game.Instance.hasWon (Game.Instance.PlayerID);
		
		if (!isAlive && !hasWon) {
			DrawYouLoseMessage ();
		}
		
		DrawPlayerStateList ();
		
		if (Game.Instance.HasLocalPlayerWon ) {
			DrawYouWinMessage ();
			DrawBackButton ();
		}
	}

	public void HandleBetweenRounds()
	{
		DrawRoundNumber ();
	}
	
	public void HandleRanking()
	{
        if (GUI.Button(new Rect(14 / 20f * WidthPixels, 20 / 40f * HeightPixels,
                                1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
                       Network.isServer ? "End game" : "Leave game", buttonGUIStyle))
        {
            OnLeaveGameRequest();
        }
		DrawRanking ();
	}

	public void HandleGamePaused()
	{
		DrawExitButton ();
		DrawEndGameButton ();
	}

	#region GUI Elements

	void DrawRoundNumber ()
	{
		// Temp hack
		int old = labelGUIStyle.fontSize;
		labelGUIStyle.fontSize = 30;
		GUI.Label (new Rect (0.4f * WidthPixels, 0.1f * HeightPixels,
		                     0.2f * WidthPixels, 0.2f * HeightPixels),
		           "Round " + _round, labelGUIStyle);
		labelGUIStyle.fontSize = old;
	}

	void drawHostButton ()
	{
		if (GUI.Button (new Rect (1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Host", buttonGUIStyle)) {
			OnStartServerRequest ();
		}
	}

	void DrawQuickGameButton ()
	{
		if (GUI.Button (new Rect (1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Race", buttonGUIStyle)) {
			OnStartQuickGameRequest ();
		}

	}

	void DrawPlayerNameLabelAndInput ()
	{
		GUI.Label (new Rect (17 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Player Name:", labelGUIStyle);
		_playerName = GUI.TextField (new Rect (21 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), _playerName, textFieldGUIStyle);
	}

	void DrawServerIpLabelAndInput ()
	{
		GUI.Label (new Rect (17 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Server IP:", labelGUIStyle);
		_serverIP = GUI.TextField (new Rect (21 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), _serverIP, textFieldGUIStyle);
	}

	void DrawJoinServerButton ()
	{
		if (GUI.Button (new Rect (21 / 30f * WidthPixels, 5 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Join", buttonGUIStyle)) {
			OnJoinGameRequest (_serverIP);
		}
	}

	void DrawServerList ()
	{
		GUILayout.BeginArea (new Rect (5 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
		_scrollPosition = GUILayout.BeginScrollView (_scrollPosition, false, true);
		GUILayout.BeginVertical (horizontalScrollbarGUIStyle);
		foreach (var item in _serverSource ().Where (item => GUILayout.Button (item.Ip + " " + item.Name, buttonGUIStyle, GUILayout.ExpandWidth (true), GUILayout.Height (1 / 15f * HeightPixels)))) {
			OnJoinGameRequest (item.Ip.ToString ());
		}
		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	void DrawPlayerList ()
	{
		GUILayout.BeginArea (new Rect (10 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
		_scrollPosition = GUILayout.BeginScrollView (_scrollPosition, false, true);
		GUILayout.BeginVertical (layoutGUIStyle);
		PlayerModel[] players = _playerSource ();
		foreach (PlayerModel player in players) {
			if (player == null)
				continue;
			// TODO fix ArgumentException
			GUILayout.Label (player.id + ": " + player.name, labelGUIStyle, GUILayout.ExpandWidth (true), GUILayout.Height (1 / 15f * HeightPixels));
		}
		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	void DrawRanking ()
	{
		GUILayout.BeginArea (new Rect (10 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
		_scrollPosition = GUILayout.BeginScrollView (_scrollPosition, false, true);
		GUILayout.BeginVertical (layoutGUIStyle);
		PlayerModel[] players = _playerSource ();
		int i = 0;
		foreach (PlayerModel player in players) {
			i++;
			if (player == null)
				continue;

			GUILayout.Label (i +  ". " + player.name + ": " + player.score, labelGUIStyle, GUILayout.ExpandWidth (true), GUILayout.Height (1 / 15f * HeightPixels));
		}
		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	void DrawStartButton ()
	{
		if (GUI.Button (new Rect (1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Start", buttonGUIStyle)) {
			OnStartNetworkGameRequest ();
		}
	}

	void DrawIpAddress ()
	{
		GUI.Label (new Rect (1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "IP: " + Network.player.ipAddress, labelGUIStyle);
	}

	void DrawYouLoseMessage ()
	{
		GUI.Label (new Rect (9 / 20f * WidthPixels, 19 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "You lose!", labelGUIStyle);
	}

	void DrawPlayerStateList ()
	{
		var i = 0;
		for (int j = 0; j < Game.Instance.Level.MaxPlayers; j++) {
			if (Game.Instance.isActivePlayer (j)) {
				GUI.Label (new Rect (1 / 20f * WidthPixels, (1 + 3 * i) / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), Game.Instance.getPlayerName (j) + ": " + (Game.Instance.isAlive (j) ? "Alive" : "Dead"), labelGUIStyle);
				i++;
			}
		}
	}

	void DrawYouWinMessage ()
	{
		GUI.Label (new Rect (9 / 20f * WidthPixels, 17 / 40f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "You won!", labelGUIStyle);
	}

	void DrawBackButton ()
	{
		if (GUI.Button (new Rect (8 / 20f * WidthPixels, 20 / 40f * HeightPixels, 2 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Back to menu", buttonGUIStyle)) {
			// TODO restart game
			OnLeaveGameRequest ();
		}
	}

	private void DrawExitButton()
	{
		if (GUI.Button(new Rect(9 / 20f * WidthPixels, 17 / 40f * HeightPixels,
		                        1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
		               "Exit", buttonGUIStyle))
		{
			OnCloseAppRequest();
		}
	}

	private void DrawEndGameButton()
	{
		if (GUI.Button(new Rect(9 / 20f * WidthPixels, 20 / 40f * HeightPixels,
		                        1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
		               Network.isServer ? "End game" : "Leave game", buttonGUIStyle))
		{
			OnLeaveGameRequest();
		}
	}

	#endregion
	
	public void HideMenuBackground()
	{
		_splashScreenLight.SetActive (false);
		_splashScreen.SetActive (false);
        _splashScreen.guiTexture.enabled = false;
	}
	
	public void ShowMenuBackground()
	{
		_splashScreenLight.SetActive (true);
		_splashScreen.SetActive (true);
		_splashScreen.guiTexture.enabled = true;
	}
	
	public void ResetCamera()
	{
		GameObject.Find("Main Camera").transform.position = new Vector3(-7.621216f, -3.097347f, -11.66232f);
		GameObject.Find("Main Camera").transform.rotation = Quaternion.identity;
	}
	
	
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
}

