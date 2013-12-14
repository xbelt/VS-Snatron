using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Drive : MonoBehaviour {
	public Transform wallTemplate;
    private Transform _latestWallGameObject;

	static readonly float MinSpeed = 25;
	static readonly float MaxSpeed = 50;

	public float accelerationRate = 10;
	public float decelerationRate = 5;
	
	float _speed = MinSpeed;

	int _numberOfWallsNear = 0;

    private int _heightPixels;
	private int _widthPixels;

    private GameConfiguration _config;

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
	void Start ()
	{
	    _config = GameObject.FindGameObjectWithTag("gameConfig").GetComponent<GameConfiguration>();

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
        _heightPixels = Screen.height;
        _widthPixels = Screen.width;
#endif
	}
	
// ReSharper disable once UnusedMember.Local
	void Update () {
	    if (GetComponent<NetworkView>().isMine)
	    {
	        bool applied = ApplyUserCommands ();
			if (applied)
				return;
	    }
		
		AdjustSpeed ();

		// Hope Collision events have happened
		// otherwise do not proceed forward when turning around

		print ("Predicted Collisions: " + _predictedCollisions);
		if (_deathPredicted) {
			kill();
			return;
		}
		
		if (_predictedCollisions > 0) {
			_deathPredicted = true;
		}
		
		transform.Translate(Vector3.forward*_speed*Time.deltaTime);
		if (_latestWallGameObject != null) {
			_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall(CurrentWallEnd);
		}
	}

	bool ApplyUserCommands ()
	{
		//Handling touch input
		foreach (var touch in Input.touches.Where (touch => touch.phase == TouchPhase.Began)) {
			if (touch.position.x > _widthPixels / 2) {
				TurnRight ();
				return true;
			}
			else {
				TurnLeft ();
				return true;
			}
		}
		// Input for preview
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			TurnLeft ();
			return true;
		}
		else
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			TurnRight ();
			return true;
		}
		return false;
	}

	private bool _deathPredicted = false;

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
		_deathPredicted = false;

		_latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
		transform.Rotate(Vector3.up, degrees);
		NewWall();

		if (_predictedCollisions > 0) {
			_deathPredicted = true;
		}
	}

// ReSharper restore UnusedMember.Local

	void NewWall() {
	    _latestWallGameObject = (Transform) Network.Instantiate(wallTemplate, CurrentWallEnd, Quaternion.identity, 0);
		_latestWallGameObject.GetComponent<WallBehaviour> ().start = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().end = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall (CurrentWallEnd);
	}

	// Collision Stuff

	private bool isAlive = true;

	// This player will die
	public void kill()
	{
		Debug.Log ("player is dead.");
		isAlive = false;
		_latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
		Destroy (gameObject);
		// TODO sync
	}

	private CollisionPrediction _collisionPrediction;

	public CollisionPrediction CollisionPrediction{
		get {
			return _collisionPrediction;
		}
		set{
			this._collisionPrediction = value;
		}
	}

	private int _predictedCollisions = 0;

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