using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	
	// Health bars
	public List<GameObject> LizardHealth = new List<GameObject>();
	public List<GameObject> WizardHealth = new List<GameObject>();
	public List<GameObject> KnightHealth = new List<GameObject>();
	public List<GameObject> UnicornHealth = new List<GameObject>();
	
	public Text LizardText;
	public Text WizardText;
	public Text KnightText;
	public Text UnicornText;

	public Text CurrentTurn;
	public Text Timeout;

	public GameObject WinScript;
	public Text WinMessage;
	public Button LeaveGameBtn;

	private const string MainMenuSceneName = "MainMenuScene";

	private void Start()
	{
		LeaveGameBtn.onClick.AddListener(OnLeaveGameBtnBlick);
	}
	
	public void ReduceHealth(IPlayer target)
	{
		switch (target.GetStringName())
		{
			case "Lizard":
			{
				Destroy(LizardHealth.Last());
				LizardHealth.RemoveAt(LizardHealth.Count - 1);
				break;
			}
			case "Wizard":
			{
				Destroy(WizardHealth.Last());
				WizardHealth.RemoveAt(WizardHealth.Count - 1);
				break;
			}
			case "Knight":
			{
				Destroy(KnightHealth.Last());
				KnightHealth.RemoveAt(KnightHealth.Count - 1);
				break;
			}
			case "Unicorn":
			{
				Destroy(UnicornHealth.Last());
				UnicornHealth.RemoveAt(UnicornHealth.Count - 1);
				break;
			}
		}
	}

	public void KillPlayer(IPlayer target)
	{
		switch (target.GetStringName())
		{
			case "Lizard":
			{
				LizardText.color = Color.red;
				break;
			}
			case "Wizard":
			{
				WizardText.color = Color.red;
				break;
			}
			case "Knight":
			{
				KnightText.color = Color.red;
				break;
			}
			case "Unicorn":
			{
				UnicornText.color = Color.red;
				break;
			}
		}
	}

	public void SetTurn(IPlayer next)
	{
		CurrentTurn.text = string.Format("{0}'s\nturn!", next.GetStringName());
	}

	public void UpdateTimer(int leftTime)
	{
		Timeout.text = string.Format("Time left: {0}", leftTime);
	}

	public void FinishGame(IPlayer winner)
	{
		WinMessage.text = string.Format("{0}\nhas won the battle!", winner.GetStringName());
		WinScript.SetActive(true);
	}

	private void OnLeaveGameBtnBlick()
	{
		SceneManager.LoadScene(MainMenuSceneName);
	}
}
