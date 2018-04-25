using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
	public class MainMenuController : MonoBehaviour
	{
		public CreateBattleController CreateBattle;
		public JoinBattleController JoinBattle;
		
		public Button CreateBtn;
		public Button JoinBtn;
		public Button LeaveBtn;

		// Use this for initialization
		private void Start()
		{
			CreateBtn.onClick.AddListener(OnCreateBtn);
			JoinBtn.onClick.AddListener(OnJoinBtn);
			LeaveBtn.onClick.AddListener(OnLeaveBtn);
		}

		private void OnCreateBtn()
		{
			CreateBattle.ShowCreateBattleMenu();
		}

		private void OnJoinBtn()
		{
			JoinBattle.ShowJoinBattleMenu();
		}

		private void OnLeaveBtn()
		{
			Application.Quit();
		}
	}
}