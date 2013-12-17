using System.Linq;
using System.Threading;
using UnityEngine;

public class Drive : MonoBehaviour {
	//public Transform wallTemplate;
    protected Transform _latestWallGameObject;

	static readonly float MinSpeed = 25;
	static readonly float MaxSpeed = 50;
	static readonly float PowerUpSpeed = 80;
    protected static readonly float IndestructibleTime = 5;

    protected const float AccelerationRate = 10;
    protected const float DecelerationRate = 5;

    protected float _speed = MinSpeed;

	int _numberOfWallsNear = 0;

    protected int HeightPixels;
	protected int WidthPixels;

	public bool isIndestructible;
    
    protected bool _showPauseMenu;
    protected GUIStyle buttonStyle = new GUIStyle();

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

    protected Vector3 CurrentWallEnd {
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
        transform.FindChild("CollisionPredictor").GetComponent<CollisionPrediction>()._drive = this;
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
        if (_predictedCollisions > 0 && !isIndestructible || _predictedWallCollisions > 0)
        {
			Kill();
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

    protected bool ApplyUserCommands ()
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

    protected void AdjustSpeed () {
		if (_numberOfWallsNear > 0) {
			_speed = Mathf.MoveTowards (_speed, MaxSpeed, AccelerationRate * Time.deltaTime);
		} else {
			_speed = Mathf.MoveTowards (_speed, MinSpeed, DecelerationRate * Time.deltaTime);
		}
		
		if (CollisionPrediction != null)
			CollisionPrediction.Length = _speed*transform.localScale.z;
	}

    protected void TurnLeft()
	{
		Turn (270f);
    }

    protected void TurnRight()
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
// ReSharper restore UnusedMember.Local

    protected virtual void NewWall() {
	    _latestWallGameObject = ((GameObject)Network.Instantiate(Resources.Load("Wall"+ Game.Instance.PlayerID), CurrentWallEnd, Quaternion.identity, 0)).transform;
		_latestWallGameObject.GetComponent<WallBehaviour> ().start = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().end = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall (CurrentWallEnd);
	}

	// Collision Stuff

	public delegate void KillEvent();
	public KillEvent OnKillEvent;

    protected virtual void Kill()
	{
		Debug.Log ("player is dead.");
		_latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        Network.Destroy(gameObject);
        GameObject.Find("Network").networkView.RPC("KillPlayer", RPCMode.All, Game.Instance.PlayerID);
        if (OnKillEvent != null)
			OnKillEvent ();

		// call some RPC method which will kill the dude on all devices
        // (must ?) also somehow display the info who has died and who wins...
        // and return to the main menu to start a new game.
        // or just start a new round when the last one has died
	}

    public CollisionPrediction CollisionPrediction { get; set; }

    protected int _predictedCollisions;
    protected int _predictedWallCollisions;

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

    public void ConsumeSpeedPowerup()
    {
        _speed = PowerUpSpeed;
    }

    public void ConsumeIndestructiblePowerup()
    {
        (new Thread(() =>
        {
            isIndestructible = true;
            for (var i = 0; i < 10 * IndestructibleTime; i++)
            {
                if (!Game.Instance.isAlive(Game.Instance.PlayerID)) {
                    break;
                }
                Game.Instance.IndestructibleTimeLeft = IndestructibleTime - 0.1*i;
                Thread.Sleep(100);
            }
            isIndestructible = false;
        })).Start();
    }

    public void OnPredictedGameWallCollisionEnter() {
        _predictedWallCollisions++;
    }

    public void OnPredictedGameWallCollisionExit() {
        _predictedWallCollisions--;
    }
}