using UnityEngine;
using System.Collections;

public class GUI_Control : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private void OnGUI() {
        if (GUI.Button(new Rect(25, 25, 100, 30), "Host"))
        {
        }
        if (GUI.Button(new Rect(25, 75, 100, 30), "Join"))
        {
        }
        if (GUI.Button(new Rect(25, 125, 100, 30), "Race"))
        {
            Application.LoadLevel(1);
        }
	}

}

