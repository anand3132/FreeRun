using System.Collections.Generic;
using RedGaint.Network.Runtime.UserData;

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
        
        public List<PlayerSummary> Players {get;set;}
    }
    public class ServerAllocationResult
    {
        public string AllocationId{ get; set; }
        public long ServerId{ get; set; }
        public string Ipv4{ get; set; }
        public string Ipv6{ get; set; }
        public int GamePort{ get; set; }
        public string LobbyId{ get; set; }
    }
}