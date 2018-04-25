using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Menu
{
	public class BattleController : MonoBehaviour
	{
		/// <summary>
		/// To be read from the second, arena scene
		/// </summary>
		public static Dictionary<int, string> FinalPlayerToIP;
		
		public NetworkManager Network;
		public GameObject BattleView;
		public Text ChosenBattleText;
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
		
		private readonly Dictionary<int, string> playerToIP = new Dictionary<int, string>();
		private int thisPlayerIndex = -1;
		private bool thisPlayerReady = false;
		
		private NetworkManager.Lobby currentLobby;

		private Coroutine battlePoller;
		private Coroutine lobbyPoller;

		// Use this for initialization
		private void Start()
		{
			CloseBtn.onClick.AddListener(HideJoinBattleMenu);
			ReadyBtn.onClick.AddListener(OnReadyBtnClick);
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
			UpdatePlayersAddresses(currentLobby.Players.Select(p => p.Address).ToList());
			UpdateChosenBattleText();

			thisPlayerIndex = currentLobby.PlayersCount;
			LeaveBtn.onClick.AddListener(() => LeaveLobby(id));
			lobbyPoller = StartCoroutine(StartPollingLobby());
		}
		private IEnumerator StartPollingLobby()
		{
			while (true)
			{
				currentLobby = Network.GetLobby(currentLobby.Id);
				UpdatePlayersAddresses(currentLobby.Players.Select(p => p.Address).ToList());
				
				if (currentLobby.Players.All(p => p.IsReady))
					FinishMatchmaking();
				
				yield return new WaitForSeconds(2.5f);
			}
		}

		private bool LeaveLobby(int id)
		{
			if (!Network.LeaveLobby(id)) return false;
			
			LeaveBtn.onClick.RemoveAllListeners();
			UpdatePlayersAddresses(new List<string>());
			UpdateChosenBattleText();
			currentLobby = null;
			StopCoroutine(lobbyPoller);
			thisPlayerIndex = -1;
			thisPlayerReady = false;

			return true;
		}

		private void OnReadyBtnClick()
		{
			if (currentLobby == null) return;

			if (!thisPlayerReady)
			{
				if (!Network.SetReady(currentLobby.Id)) return;
				thisPlayerReady = true;
			}
			else
			{
				if (!Network.SetNotReady(currentLobby.Id)) return;
				thisPlayerReady = false;
			}

			switch (thisPlayerIndex)
			{
				case 1: Player1.color = thisPlayerReady ? Color.green : Color.black; break;
				case 2: Player2.color = thisPlayerReady ? Color.green : Color.black; break;
				case 3: Player3.color = thisPlayerReady ? Color.green : Color.black; break;
				case 4: Player4.color = thisPlayerReady ? Color.green : Color.black; break;
			}
		}

		private void UpdatePlayersAddresses(List<string> addresses)
		{
			playerToIP.Clear();
			Player1.text = "";
			Player2.text = "";
			Player3.text = "";
			Player4.text = "";
			
			if (!addresses.Any()) return;
			switch (addresses.Count)
			{
				case 1:
					Player1.text = "Player 1: " + addresses[0];
					Player1.color = currentLobby.Players[0].IsReady ? Color.green : Color.black;
					playerToIP.Add(1, addresses[0]);
					break;
				case 2:
					Player1.text = "Player 1: " + addresses[0];
					Player1.color = currentLobby.Players[0].IsReady ? Color.green : Color.black;
					playerToIP.Add(1, addresses[0]);
					Player2.text = "Player 2: " + addresses[1];
					Player2.color = currentLobby.Players[1].IsReady ? Color.green : Color.black;
					playerToIP.Add(2, addresses[1]);
					break;
				case 3:
					Player1.text = "Player 1: " + addresses[0];
					Player1.color = currentLobby.Players[0].IsReady ? Color.green : Color.black;
					playerToIP.Add(1, addresses[0]);
					Player2.text = "Player 2: " + addresses[1];
					Player2.color = currentLobby.Players[1].IsReady ? Color.green : Color.black;
					playerToIP.Add(2, addresses[1]);
					Player3.text = "Player 3: " + addresses[2];
					Player3.color = currentLobby.Players[2].IsReady ? Color.green : Color.black;
					playerToIP.Add(3, addresses[2]);
					break;
				case 4:
					Player1.text = "Player 1: " + addresses[0];
					Player1.color = currentLobby.Players[0].IsReady ? Color.green : Color.black;
					playerToIP.Add(1, addresses[0]);
					Player2.text = "Player 2: " + addresses[1];
					Player2.color = currentLobby.Players[1].IsReady ? Color.green : Color.black;
					playerToIP.Add(2, addresses[1]);
					Player3.text = "Player 3: " + addresses[2];
					Player3.color = currentLobby.Players[2].IsReady ? Color.green : Color.black;
					playerToIP.Add(3, addresses[2]);
					Player4.text = "Player 4: " + addresses[3];
					Player4.color = currentLobby.Players[3].IsReady ? Color.green : Color.black;
					playerToIP.Add(4, addresses[3]);
					break;
			}			
		}
		private void UpdateChosenBattleText()
		{
			if (currentLobby == null)
				ChosenBattleText.text = "";
			else
				ChosenBattleText.text = "Battle №" + currentLobby.Id;
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
			battlePoller = StartCoroutine(StartPollingBattles());
			UpdatePlayersAddresses(new List<string>());
			UpdateChosenBattleText();
		}

		private void HideJoinBattleMenu()
		{
			if (currentLobby != null)
			{
				LeaveLobby(currentLobby.Id);
				currentLobby = null;
			}
			StopCoroutine(battlePoller);
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// Final function
		/// </summary>
		private void FinishMatchmaking()
		{
			FinalPlayerToIP = playerToIP;
			Network.DeleteLobby(currentLobby.Id);
			SceneManager.LoadScene("GameScene");
		}
	}
}