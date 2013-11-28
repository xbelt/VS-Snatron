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

    private GameConfiguration config;

    Vector2 _scrollPosition = Vector2.zero;

    private int WidthPixels { get; set; }
    private int HeightPixels { get; set; }


// ReSharper disable once UnusedMember.Local
    private void Start()
    {
        config = GameObject.FindGameObjectWithTag("gameConfig").GetComponent<GameConfiguration>();
        ReadScreenDimensionsAndroid();
        StartDiscoverServerThread();
    }

    private void ReadScreenDimensionsAndroid()
    {
        try
        {
            using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (new AndroidJavaClass("android.util.DisplayMetrics"))
                {
                    using (
                        AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                            activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
                            windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                            displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                        )
                    {
                        displayInstance.Call("getMetrics", metricsInstance);
                        HeightPixels = metricsInstance.Get<int>("heightPixels");
                        WidthPixels = metricsInstance.Get<int>("widthPixels");
                    }
                }
            }
        }
        catch (Exception)
        {
            HeightPixels = 600;
            WidthPixels = 800;
        }
    }

    private void StartDiscoverServerThread()
    {
        (new Thread(() =>
        {
            while (_isSearching)
            {
                var newServer = ServerDiscoverer.DiscoverServers();
                var addServer = true;
                foreach (var server in _servers.Where(server => server.Ip.Equals(newServer.Ip)))
                {
                    addServer = false;
                }
                if (addServer && newServer != null && newServer.Name != null)
                {
                    _servers.Add(newServer);
                }
            }
        })
            ).Start();
    }


// ReSharper disable once UnusedMember.Local
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Escape))
	    {
	        _hostServerGui = false;
	    }
	}

    #region GUI
    // ReSharper disable once UnusedMember.Local
	private void OnGUI() {
	    if (_hostServerGui)
	    {
	        HandleHostingGUI();
	    }
        else if (_waitingScreenOn)
        {
            HandleWaitingScreen();
        }
	    else
	    {
	        HandleStartScreenGUI();
	    }
	}

    private void HandleWaitingScreen()
    {
        if (GUI.Button(new Rect(25, 75, 100, 30), "Race"))
        {
            _isSearching = false;
            ServerHoster.IsHosting = false;
            Application.LoadLevel(1);
        }
    }

    private void HandleStartScreenGUI()
    {
        if (GUI.Button(new Rect(25, 25, 100, 30), "Host"))
        {
            _hostServerGui = true;
        }
        if (GUI.Button(new Rect(25, 75, 100, 30), "Race"))
        {
            _isSearching = false;
            ServerHoster.IsHosting = false;
            Application.LoadLevel(1);
        }

        GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), GUI.skin.window);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(GUI.skin.box);

        foreach (var item in _servers)
        {
            if (GUILayout.Button(item.Ip + " " + item.Name, GUI.skin.box, GUILayout.ExpandWidth(true)))
            {
                _isSearching = false;
                ServerHoster.IsHosting = false;
                Application.LoadLevel(1);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void HandleHostingGUI()
    {
        if (GUI.Button(new Rect(25, 25, 100, 30), "Play"))
        {
            _waitingScreenOn = true;
            _hostServerGui = false;
            ServerHoster.HostServer(config.HostName);
        }
        GUI.Label(new Rect(25, 75, 100, 30), "HostName");
        config.HostName = GUI.TextField(new Rect(150, 75, 100, 30), config.HostName, 20);
        GUI.Label(new Rect(25, 125, 100, 30), "# Players");
        config.NumberOfPlayers =
            Convert.ToInt32(Regex.Replace(
                GUI.TextField(new Rect(150, 125, 100, 30), config.NumberOfPlayers.ToString()), "[^.0-9]", ""));
    }
    #endregion
}
