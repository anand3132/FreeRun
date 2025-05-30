using Unity.Netcode;

namespace RedGaint.Network.Runtime
{
    internal interface ICharacter
    {
        NetworkObject NetworkObject { get; }
    }
}
