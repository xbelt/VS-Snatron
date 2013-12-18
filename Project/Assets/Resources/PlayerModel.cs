using UnityEngine;
using System.Collections;

public class PlayerModel
{
	public readonly int id;
	public readonly string name;
	public int rank; // for one round
	public int score; // for a series of rounds
	public bool isAlive;
	
	public PlayerModel(int id, string name)
	{
		this.id = id;
		this.name = name;
		startGame ();
	}
	
	public void startGame()
	{
		isAlive = true;
		score = 0;
		rank = 0;
	}
	
	public void startRound()
	{
		isAlive = true;
		rank = 1;
	}
}

