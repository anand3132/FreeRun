namespace RedGaint.Network.Runtime
{
    public class SessionQuery
    {
        
    }
    public class SessionResponse
    {
        public string LobbyId { get; set; }
        public string LobbyName { get; set; }
        public string Message { get; set; }
    }
    public class ServerAllocationResult
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string LobbyId { get; set; }
    }
}