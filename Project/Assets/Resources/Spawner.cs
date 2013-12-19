using UnityEngine;
using System.Collections;
using Assets.Resources;
using System.Collections.Generic;

public class Spawner
{
	private Level _level;

	private int _localPlayerId;

	private readonly Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject> ();
	public GameObject LocalPlayer {
		get {
			GameObject go;
			spawnedPlayers.TryGetValue(_localPlayerId, out go);
			return go;
		}
	}

	Transform _playerPrefab;
	Transform _gridPrefab;

	public delegate void SpawnEvent(int playerId);
	public SpawnEvent OnSpawned;
	
	public delegate void KillEvent (int playerId);
	public KillEvent OnKilled;

	public Spawner(Level level)
	{
		_level = level;
		_gridPrefab = Resources.Load<Transform>("Lines");
	}

	public int LocalPlayerId {
		get {
			return _localPlayerId;
		}
		set {
			_localPlayerId = value;
		}
	}

	public void SpawnLocalPlayer(KillEvent OnKilled)
	{
		Vector3 location;
		Quaternion orientation;
		_level.MapStartLocation (_localPlayerId, out location, out orientation);

		_playerPrefab = Resources.Load<Transform>("Player" + _localPlayerId);
		
		var player = Network.Instantiate(_playerPrefab, location, orientation, 0) as Transform;
		var cam = GameObject.Find("Main Camera");
		cam.AddComponent<SmoothFollow>().target = player;
		
		MonoBehaviour.Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
		MonoBehaviour.Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
		
		Drive _localPlayerDrive = player.GetComponent<Drive> ();
		_localPlayerDrive.playerId = _localPlayerId;
		_localPlayerDrive.OnDeadlyCollision += (int id) => OnKilled (id); // TODO
		
		spawnedPlayers.Add(_localPlayerId, player.gameObject);

		if (OnSpawned != null)
			OnSpawned (_localPlayerId);
	}
	
	public void SpawnAI(int playerId, KillEvent onKilled) {
		_playerPrefab = Resources.Load<Transform>("Player" + playerId);
		Vector3 location;
		Quaternion orientation;
		_level.MapStartLocation(playerId, out location, out orientation);

		var player = Network.Instantiate(_playerPrefab, location, orientation, 0) as Transform;
		MonoBehaviour.Destroy(player.gameObject.GetComponent<Drive>());
		player.gameObject.AddComponent<KIControler>();
		KIControler ai = player.gameObject.GetComponent<KIControler>();
		ai.KIId = playerId; // TODO instead register event as normal and assign it the next free id
		ai.OnDeadlyCollision += (int id) => onKilled (id);
		ai.playerId = playerId;
		spawnedPlayers.Add(playerId, player.gameObject);

		if (OnSpawned != null)
			OnSpawned (playerId);
	}
	
	public void Kill(int playerId)
	{
		GameObject target = spawnedPlayers [playerId];
		if (target == null)
			return; // Warning : not cool behavior!
		
		Network.Destroy (target);
		spawnedPlayers.Remove (playerId);
		// Logically destroy the player
		if (OnKilled != null)
			OnKilled(playerId);
		// TODO move this towards network / Main Controller
		GameObject.Find("Network").networkView.RPC("KillPlayer", RPCMode.All, playerId);
	}

	public void InstantiateLevelObjects()
	{
		InstantiateGameBorders ();
		InstantiateCubes ();
	}
	
	private System.Random random = new System.Random();

	public void SpawnPowerUp()
	{
		//TODO: Make dependent of framerate
		Debug.Log ("Spawn powerUp");
		var x = random.Next (-_level.FieldBorderCoordinates, _level.FieldBorderCoordinates);
		var z = random.Next (-_level.FieldBorderCoordinates, _level.FieldBorderCoordinates);
		Network.Instantiate (
			Resources.Load<Transform> ("PowerUpPrefab" + random.Next (0, 2)),
			new Vector3 (x, 0, z),
			Quaternion.identity, 0);
	}
	
	private void InstantiateGameBorders() {
		var leftWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var frontWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var rightWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var backWall =
			Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
		var wallColor = new Color(204 / 255f, 204 / 255f, 204 / 255f, 1f);
		var shader = Shader.Find("Diffuse");

		int dist = _level.FieldBorderCoordinates;

		if (leftWall != null)
		{
			leftWall.transform.localScale = new Vector3(1, 5, 1);
			leftWall.GetComponent<WallBehaviour>().start = new Vector3(-dist, 1.5f, -dist);
			leftWall.GetComponent<WallBehaviour>().updateWall(new Vector3(dist, 1.5f, -dist));
			leftWall.renderer.material.shader = shader;
		}
		if (frontWall != null)
		{
			frontWall.transform.localScale = new Vector3(1, 5, 1);
			frontWall.GetComponent<WallBehaviour>().start = new Vector3(dist, 1.5f, -dist);
			frontWall.GetComponent<WallBehaviour>().updateWall(new Vector3(dist, 1.5f, dist));
			frontWall.renderer.material.shader = shader;
		}
		if (rightWall != null)
		{
			rightWall.transform.localScale = new Vector3(1, 5, 1);
			rightWall.GetComponent<WallBehaviour>().start = new Vector3(dist, 1.5f, dist);
			rightWall.GetComponent<WallBehaviour>().updateWall(new Vector3(-dist, 1.5f, dist));
			rightWall.renderer.material.shader = shader;
		}
		if (backWall != null)
		{
			backWall.transform.localScale = new Vector3(1, 5, 1);
			backWall.GetComponent<WallBehaviour>().start = new Vector3(-dist, 1.5f, dist);
			backWall.GetComponent<WallBehaviour>().updateWall(new Vector3(-dist, 1.5f, -dist));
			backWall.renderer.material.shader = shader;
		}
	}
	
	private void InstantiateCubes() {
		var cubes = new List<Transform>();
		var random = new System.Random();
		var shader = Shader.Find("Diffuse");

		int dist = _level.FieldBorderCoordinates;
		int n = _level.NumberOfCubes;
		for(var i = 0; i < n; ++i) {
			var x = random.Next(-dist, dist);
			var z = random.Next(-dist, dist);
			cubes.Add(Network.Instantiate(Resources.Load<Transform>("Cube"), new Vector3(x, 1.5f, z), Quaternion.identity, 0) as Transform);
			cubes[i].renderer.material.shader = shader;
		}
	}

	private void ClearMyObjects()
	{
		var walls = GameObject.FindGameObjectsWithTag("wall");
		foreach (var wall in walls)
		{
			MonoBehaviour.Destroy(wall);
		}
		var gameWalls = GameObject.FindGameObjectsWithTag("gameWall");
		foreach (var gameWall in gameWalls) {
			MonoBehaviour.Destroy(gameWall);
		}
		var lines = GameObject.FindGameObjectsWithTag("line");
		foreach (var line in lines)
		{
			MonoBehaviour.Destroy(line);
		}
		var cubes = GameObject.FindGameObjectsWithTag("cube");
		foreach (var cube in cubes)
		{
			MonoBehaviour.Destroy(cube);
		}
		var players = GameObject.FindGameObjectsWithTag("tron");
		foreach (var player in players)
		{
			MonoBehaviour.Destroy(player);
		}
		var powerUp0s = GameObject.FindGameObjectsWithTag("powerUp0");
		foreach (var powerUp0 in powerUp0s) {
			MonoBehaviour.Destroy(powerUp0);
		}
		var powerUp1s = GameObject.FindGameObjectsWithTag("powerUp1");
		foreach (var powerUp1 in powerUp1s) {
			MonoBehaviour.Destroy(powerUp1);
		}
	}
}

