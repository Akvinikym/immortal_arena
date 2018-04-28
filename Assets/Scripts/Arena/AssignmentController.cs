using System;
using UnityEngine;
using UnityEngine.UI;

namespace Arena
{
	public class AssignmentController : MonoBehaviour
	{
		public Text Assignment;
		public InputField Input;

		public bool ExamIsOver;
		public bool ExamIsSuccesfull;

		private static readonly System.Random rand = new System.Random();
		private int rightAnswer;

		private void Start()
		{
			Input.onEndEdit.AddListener(CheckAnswer);
		}

		public void StartExam(bool targetIsRange)
		{
			ExamIsOver = false;
			ExamIsSuccesfull = false;
		
			// Generate task; simpler for ranges, harder for melees
			if (targetIsRange)
			{
				var x = rand.Next(2, 9);
				var y = rand.Next(12, 29);
				Assignment.text = string.Format("{0} * {1}", x, y);
				rightAnswer = x * y;
			}
			else
			{
				var x = rand.Next(11, 19);
				var y = rand.Next(11, 19);
				var z = rand.Next(11, 29);
				Assignment.text = string.Format("{0} * {1} + {2}", x, y, z);
				rightAnswer = x * y + z;
			}
		
			this.gameObject.SetActive(true);
		}

		private void CheckAnswer(string answer)
		{
			if (answer == "") return;
			ExamIsOver = true;
			ExamIsSuccesfull = Convert.ToInt32(answer) == rightAnswer;
		}

		public void StopExam()
		{
			this.gameObject.SetActive(false);
		}
	}
}
