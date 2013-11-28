using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Assets;
using UnityEngine;

public class GUI_Control : MonoBehaviour {
    private readonly List<Server> _servers = new List<Server>();
    private bool isSearching = true;

    Vector2 _scrollPosition = Vector2.zero;


    // Use this for initialization
	void Start () {
        (new Thread(() =>
	    {
	        while (isSearching)
	        {
                Debug.Log("First line");
	            var newServer = ServerDiscoverer.DiscoverServers();
                Debug.Log("Got new server");
	            bool addServer = true;
	            foreach (var server in _servers)
	            {
	                if (server.Name.Equals(newServer.Name))
	                {
	                    addServer = false;
	                }
	            }
	            if (addServer && newServer != null && newServer.Name != null)
	            {
	                _servers.Add(newServer);
	            }
	        }
	    })).Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private void OnGUI() {
	    if (GUI.Button(new Rect(25, 25, 100, 30), "Host"))
	    {
            //TODO: replace with chosen name
	        ServerHoster.HostServer("testName2");
	    }
        if (GUI.Button(new Rect(25, 75, 100, 30), "Race")) {
            isSearching = false;
            ServerHoster.IsHosting = false;
            Application.LoadLevel(1);
        }
        
        GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), GUI.skin.window);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
        GUILayout.BeginVertical(GUI.skin.box);

        foreach (var item in _servers)
        {
            if (GUILayout.Button(item.Name, GUI.skin.box, GUILayout.ExpandWidth(true)))
            {
                isSearching = false;
                ServerHoster.IsHosting = false;
                Application.LoadLevel(1);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
	}

}


internal class GameServer {
    private IPAddress _ipAddress;

    public GameServer(IPAddress address) {
        _ipAddress = address;
    }

}

