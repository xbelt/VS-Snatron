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
    private bool _hostServerGui;
    private bool _waitingScreenOn;

    private string _currentIp = "0.0.0.0";
    private string _playerName = "Player";

    public Transform tron;
    public Transform grid;
    public GUIStyle buttonGUIStyle;
    public GUIStyle labelGUIStyle;
    public GUIStyle layoutGUIStyle;
    public GUIStyle textFieldGUIStyle;
    public GUIStyle horizontalScrollbarGUIStyle;

    private Vector2 _scrollPosition = Vector2.zero;
    private bool drawGUI = true;

    private int WidthPixels { get; set; }
    private int HeightPixels { get; set; }


    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        ChangeWifiSettingsAndroid();
        ReadScreenDimensionsAndroid();
        SetFontSize(HeightPixels / 50);
        SetTextColor(Color.white);
        NetworkControl.StartListeningForNewServers();
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


    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _hostServerGui = false;
        }
    }

    #region GUI

    // ReSharper disable once UnusedMember.Local
    private void OnGUI()
    {
        if (_hostServerGui)
        {
            HandleHostingGUI();
        }
        else if (_waitingScreenOn)
        {
            HandleWaitingScreen();
        }
        else if (drawGUI)
        {
            HandleStartScreenGUI();
        }
    }

    private void HandleStartScreenGUI()
    {
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Host", buttonGUIStyle))
        {
            _hostServerGui = true;
        }
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Race", buttonGUIStyle))
        {
            StartQuickGame();
        }

        GUI.Label(new Rect(17 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "Player Name:", labelGUIStyle);
        _playerName = GUI.TextField(new Rect(21 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), _playerName, textFieldGUIStyle);
        GUILayout.BeginArea(new Rect(5 / 30f * WidthPixels, 1 / 20f * HeightPixels, 11 / 30f * WidthPixels, 18 / 20f * HeightPixels), layoutGUIStyle);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(horizontalScrollbarGUIStyle);

        foreach (var item in NetworkControl.Servers.Where(item => GUILayout.Button(item.Ip + " " + item.Name, buttonGUIStyle, GUILayout.ExpandWidth(true))))
        {
            //TODO: test new LINQ expression
            NetworkControl.Connect(item.Ip.ToString(), Protocol.GamePort);
            NetworkControl.StopSearching();
            NetworkControl.StopAnnouncingServer();
            _waitingScreenOn = true;
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void HandleHostingGUI()
    {
        if (GUI.Button(new Rect(1 / 30f * WidthPixels, 1 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
            "Play", buttonGUIStyle))
        {
            _waitingScreenOn = true;
            _hostServerGui = false;
            NetworkControl.AnnounceServer();
            NetworkControl.IsSearching = false;
            NetworkControl.PlayerID = 0;
            NetworkControl.AddPlayer(_playerName);
        }

        GUI.Label(new Rect(1 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels), "HostName",
            labelGUIStyle);
        NetworkControl.HostName =
            GUI.TextField(new Rect(5 / 30f * WidthPixels, 3 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
                NetworkControl.HostName, 20, textFieldGUIStyle);
        GUI.Label(new Rect(1 / 30f * WidthPixels, 5 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
            "# of opponents", labelGUIStyle);
        Int32.TryParse(Regex.Replace(
            GUI.TextField(new Rect(5 / 30f * WidthPixels, 5 / 20f * HeightPixels, 1 / 10f * WidthPixels, 1 / 20f * HeightPixels),
                NetworkControl.NumberOfPlayers.ToString(), textFieldGUIStyle), "[^.0-9]", ""),
            out NetworkControl.NumberOfPlayers);
    }



    private void HandleWaitingScreen()
    {
        if (Network.isServer)
        {
            GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), layoutGUIStyle);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            GUILayout.BeginVertical(layoutGUIStyle);

            foreach (var item in Network.connections)
            {
                GUILayout.Label(item.ipAddress + " " + item.port, labelGUIStyle, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        if (Network.isServer && Network.connections.Length > 0)
        {

            StartNetworkGame();
            NetworkControl.StopAnnouncingServer();
            _hostServerGui = false;
            _waitingScreenOn = false;
            drawGUI = false;
            NetworkControl.InstantiateGameBorders();
            NetworkControl.InstantiateCubes();

        }
        /*if (Network.maxConnections == Network.connections.Length) {
        StartNetworkGame();
        ServerHoster.IsHosting = false;
        _hostServerGui = false;
        _waitingScreenOn = false;
        drawGUI = false;
        InstantiateGameBorders();
    }*/

        if (GUI.Button(new Rect(25, 75, 100, 30), "Race", buttonGUIStyle))
        {
            StartQuickGame();
        }
    }

    void OnConnectedToServer()
    {
        print("Connected");
        StartNetworkGame();
    }
    private void StartNetworkGame()
    {
        NetworkControl.StopSearching();
        NetworkControl.StopAnnouncingServer();
        NetworkControl.StartGame();
        Destroy(gameObject);
    }

    private void StartQuickGame()
    {
        NetworkControl.StopSearching();
        NetworkControl.StopAnnouncingServer();
        Network.InitializeServer(1, Protocol.GamePort, false);
        NetworkControl.InstantiateGameBorders();
        NetworkControl.InstantiateCubes();
        Destroy(GameObject.Find("SplashScreenLight"));
        Destroy(gameObject);
        NetworkControl.StartGame();
    }

    #endregion
}