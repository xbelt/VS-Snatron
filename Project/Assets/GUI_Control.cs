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
		GUI.Label (new Rect (10, 10, 100, 40), "welcome to Snatron");
		GUI.Button (new Rect (25, 75, 100, 30), "START!!");
	}

}
