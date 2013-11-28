using System;
using System.Security.Policy;
using UnityEngine;
using System.Collections;

public class GameConfiguration : MonoBehaviour
{

    public int NumberOfPlayers;
    public String HostName;

	// Use this for initialization
	void Start () {
	    DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
