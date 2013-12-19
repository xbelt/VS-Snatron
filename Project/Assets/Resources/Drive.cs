using System.Linq;
using System.Threading;
using UnityEngine;

public class Drive : MonoBehaviour {

	public GUIStyle labelGUIStyle;

	public int playerId;

    protected Transform _latestWallGameObject;

	static readonly float MinSpeed = 25;
	static readonly float MaxSpeed = 50;
	static readonly float PowerUpSpeed = 80;
    protected static readonly float IndestructibleTime = 5;

    protected const float AccelerationRate = 10;
    protected const float DecelerationRate = 5;

    protected float _speed = MinSpeed;

	protected bool IsIndestructible;
	protected double IndestructibleTimeLeft { get; set; }

	int _numberOfWallsNear = 0;

    protected int HeightPixels;
	protected int WidthPixels;

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
        if (_predictedCollisions > 0 && !IsIndestructible || _predictedWallCollisions > 0)
        {
			DeadlyCollide();
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

	void OnGUI()
	{
		if (IsIndestructible)
		{
			GUI.Label(new Rect(9 / 20f * WidthPixels, 
			                   19 / 40f * HeightPixels, 
			                   1 / 10f * WidthPixels, 
			                   1 / 20f * HeightPixels),
			          "Indestructible for " + IndestructibleTimeLeft.ToString("0.0") + "s", 
			          labelGUIStyle);
		}
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
		_latestWallGameObject = ((GameObject)Network.Instantiate(Resources.Load("Wall"+ playerId), CurrentWallEnd, Quaternion.identity, 0)).transform;
		_latestWallGameObject.GetComponent<WallBehaviour> ().start = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().end = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall (CurrentWallEnd);
	}

	// Collision Stuff

	public delegate void DeadlyCollisionEvent(int playerId);
	public DeadlyCollisionEvent OnDeadlyCollision;

	/// <summary>
	/// Earlier, this was called Kill()
	/// </summary>
    protected virtual void DeadlyCollide()
	{
		Debug.Log ("DeadlyCollision.");
		_latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        if (OnDeadlyCollision != null)
			OnDeadlyCollision (playerId);
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

    public void ConsumeIndestructiblePowerup() {
        IndestructibleTimeLeft += IndestructibleTime;
        if (!(IndestructibleTimeLeft > IndestructibleTime)) {
            (new Thread(() => {
                IsIndestructible = true;
                while (IndestructibleTimeLeft > 0.1 && Game.Instance.isAlive(Game.Instance.PlayerID)) {
                    IndestructibleTimeLeft -= 0.1;
                    Thread.Sleep(100);
                }

                IsIndestructible = false;
            })).Start();
        }
    }

    public void OnPredictedGameWallCollisionEnter() {
        _predictedWallCollisions++;
    }

    public void OnPredictedGameWallCollisionExit() {
        _predictedWallCollisions--;
    }
}