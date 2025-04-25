// using System.Collections.Generic;
//
// namespace RedGaint.Network.Runtime
// {
//     public class LobbyModel : Model
//     {
//         // A list of all players currently in the lobby
//         public List<LobbyPlayerData> Players { get; private set; } = new List<LobbyPlayerData>();
//
//         // How many seconds left before game starts
//         public int CountdownSeconds { get; set; }
//
//         // Max number of players allowed in this lobby
//         public int MaxPlayers { get; set; } = 4;
//
//         // Optional: track if the local player is ready
//         public bool IsLocalPlayerReady { get; set; }
//
//         // Optional: selected map or mode
//         public string SelectedMap { get; set; } = "DefaultMap";
//
//         public void AddPlayer(LobbyPlayerData player)
//         {
//             if (!Players.Exists(p => p.PlayerId == player.PlayerId))
//             {
//                 Players.Add(player);
//             }
//         }
//
//         public void RemovePlayer(string playerId)
//         {
//             Players.RemoveAll(p => p.PlayerId == playerId);
//         }
//
//         public void ClearLobby()
//         {
//             Players.Clear();
//             CountdownSeconds = 0;
//             IsLocalPlayerReady = false;
//         }
//     }
// }