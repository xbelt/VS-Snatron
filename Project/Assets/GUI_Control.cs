using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Assets;
using UnityEngine;
using System.Collections;

public class GUI_Control : MonoBehaviour {
    public Boolean isDiscovering = true;
    private List<String> servers = new List<String>();

    private IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Broadcast, Protocol.serverPort);
    UdpClient udpClient = new UdpClient();

    Vector2 scrollPosition = Vector2.zero;
    // Use this for initialization
	void Start () {
        var sendBytes4 = Encoding.ASCII.GetBytes(Protocol.Discover);
        udpClient.Send(sendBytes4, sendBytes4.Length, ipEndPoint);

        (new Thread(() =>
        {
            while (isDiscovering)
            {
                var endPoint = new IPEndPoint(IPAddress.Any, Protocol.serverPort);
                var result = udpClient.Receive(ref endPoint);
                var message = Encoding.ASCII.GetString(result);
                servers.Add(message);
            }
        })).Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private void OnGUI() {
        if (GUI.Button(new Rect(25, 25, 100, 30), "Host"))
        {
        }
        if (GUI.Button(new Rect(25, 75, 100, 30), "Race")) {
            isDiscovering = false;
            udpClient.Close();
            Application.LoadLevel(1);
        }
        
        GUILayout.BeginArea(new Rect(150f, 25f, 300f, 200f), GUI.skin.window);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        GUILayout.BeginVertical(GUI.skin.box);

        foreach (string item in servers)
        {
            if (GUILayout.Button(item, GUI.skin.box, GUILayout.ExpandWidth(true))) {
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

