
namespace RedGaint.Network.Sandbox
{
    public enum ConnectStatus
    {
        Undefined,
        Success,
        ServerFull,
        LoggedInAgain,
        UserRequestedDisconnect,
        GenericDisconnect,
        Timeout
    }
}