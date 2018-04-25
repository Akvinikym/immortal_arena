using System;
using UnityEngine;

namespace Arena
{
	public class PointController : MonoBehaviour
	{
		private SpriteRenderer sprite;
		private GameController gameController;

		public int XCoordinate, YCoordinate;

		private void Start()
		{
			sprite = this.gameObject.GetComponent<SpriteRenderer>();
			gameController = this.gameObject.transform.parent.GetComponent<GameController>();

			var coordinates = this.gameObject.name.Split(',');
			XCoordinate = Convert.ToInt32(coordinates[0]);
			YCoordinate = Convert.ToInt32(coordinates[1]);
		}

		private void OnMouseOver()
		{
			sprite.enabled = true;
		}

		private void OnMouseExit()
		{
			sprite.enabled = false;
		}

		private void OnMouseDown()
		{
			gameController.MovePlayer(this);
		}
	}
}