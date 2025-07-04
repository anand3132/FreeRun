// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections.Generic;
// using RedGaint.Network.Runtime;
//
// namespace RedGaint.Network.Runtime
// {
//     public class WaitingAreaView : View<MetagameApplication>
//     {
//         [SerializeField] private Button cancelButton;
//         [SerializeField] private TextMeshProUGUI timerText;
//
//         private float timer;
//         private bool trackingTime;
//
//         private void OnEnable()
//         {
//             cancelButton.onClick.AddListener(OnCancel);
//         }
//
//         private void OnDisable()
//         {
//             cancelButton.onClick.RemoveListener(OnCancel);
//         }
//
//         public void Show()
//         {
//             Show();
//             timer = 0;
//             trackingTime = true;
//             timerText.text = "Searching for match...";
//         }
//
//         public async void Hide()
//         {
//             trackingTime = false;
//             base.Hide();
//
//             // Ensure we stop searching when hiding the view
//             await MetagameApplication.Instance.MatchmakerTicketer.StopSearch();
//         }
//
//         private void Update()
//         {
//             if (!trackingTime) return;
//
//             timer += Time.deltaTime;
//             timerText.text = $"Searching... {Mathf.FloorToInt(timer)}s";
//         }
//
//         private void OnCancel()
//         {
//             Hide();
//             BaseApplication.Instance.ShowMainMenu(); // or any method that returns to the main view
//         }
//
//         public void OnMatchmakerTicked(int seconds)
//         {
//             timerText.text = $"Searching... {seconds}s";
//         }
//
//         public void OnMatchFound(MultiplayAssignment assignment, List<(string playerId, int characterId)> players)
//         {
//             if (assignment == null)
//             {
//                 timerText.text = "Matchmaking failed. Returning...";
//                 Debug.LogWarning("Matchmaking failed or timed out.");
//                 Invoke(nameof(ReturnToMainMenu), 2f); // delay to show the message
//                 return;
//             }
//
//             trackingTime = false;
//
//             // Proceed to next screen, e.g., Lobby or GameplaySceneLoader
//             Debug.Log("Match found. Transitioning to next scene...");
//             BaseApplication.Instance.StartGame(assignment, players);
//         }
//
//         private void ReturnToMainMenu()
//         {
//             Hide();
//                 ShowMainMenu();
//         }
//     }
// }
