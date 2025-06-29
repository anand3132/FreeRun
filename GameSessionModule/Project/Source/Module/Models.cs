using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class SessionRequest
    {
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public string PlayerName { get; set; }
        public int Xp { get; set; }
    }

    public class SessionResponse
    {
        public string LobbyId { get; set; }
        public string LobbyName { get; set; }
        public string Message { get; set; }
    }

    public class LobbyRequest
    {
        public string lobbyId { get; set; }
    }

    public class PlayerSummary
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string CharacterId { get; set; }
    }

    public class AllocationResponse
    {
        [JsonProperty("allocationId")] public string AllocationId { get; set; }

        [JsonProperty("href")] public string Href { get; set; }
    }

    public class ServerAllocationResult
    {
        [JsonProperty("allocationId")]
        public string AllocationId { get; set; }
        
        [JsonProperty("serverId")]
        public long ServerId { get; set; }

        [JsonProperty("ipv4")]
        public string Ipv4 { get; set; }

        [JsonProperty("ipv6")]
        public string Ipv6 { get; set; }
        
        [JsonProperty("gamePort")]
        public int GamePort { get; set; }
        public string LobbyId { get; set; }
    }
    //Create Alloaction
    public class AllocationPayload
    {
        [JsonProperty("lobbyId")]
        public string LobbyId { get; set; }

        [JsonProperty("players")]
        public List<Player> Players { get; set; }
    }

    public class AllocationRequest
    {
        public string allocationId { get; set; }
    
        public int buildConfigurationId { get; set; }
    
        public string payload { get; set; }
    
        public string regionId { get; set; }
    
        public bool restart { get; set; }
    }
//-------------------------------------------------
    public class MultiplayAllocationInfo
    {
        public string AllocationId { get; set; }
        public int BuildConfigurationId { get; set; }
        public string Created { get; set; }
        public string FleetId { get; set; }
        public int GamePort { get; set; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
        public int MachineId { get; set; }
        public bool Readiness { get; set; }
        public string RegionId { get; set; }
        public string RequestId { get; set; }
        public string Requested { get; set; }
        public int ServerId { get; set; }
    }
}