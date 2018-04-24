using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	private readonly List<IPlayer> allPlayers = new List<IPlayer>();
	private readonly Dictionary<IPlayer, PointController> playersPositions = 
		new Dictionary<IPlayer, PointController>();
	
	// Health of players is counted as left number of strikes from other players
	private const int InitialHealth = 3;
	private readonly Dictionary<IPlayer, int> playersHealth = new Dictionary<IPlayer, int>();

	private IPlayer currentPlayer;
	private bool currentPlayerMoved;

	// Time, which is left for current player's turn
	private int timeLeft;
	private const int TimeForTurn = 30;
	private Coroutine timerCoroutine;
	
	// UI
	public UIController UiController;
	public Text Message;
	public InputField Ip;
	NetworkClient client;
	ConnectionConfig cc;


	private void Start ()
	{
		cc = new ConnectionConfig ();
		cc.AddChannel(QosType.Reliable);

		NetworkServer.Configure (cc, 10);
		NetworkServer.Listen(4444);
	
		client = new NetworkClient();
		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MyMessageTypes.MSG_MOVE, OnMove);
		client.RegisterHandler(MyMessageTypes.MSG_TURN, OnTurn);
		client.RegisterHandler(MyMessageTypes.MSG_ATTACK, OnAttack);
		client.Configure(cc, 10);


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

		allPlayers.Add(Lizard);
		allPlayers.Add(Wizard);
		allPlayers.Add(Knight);
		allPlayers.Add(Unicorn);
		
		// Start game
		currentPlayer.SetActive();
		UiController.SetTurn(currentPlayer);
		timerCoroutine = StartCoroutine(StartTimer());


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
//			UpdateTextServer ();
		}
		if (Input.GetKeyDown("return"))
		{
			// Player gives up the turn
			ConnectToServer ();
		}
	}

	// Timer, counting, how much left for current player's turn
	private IEnumerator StartTimer()
	{
		timeLeft = TimeForTurn;
		while (timeLeft >= 0)
		{
			UiController.UpdateTimer(timeLeft);
			yield return new WaitForSeconds(1.0f);
			timeLeft--;
		}
		GiveUpTurn();
	}

	private void ConnectToServer()
	{
		Debug.Log (Ip.text);
		client.Connect(Ip.text, 4444);
	}

	public class TurnMessage : MessageBase {
		public int NextPlayer;

		public TurnMessage(int nextPlayer) {
			NextPlayer = nextPlayer;
		}

		public TurnMessage() {
		}
	}

	private void GiveUpTurn()
	{
		// Some player has won
		if (alivePlayers.Count == 1) UiController.FinishGame(alivePlayers.First());
		
		// Choose next player
		var currentPlayerIndex = alivePlayers.IndexOf(currentPlayer);
		var nextPlayerIndex = currentPlayerIndex == alivePlayers.Count - 1 ? 0 : currentPlayerIndex + 1;
		currentPlayer = alivePlayers[nextPlayerIndex];
	
		currentPlayerMoved = false;
		currentPlayer.SetActive();
		
		UiController.SetTurn(alivePlayers[nextPlayerIndex]);
		
		StopCoroutine(timerCoroutine);
		timerCoroutine = StartCoroutine(StartTimer());

		NetworkServer.SendToAll(MyMessageTypes.MSG_TURN, 
			new TurnMessage(nextPlayerIndex));
	}

	private void KillPlayer(IPlayer target)
	{
		target.Hit();
		Destroy(target.GetGameObject());
		alivePlayers.Remove(target);
		playersPositions.Remove(target);
	}

	public class MoveMessage : MessageBase {
		public int player;
		public int x;
		public int y;

		public MoveMessage(int player, int x, int y) {
			this.player = player;
			this.x = x;
			this.y = y;
		}

		public MoveMessage() {
		}
	}

	public void MovePlayer(PointController newPos)
	{
		if (currentPlayerMoved) return;
		
		if (playersPositions.ContainsValue(newPos)) return;
		
		playersPositions[currentPlayer] = newPos;
		var pos = newPos.gameObject.transform.position;
		currentPlayer.GetGameObject().transform.position = new Vector3(pos.x, pos.y, 0);

		NetworkServer.SendToAll(MyMessageTypes.MSG_MOVE, 
			new MoveMessage(allPlayers.IndexOf(currentPlayer), newPos.XCoordinate, newPos.YCoordinate));

		currentPlayerMoved = true;
	}

	public class AttackMessage : MessageBase {
		public int Target;

		public AttackMessage(int target) {
			Target = target;
		}

		public AttackMessage() {
		}
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
			UiController.ReduceHealth(target);
		}
		else
		{
			// Die
			KillPlayer(target);
			UiController.ReduceHealth(target);
			UiController.KillPlayer(target);
		}

		NetworkServer.SendToAll(MyMessageTypes.MSG_ATTACK, 
			new AttackMessage(allPlayers.IndexOf(target)));
		
		GiveUpTurn();
	}

	public class MyMessageTypes
	{
		public static short MSG_MOVE = 1000;
		public static short MSG_TURN = 1001;
		public static short MSG_ATTACK = 1002;

	};
		
	// RPC
//	[Command]
//	private void UpdateTextServer() 
//	{
//		NetworkServer.SendToAll(MyMessageTypes.MSG_TEXT, new TextMessage("hello there!"));
//	}

	private void OnUpdateText(NetworkMessage netMsg) {
		Message.text = netMsg.reader.ReadString();
	}

	public void OnConnected(NetworkMessage netMsg) {
		Debug.Log ("connected to server");
	}
		
	private void OnMove(NetworkMessage netMsg) {
		var message = netMsg.ReadMessage<MoveMessage> ();
		var player = message.player;
		var x = message.x;
		var y = message.y;

		var pos = FieldPoints [5 * x + y];
		playersPositions[allPlayers[player]] = pos;
		allPlayers[player].GetGameObject().transform.position = pos.gameObject.transform.position;
	}

	private void OnTurn(NetworkMessage netMsg) {
		var message = netMsg.ReadMessage<TurnMessage> ();
		var player = message.NextPlayer;

		if (alivePlayers.Count == 1) Application.Quit();

		currentPlayer = alivePlayers[player];

		currentPlayerMoved = false;
		currentPlayer.SetActive();

	}

	private void OnAttack(NetworkMessage netMsg) {
		var message = netMsg.ReadMessage<AttackMessage> ();
		var target = allPlayers[message.Target];

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
	}
}
