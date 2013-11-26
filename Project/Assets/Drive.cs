using UnityEngine;
using System.Collections;

public class Drive : MonoBehaviour {

	int HeightPixels;
	int WidthPixels;
	// Use this for initialization
	void Start () {
		using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
		    metricsClass = new AndroidJavaClass("android.util.DisplayMetrics")
		) {
            using(
    			AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
    			activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
    			windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
    			displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
			) {
				displayInstance.Call ("getMetrics", metricsInstance);
				HeightPixels = metricsInstance.Get<int> ("heightPixels");
				WidthPixels = metricsInstance.Get<int> ("widthPixels");
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * Time.deltaTime * 20);

		foreach(var touch in Input.touches) {
			if (touch.phase == TouchPhase.Began) {
				// Construct a ray from the current touch coordinates
				if (touch.position.x > WidthPixels/2) {
					TurnRight();
				}
				else {
					TurnLeft();
				}
			}
		}


        // Input for preview
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			TurnLeft();
		} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
			TurnRight();
		}
	}

    void TurnLeft()
    {
        transform.Rotate(Vector3.up, 270);
    }

    void TurnRight()
    {
        transform.Rotate(Vector3.up, 90);
    }
		}
	}

	void OnTriggerEnter(Collider other) {
		print("onTriggerEnter");
		transform.Rotate(Vector3.up, 180);
		//if (collider.gameObject.tag == "wall") {
				//Destroy (gameObject);
		//}
	}

	void onCollisionEnter(Collider other) {
		print("onCollisionEnter");
		transform.Rotate(Vector3.up, 180);
		//Destroy (gameObject);
		}
}
