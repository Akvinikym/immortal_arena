﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;
using Network;

namespace Arena
{
    public class GameController : MonoBehaviour
    {
        public List<PointController> FieldPoints;

        public LizardController Lizard;
        public WizardController Wizard;
        public KnightController Knight;
        public UnicornController Unicorn;

        public Network.NetworkManager NetManager;

        public AssignmentController AssController;

        private int playerNumber;
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

        private const int TimeForTurn = 20;
        private Coroutine timerCoroutine;

        private readonly Dictionary<IPlayer, int> turnSkipsInRow = new Dictionary<IPlayer, int>();

        // UI
        public UIController UiController;

        private void Start()
        {
            // Choose, who will be the first
            currentPlayer = Lizard;

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

            turnSkipsInRow.Add(Lizard, 0);
            turnSkipsInRow.Add(Wizard, 0);
            turnSkipsInRow.Add(Knight, 0);
            turnSkipsInRow.Add(Unicorn, 0);

            // Start game
            currentPlayer.SetActive();
            UiController.SetTurn(currentPlayer);
            timerCoroutine = StartCoroutine(StartTimer());

            NetManager.OnMoveHandler = (player, x, y) =>
            {
                var pos = FieldPoints[5 * x + y];
                playersPositions[allPlayers[player]] = pos;
                allPlayers[player].GetGameObject().transform.position = pos.gameObject.transform.position;
            };
            NetManager.OnTurnHandler = () =>
            {
                if (alivePlayers.Count == 1)
                {
                    UiController.FinishGame(alivePlayers.First());
                    StopAllCoroutines();
                    return;
                }

                currentPlayer = alivePlayers[(alivePlayers.IndexOf(currentPlayer) + 1) % alivePlayers.Count];
                UiController.SetTurn(currentPlayer);
                currentPlayerMoved = false;
                currentPlayer.SetActive();

                StopCoroutine(timerCoroutine);
                timerCoroutine = StartCoroutine(StartTimer());
            };

            NetManager.OnAttackHandler = (target) =>
            {
                // TODO: fix to support ranges and attack animation
                if (playersHealth[alivePlayers[target]] != 1)
                {
                    // Target is still alive
                    alivePlayers[target].Hit();
                    playersHealth[alivePlayers[target]] -= 1;
                    UiController.ReduceHealth(alivePlayers[target]);
                }
                else
                {
                    // Die
                    UiController.ReduceHealth(alivePlayers[target]);
                    UiController.KillPlayer(alivePlayers[target]);
                    KillPlayer(alivePlayers[target]);
                }
            };
            
            // TODO: uncomment
//            playerNumber = NetManager.GetPlayerNumber();
            playerNumber = 0;
        }

        private void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                // Player gives up the turn
                GiveUpTurn();
            }
            if (Input.GetKeyDown("return"))
            {
                Debug.Log("pressing enter");
                // Player gives up the turn
                NetManager.Connect();
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
            turnSkipsInRow[currentPlayer]++;
            // Some player skipped too much turns, kill him
            var index = alivePlayers.IndexOf(currentPlayer);
            var killPlayer = false;
            if (turnSkipsInRow[currentPlayer] >= 2)
            {
                KillPlayer(currentPlayer);
                UiController.KillPlayer(currentPlayer);
                killPlayer = true;
            }
            GiveUpTurn(false, killPlayer, index);
        }

        private void GiveUpTurn(bool respectOrder = true,
            bool playerWasTimeoutKilled = false, int timeoutKilledPlayerIndex = -1)
        {
            if (playerNumber != allPlayers.IndexOf(currentPlayer) && respectOrder)
            {
                Debug.Log("not your turn");
                return;
            }

            // Some player has won
            if (alivePlayers.Count == 1)
            {
                UiController.FinishGame(alivePlayers.First());
                StopAllCoroutines();
                return;
            }
            
            if (!AssController.ExamIsOver) AssController.StopExam();

            // Choose next player
            int nextPlayerIndex;
            if (timeoutKilledPlayerIndex >= 0 && playerWasTimeoutKilled)
            {
                nextPlayerIndex = timeoutKilledPlayerIndex == alivePlayers.Count ? 0 : timeoutKilledPlayerIndex;
            }
            else
            {
                var currentPlayerIndex = alivePlayers.IndexOf(currentPlayer);
                nextPlayerIndex = currentPlayerIndex == alivePlayers.Count - 1 ? 0 : currentPlayerIndex + 1;
            }

            currentPlayer = alivePlayers[nextPlayerIndex];

            currentPlayerMoved = false;
            currentPlayer.SetActive();

            UiController.SetTurn(currentPlayer);

            StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(StartTimer());

//			if (respectOrder) {
            NetManager.NextTurn();
//			}
        }

        private void KillPlayer(IPlayer target)
        {
            target.Hit();
            Destroy(target.GetGameObject());
            alivePlayers.Remove(target);
            playersPositions.Remove(target);
        }


        public void MovePlayer(PointController newPos)
        {
            if (playerNumber != allPlayers.IndexOf(currentPlayer))
            {
                Debug.Log("not your turn");
                return;
            }
            if (currentPlayerMoved) return;

            if (playersPositions.ContainsValue(newPos)) return;

            playersPositions[currentPlayer] = newPos;
            var pos = newPos.gameObject.transform.position;
            currentPlayer.GetGameObject().transform.position = new Vector3(pos.x, pos.y, 1);

            NetManager.Move(allPlayers.IndexOf(currentPlayer), newPos.XCoordinate, newPos.YCoordinate);

            currentPlayerMoved = true;
        }


        public void AttackPlayer(IPlayer target)
        {
            if (playerNumber != allPlayers.IndexOf(currentPlayer))
            {
                Debug.Log("not your turn");
                return;
            }

            if (ReferenceEquals(currentPlayer, target)) return;

            var attackerPos = playersPositions[currentPlayer];
            var targetPos = playersPositions[target];
            var currentPlayerIsRange = 
                ReferenceEquals(currentPlayer, Lizard) || ReferenceEquals(currentPlayer, Wizard);
            
            if (!currentPlayerIsRange && (
                    Math.Abs(attackerPos.XCoordinate - targetPos.XCoordinate) > 1 ||
                    Math.Abs(attackerPos.YCoordinate - targetPos.YCoordinate) > 1))
                return;
            
            // Start player's exam
            AssController.StartExam(currentPlayerIsRange);
            while (!AssController.ExamIsOver) {}
            if (!AssController.ExamIsSuccesfull)
            {
                GiveUpTurn();
                return;
            }

            currentPlayer.StartAttack();

            if (currentPlayerIsRange)
            {
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
            }
            else
            {
                switch (playersHealth[target])
                {
                    case 1:
                        // Die
                        KillPlayer(target);
                        playersHealth[target] -= 1;
                        UiController.ReduceHealth(target);
                        UiController.KillPlayer(target);
                        break;
                    case 2:
                        // Die
                        KillPlayer(target);
                        playersHealth[target] -= 2;
                        UiController.ReduceHealth(target);
                        UiController.ReduceHealth(target);
                        UiController.KillPlayer(target);
                        break;
                    default:
                        // Live
                        target.Hit();
                        playersHealth[target] -= 2;
                        UiController.ReduceHealth(target);
                        UiController.ReduceHealth(target);
                        break;
                }
            }

            NetManager.Attack(allPlayers.IndexOf(target));

            GiveUpTurn();
        }
    }
}