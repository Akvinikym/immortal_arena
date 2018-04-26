using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Sockets;

namespace Network {

	public class MessageTypes
	{
		public static short MSG_MOVE = 1000;
		public static short MSG_TURN = 1001;
		public static short MSG_ATTACK = 1002;
	};

	public class AttackMessage : MessageBase
	{
		public int Target;

		public AttackMessage(int target)
		{
			Target = target;
		}

		public AttackMessage()
		{
		}
	}

	public class MoveMessage : MessageBase
	{
		public int player;
		public int x;
		public int y;

		public MoveMessage(int player, int x, int y)
		{
			this.player = player;
			this.x = x;
			this.y = y;
		}

		public MoveMessage()
		{
		}
	}

	public class TurnMessage : MessageBase
	{
		public int TurnNumber;

		public TurnMessage(int turnNumber)
		{
			TurnNumber = turnNumber;
		}

		public TurnMessage()
		{
		}
	}

	public class NetworkManager : NetworkBehaviour {

		private List<string> playerAddresses;
		private ConnectionConfig cc;
		private List<NetworkClient> clients;

		private int currentTurn = 0;

		public Action<int, int, int> OnMoveHandler;
		public Action<int> OnAttackHandler;
		public Action OnTurnHandler;

		private void Awake() {
			Debug.Log ("starting network manager");
			playerAddresses = new List<string>{"192.168.0.177", "192.168.0.179"};

			cc = new ConnectionConfig();
			cc.AddChannel(QosType.Reliable);

			NetworkServer.Configure(cc, 10);
			NetworkServer.Listen(4444);

			clients = new List<NetworkClient> ();

		}

		public void Connect() {
			foreach (var address in playerAddresses) {
				if (GetLocalIP () == address) {
					continue;
				}
				var client = new NetworkClient ();
				client.RegisterHandler (MsgType.Connect, OnConnected);
				client.RegisterHandler (MessageTypes.MSG_MOVE, OnMove);
				client.RegisterHandler (MessageTypes.MSG_TURN, OnTurn);
				client.RegisterHandler (MessageTypes.MSG_ATTACK, OnAttack);
				client.Configure (cc, 10);
				client.Connect (address, 4444);
				clients.Add (client);
			}
		}

		public void OnConnected(NetworkMessage netMsg)
		{
			Debug.Log("connected to server " + netMsg.conn.address);
		}

		public void OnMove(NetworkMessage netMsg)
		{
			Debug.Log ("moving!");
			var message = netMsg.ReadMessage<MoveMessage>();
			var player = message.player;
			var x = message.x;
			var y = message.y;

			OnMoveHandler (player, x, y);
		}

		public void OnTurn(NetworkMessage netMsg)
		{
			var turn = netMsg.ReadMessage<TurnMessage> ().TurnNumber;

			if (currentTurn == turn) {
				return;
			}
			currentTurn = turn;
			Debug.Log ("next turn!");
			OnTurnHandler();
		}

		public void OnAttack(NetworkMessage netMsg)
		{
			Debug.Log ("attacking!");
			var message = netMsg.ReadMessage<AttackMessage>();

			OnAttackHandler (message.Target);
		}

		public void Move(int player, int x, int y) {
			NetworkServer.SendToAll(Network.MessageTypes.MSG_MOVE,
				new MoveMessage (player, x, y));
		}

		public void NextTurn() {
			NetworkServer.SendToAll(Network.MessageTypes.MSG_TURN,
				new TurnMessage (currentTurn + 1));
			currentTurn++;
		}

		public void Attack(int player) {
			NetworkServer.SendToAll(Network.MessageTypes.MSG_ATTACK,
				new AttackMessage (player));
		}

		public int GetPlayerNumber() {
			return playerAddresses.IndexOf(GetLocalIP());
		}

		private static string GetLocalIP()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
					return ip.ToString();
			}
			return "";
		}
	}
}
