using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
	public List<PointController> FieldPoints;
	
	public LizardController Lizard;
	public WizardController Wizard;
	public KnightController Knight;
	public UnicornController Unicorn;

	private readonly List<IPlayer> alivePlayers = new List<IPlayer>();
	private readonly Dictionary<IPlayer, PointController> playersPositions = 
		new Dictionary<IPlayer, PointController>();
	
	// Health of players is counted as left number of strikes from other players
	private const int InitialHealth = 3;
	private readonly Dictionary<IPlayer, int> playersHealth = new Dictionary<IPlayer, int>();

	private IPlayer currentPlayer;
	private bool currentPlayerMoved;


	private void Start ()
	{
		// Choose, who will be the first
		var rand = new System.Random();
		switch (rand.Next(0, 3))
		{
			case 0: currentPlayer = Lizard; break;
			case 1: currentPlayer = Wizard; break;
			case 2: currentPlayer = Knight; break;
			case 3: currentPlayer = Unicorn; break;
		}
		
		// Set all initial positions of all players
		playersPositions.Add(Lizard, FieldPoints[0]);		
		playersPositions.Add(Wizard, FieldPoints[4]);
		playersPositions.Add(Knight, FieldPoints[20]);
		playersPositions.Add(Unicorn, FieldPoints[24]);
				
		playersHealth.Add(Lizard, InitialHealth);
		playersHealth.Add(Wizard, InitialHealth);
		playersHealth.Add(Knight, InitialHealth);
		playersHealth.Add(Unicorn, InitialHealth);
		
		alivePlayers.Add(Lizard);
		alivePlayers.Add(Wizard);
		alivePlayers.Add(Knight);
		alivePlayers.Add(Unicorn);
		
		// Start game
		currentPlayer.SetActive();
	}
	
	private void Update () 
	{
		if (Input.GetKeyDown("space"))
		{
			// Player gives up the turn
			GiveUpTurn();
		}
	}

	private void GiveUpTurn()
	{
		// Some player has won
		if (alivePlayers.Count == 1) Application.Quit();
		
		// Choose next player
		var currentPlayerIndex = alivePlayers.IndexOf(currentPlayer);
		currentPlayer = 
			currentPlayerIndex == alivePlayers.Count - 1 
				? alivePlayers[0] 
				: alivePlayers[currentPlayerIndex + 1];
	
		currentPlayerMoved = false;
		currentPlayer.SetActive();
	}

	private void KillPlayer(IPlayer target)
	{
		target.Hit();
		Destroy(target.GetGameObject());
		alivePlayers.Remove(target);
	}
	
	public void MovePlayer(PointController newPos)
	{
		if (currentPlayerMoved) return;
		
		playersPositions[currentPlayer] = newPos;
		currentPlayer.GetGameObject().transform.position = newPos.gameObject.transform.position;

		currentPlayerMoved = true;
	}

	public void AttackPlayer(IPlayer target)
	{
		if (ReferenceEquals(currentPlayer, target)) return;

		var attackerPos = playersPositions[currentPlayer];
		var targetPos = playersPositions[target];
		if (Math.Abs(attackerPos.XCoordinate - targetPos.XCoordinate) > 1 ||
		    Math.Abs(attackerPos.YCoordinate - targetPos.YCoordinate) > 1)
			return;
		
		if (playersHealth[target] != 1)
		{
			// Target is still alive
			target.Hit();
			playersHealth[target] -= 1;
		}
		else
		{
			// Die
			KillPlayer(target);
		}
		
		GiveUpTurn();
	}
}
