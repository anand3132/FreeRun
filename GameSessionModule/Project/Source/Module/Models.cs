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
    
    public class ServerInfo
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
    }
    public class ServerAllocationResult
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string LobbyId { get; set; }
    }
}