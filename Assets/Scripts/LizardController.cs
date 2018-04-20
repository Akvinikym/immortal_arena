using UnityEngine;
using System.Collections;

public class LizardController : MonoBehaviour, IPlayer
{
	public GameController Game;

	public SpriteRenderer Sprite;

	private void Start()
	{
	}

	public void StartMove()
	{
		
	}

	public void StopMove()
	{
		
	}

	public void StartAttack()
	{
		
	}

	public void StopAttack()
	{
		
	}
	
	public GameObject GetGameObject()
	{
		return this.gameObject;
	}

	public void SetActive()
	{
		StartCoroutine(BlinkGreen());
	}
	private IEnumerator BlinkGreen()
	{
		Sprite.color = Color.green;
		yield return new WaitForSeconds(0.5f);
		Sprite.color = Color.white;
	}
	
	public void Hit()
	{
		StartCoroutine(BlinkRed());
	}
	private IEnumerator BlinkRed()
	{
		Sprite.color = Color.red;
		yield return new WaitForSeconds(0.5f);
		Sprite.color = Color.white;
	}
	
	private void OnMouseDown()
	{
		// the player was selected as target to hit
		Game.AttackPlayer(this);
	}
}
