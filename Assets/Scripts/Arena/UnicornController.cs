using UnityEngine;
using System.Collections;

namespace Arena
{
	public class UnicornController : MonoBehaviour, IPlayer
	{
		public GameController Game;

		public SpriteRenderer Sprite;
		public Sprite SpriteIdle;
		public Sprite SpriteAttack;

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
			StartCoroutine(AttackingState());
		}
		private IEnumerator AttackingState()
		{
			Sprite.sprite = SpriteAttack;
			yield return new WaitForSeconds(1f);
			Sprite.sprite = SpriteIdle;
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

		public string GetStringName()
		{
			return "Unicorn";
		}

		private void OnMouseDown()
		{
			// the player was selected as target to hit
			Game.StartAttackPlayer(this);
		}
	}
}