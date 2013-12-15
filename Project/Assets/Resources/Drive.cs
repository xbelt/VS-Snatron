using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Drive : MonoBehaviour {
	//public Transform wallTemplate;
    private Transform _latestWallGameObject;

	static readonly float MinSpeed = 25;
	static readonly float MaxSpeed = 50;

	public float accelerationRate = 10;
	public float decelerationRate = 5;
	
	float _speed = MinSpeed;

	int _numberOfWallsNear = 0;

    private int HeightPixels;
	private int WidthPixels;

    private bool _showPauseMenu;
    private GUIStyle buttonStyle = new GUIStyle();

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
		    if (_latestWallGameObject == null) return transform.position - WallOffset;
            if (Vector3.SqrMagnitude(_latestWallGameObject.GetComponent<WallBehaviour>().end - _latestWallGameObject.GetComponent<WallBehaviour>().start) > 20)
            {
		        return transform.position - WallOffset;
		    }
		    return transform.position;
		}
	}

// ReSharper disable once UnusedMember.Local
	void Start () {
	    buttonStyle.normal.background = Resources.Load<Texture2D>("TextBox");
	    buttonStyle.normal.textColor = Color.white;
	    var size = HeightPixels/50;
        buttonStyle.fontSize = size < 12 ? 12 : size;
        buttonStyle.alignment = TextAnchor.MiddleCenter;

		if (GetComponent<NetworkView>().isMine) {
			NewWall();
		}

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
        HeightPixels = Screen.height;
        WidthPixels = Screen.width;
#endif
	}
	
// ReSharper disable once UnusedMember.Local
	void Update () {
	    if (GetComponent<NetworkView>().isMine)
	    {
			// Note: For the collision detection to work well,
			// it is essential that the tron rests at the same place
			// when it is turning. Otherwise the engine has no time to
			// predict the collision. Therefore we return if there was some user input
	        bool applied = ApplyUserCommands ();
			if (applied)
				return;
	    }

		// The tron would keep moving straight
		// But are there any obstacles in front?
		if (_predictedCollisions > 0) {
			kill();
			return;
		}

		// move forward
		transform.Translate(Vector3.forward*_speed*Time.deltaTime);
		if (_latestWallGameObject != null) {
			_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall(CurrentWallEnd);
		}

		// Adjust the speed last. Implies resizing collider for collision prediction.
		// want to resize the collider before control is returned to unity for collision detection etc.
		// Adjusting the collider within this method, earlier, would not immediately trigger new collisions etc.
		AdjustSpeed ();
	}

	bool ApplyUserCommands ()
	{
		//Handling touch input
		foreach (var touch in Input.touches.Where (touch => touch.phase == TouchPhase.Began)) {
			if (touch.position.x > WidthPixels / 2) {
				TurnRight ();
				return true;
			}
			TurnLeft ();
			return true;
		}
		// Input for preview
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			TurnLeft ();
			return true;
		}
	    if (!Input.GetKeyDown(KeyCode.RightArrow)) return false;
	    TurnRight ();
	    return true;
	}

	void AdjustSpeed ()
	{
		if (_numberOfWallsNear > 0) {
			_speed = Mathf.MoveTowards (_speed, MaxSpeed, accelerationRate * Time.deltaTime);
		} else {
			_speed = Mathf.MoveTowards (_speed, MinSpeed, decelerationRate * Time.deltaTime);
		}
		
		if (CollisionPrediction != null)
			CollisionPrediction.Length = _speed*transform.localScale.z;
	}
	
    void TurnLeft()
	{
		Turn (270f);
    }

    void TurnRight()
    {
		Turn (90f);
    }

	// Turn right
	void Turn(float degrees)
	{
		_latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
		transform.Rotate(Vector3.up, degrees);
		NewWall();
	}

    private void OnGUI() {
        if (_showPauseMenu) {
            if (GUI.Button(new Rect(15/30f*WidthPixels, 10/20f*HeightPixels, 1/10f*WidthPixels, 1/20f*HeightPixels),
                "Exit", buttonStyle)) {
                Application.Quit();
            }
        }
    }
// ReSharper restore UnusedMember.Local

	void NewWall() {
	    _latestWallGameObject = ((GameObject)Network.Instantiate(Resources.Load("Wall"+NetworkControl.PlayerID), CurrentWallEnd, Quaternion.identity, 0)).transform;
		_latestWallGameObject.GetComponent<WallBehaviour> ().start = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().end = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall (CurrentWallEnd);
	}

	// Collision Stuff

	public void kill()
	{
		Debug.Log ("player is dead.");
		_latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        Network.Destroy(gameObject);
		// call some RPC method which will kill the dude on all devices
		// (must ?) also somehow display the info who has died and who wins...
		// and return to the main menu to start a new game.
		// or just start a new round when the last one has died
	}

	private CollisionPrediction _collisionPrediction;

	public CollisionPrediction CollisionPrediction{
		get {
			return _collisionPrediction;
		}
		set{
			_collisionPrediction = value;
		}
	}

	private int _predictedCollisions;

	public void OnPredictedCollisionEnter()
	{
		_predictedCollisions++;
	}

	public void OnPredictedCollisionExit()
	{
		_predictedCollisions--;
	}

	// SpeedUpCollider
	
	// ReSharper disable UnusedMember.Local
	public void OnSpeedUpTriggerEnter()
	{
		_numberOfWallsNear++;
	}
	
	public void OnSpeedUpTriggerExit()
	{
		_numberOfWallsNear--;
	}
}