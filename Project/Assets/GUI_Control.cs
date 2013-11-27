using UnityEngine;
using System.Collections;

public class GUI_Control : MonoBehaviour {

    public bool gameStarted = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private void OnGUI() {
	    if (!gameStarted) {
	        if (GUI.Button(new Rect(25, 75, 100, 30), "START!!")) {
	            gameStarted = true;
	        }
	    }
	}

}
