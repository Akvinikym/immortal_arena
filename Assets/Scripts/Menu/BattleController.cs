using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Menu
{
	public class BattleController : MonoBehaviour
	{
		public NetworkManager Network;
		public GameObject BattleView;
		public Text Player1;
		public Text Player2;
		public Text Player3;
		public Text Player4;
		public Button ReadyBtn;
		public Button LeaveBtn;
		public Button CloseBtn;
		
		public Button BattleBtnPrefab;
		
		private readonly List<Button> availableBattlesButtons = new List<Button>();
		private List<NetworkManager.Lobby> availableLobbies;
		private Dictionary<int, string> playerToIP;

		private NetworkManager.Lobby currentLobby;

		private Coroutine battlePoller;
		private Coroutine lobbyPoller;

		// Use this for initialization
		private void Start()
		{
			Debug.Log("Hi");
			CloseBtn.onClick.AddListener(HideJoinBattleMenu);
			battlePoller = StartCoroutine(StartPollingBattles());
			UpdatePlayersAddresses(new List<string>());
		}

		private void PollBattles()
		{
			var lobbies = Network.GetLobbies();
			foreach (var button in availableBattlesButtons)
			{
				Destroy(button.gameObject);
			}
			availableBattlesButtons.Clear();
			
			foreach (var lobby in lobbies)
			{
				var button = Instantiate(BattleBtnPrefab);
				button.name = lobby.Id.ToString();
				button.transform.SetParent(BattleView.transform);
				button.GetComponentInChildren<Text>().text = "Battle №" + lobby.Id;
				button.onClick.AddListener(() => JoinBattle(lobby.Id));
				button.gameObject.SetActive(true);
				availableBattlesButtons.Add(button);
			}
			availableLobbies = lobbies;
		}
		private IEnumerator StartPollingBattles()
		{
			while (true)
			{
				PollBattles();
				yield return new WaitForSeconds(5);
			}
		}

		private void JoinBattle(int id)
		{
			if (currentLobby != null)
			{
				if (!LeaveLobby(currentLobby.Id))
					return;
			}
			if (!Network.JoinLobby(id)) return;

			currentLobby = Network.GetLobby(id);
			UpdatePlayersAddresses(currentLobby.Players.ToList());
			
			LeaveBtn.onClick.AddListener(() => LeaveLobby(id));
			lobbyPoller = StartCoroutine(StartPollingLobby());
		}
		private IEnumerator StartPollingLobby()
		{
			while (true)
			{
				var lobby = Network.GetLobby(currentLobby.Id);
				UpdatePlayersAddresses(lobby.Players.ToList());
				currentLobby = lobby;
				
				yield return new WaitForSeconds(2.5f);
			}
		}

		private bool LeaveLobby(int id)
		{
			if (!Network.LeaveLobby(id)) return false;
			
			LeaveBtn.onClick.RemoveAllListeners();
			UpdatePlayersAddresses(new List<string>());
			currentLobby = null;
			StopCoroutine(lobbyPoller);

			return true;
		}

		private void UpdatePlayersAddresses(List<string> addresses)
		{
			Player1.text = "";
			Player2.text = "";
			Player3.text = "";
			Player4.text = "";
			if (!addresses.Any()) return;
			switch (addresses.Count)
			{
				case 1:
					Player1.text = "Player 1: " + addresses[0];
					break;
				case 2:
					Player1.text = "Player 1: " + addresses[0];
					Player2.text = "Player 2: " + addresses[1];
					break;
				case 3:
					Player1.text = "Player 1: " + addresses[0];
					Player2.text = "Player 2: " + addresses[1];
					Player3.text = "Player 3: " + addresses[2];
					break;
				case 4:
					Player1.text = "Player 1: " + addresses[0];
					Player2.text = "Player 2: " + addresses[1];
					Player3.text = "Player 3: " + addresses[2];
					Player4.text = "Player 4: " + addresses[3];
					break;
			}
		}

		public bool CreateBattle()
		{
			var lobbyId = Network.CreateLobby();
			if (lobbyId == -1) return false;

			JoinBattle(lobbyId);
			return true;
		}
		
		public void ShowBattleMenu()
		{
			this.gameObject.SetActive(true);
		}

		private void HideJoinBattleMenu()
		{
			if (currentLobby != null)
			{
				LeaveLobby(currentLobby.Id);
				currentLobby = null;
			}
			// TODO: bug is somewhere here
			this.gameObject.SetActive(false);
		}
	}
}