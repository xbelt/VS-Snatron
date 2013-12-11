using System.Runtime.InteropServices;
using UnityEngine;

public class Drive : MonoBehaviour {
	public Transform wallTemplate;
    private Transform latestWallGameObject;

	static Vector3 MinSpeed = Vector3.forward * 25;
	static Vector3 MaxSpeed = Vector3.forward * 45;
	
	Vector3 Speed = MinSpeed;

	int NumberOfWallsNear = 0;

	int HeightPixels;
	int WidthPixels;

    private GameConfiguration config;

    Vector3 Offset {
        get {
            return Vector3.forward * 5;
        }
    }
    Vector3 WallOffset {
        get {
            return transform.rotation * Offset;
        }
    }

	Vector3 CurrentWallEnd {
		get {
		    if (latestWallGameObject == null) return transform.position - WallOffset;
            if (Vector3.SqrMagnitude(latestWallGameObject.GetComponent<WallBehaviour>().end - latestWallGameObject.GetComponent<WallBehaviour>().start) > 20)
            {
		        return transform.position - WallOffset;
		    }
		    return transform.position;
		}
	}

	void AdjustSpeed ()
	{
		if (NumberOfWallsNear > 1) {
			Speed = Vector3.MoveTowards (Speed, MaxSpeed, 30 * Time.deltaTime);
		} else {
			Speed = Vector3.MoveTowards(Speed, MinSpeed, 30 * Time.deltaTime);
		}
	}

	void Start ()
	{
	    config = GameObject.FindGameObjectWithTag("gameConfig").GetComponent<GameConfiguration>();

		NewWall();
#if UNITY_Android
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
#else
        HeightPixels = 600;
        WidthPixels = 800;
#endif
	}
	
	// Update is called once per frame
	void Update () {
		AdjustSpeed ();
	    transform.Translate(Speed*Time.deltaTime);
	    if (latestWallGameObject != null) {
			latestWallGameObject.GetComponent<WallBehaviour> ().updateWall(CurrentWallEnd);
		}

        GameObject.Find("LightFront").GetComponent<Light>().transform.position = transform.position + Vector3.up;
        GameObject.Find("LightBack").GetComponent<Light>().transform.position = transform.position + Vector3.up - WallOffset;

		//Handling touch input
		foreach(var touch in Input.touches) {
			if (touch.phase == TouchPhase.Began) {
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
        latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        transform.Rotate(Vector3.up, 270);
        NewWall();
    }

    void TurnRight()
    {
        latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        transform.Rotate(Vector3.up, 90);
        
        NewWall();
    }

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "wall") {
			print (other.name + " " + name);
				NumberOfWallsNear++;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "wall") {
			NumberOfWallsNear--;
		}
	}

	void NewWall() {
	    latestWallGameObject = (Transform) Network.Instantiate(wallTemplate, CurrentWallEnd, Quaternion.identity, 0);
		latestWallGameObject.GetComponent<WallBehaviour> ().start = CurrentWallEnd;
		latestWallGameObject.GetComponent<WallBehaviour> ().end = CurrentWallEnd;
		latestWallGameObject.GetComponent<WallBehaviour> ().updateWall (CurrentWallEnd);
	}
}