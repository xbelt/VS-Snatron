using UnityEngine;
using System.Collections;

public class Drive : MonoBehaviour {
	public Transform wallTemplate;
	WallBehaviour latestWall;

	static Vector3 MinSpeed = Vector3.forward * 25;
	static Vector3 MaxSpeed = Vector3.forward * 45;
	
	Vector3 Speed = MinSpeed;

	int NumberOfWallsNear = 0;

	int HeightPixels;
	int WidthPixels;
	// Use this for initialization

	Vector3 WallSpawn {
		get {
			return transform.position;
		}
	}

	void AdjustSpeed ()
	{
		if (NumberOfWallsNear > 2) {
			Speed = Vector3.MoveTowards (Speed, MaxSpeed, 30 * Time.deltaTime);
		} else {
			Speed = Vector3.MoveTowards(Speed, MinSpeed, 30 * Time.deltaTime);
		}
	}

	void Start () {
		NewWall();
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
		AdjustSpeed ();
		transform.Translate (Speed * Time.deltaTime);
		if (latestWall != null) {
			latestWall.GetComponent<WallBehaviour> ().updateWall(WallSpawn);
		}
		
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
        NewWall();
        transform.Rotate(Vector3.up, 270);
    }

    void TurnRight()
    {
        NewWall();
        transform.Rotate(Vector3.up, 90);
    }

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "wall") {
			print (other.name + " " + name);
				NumberOfWallsNear++;
				print ("Walls near: " + NumberOfWallsNear);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "wall") {
			NumberOfWallsNear--;
			print ("Walls near: " + NumberOfWallsNear);
		}
	}

	void NewWall() {
		latestWall = (Instantiate (wallTemplate, WallSpawn, Quaternion.identity) as Transform).GetComponent<WallBehaviour> ();
		latestWall.GetComponent<WallBehaviour> ().start = WallSpawn;
		latestWall.GetComponent<WallBehaviour> ().end = WallSpawn;
		latestWall.GetComponent<WallBehaviour> ().updateWall (WallSpawn);

	}
}
