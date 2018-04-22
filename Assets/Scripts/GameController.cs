using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
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

	// UI
	public Text Message;
	NetworkView networkView;
	NetworkClient client;


	private void Start ()
	{
//		Network.InitializeServer(10, 5000);
//		networkView = new NetworkView ();
		NetworkServer.Listen(4444);
		client = new NetworkClient();
		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MyMessageTypes.MSG_TEXT, OnUpdateText);
		client.Connect ("127.0.0.1", 4444);

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
//		if (!isServer) {
//			Debug.Log ("not a server");
//			return;
//		}
//		Network.Connect("127.0.0.1", 5000);
	}
	
	private void Update () 
	{
		if (Input.GetKeyDown("space"))
		{
			// Player gives up the turn
			GiveUpTurn();
			UpdateTextServer ();
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

	public class MyMessageTypes
	{
		public static short MSG_TEXT = 1000;
		public static short MSG_SCORE = 1005;
	};
		
	public class TextMessage : MessageBase {
		public string text;

		public TextMessage(string text) {
			this.text = text;
		}
	}
	// RPC
//	[Command]
	private void UpdateTextServer() 
	{
		NetworkServer.SendToAll(MyMessageTypes.MSG_TEXT, new TextMessage("hello there!"));
	}

	private void OnUpdateText(NetworkMessage netMsg) {
		Message.text = netMsg.reader.ReadString();
	}

	public void OnConnected(NetworkMessage netMsg) {
		Debug.Log ("connected to server");
	}
		
}
