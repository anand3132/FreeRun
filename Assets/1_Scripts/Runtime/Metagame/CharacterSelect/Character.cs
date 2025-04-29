using Unity.Netcode;
using UnityEngine;

namespace RedGaint.Network.Runtime
{

    [CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
    public class Character : ScriptableObject
    {
        [SerializeField] private int id = -1;
        [SerializeField] private string displayName = "New Display Name";
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject introPrefab;
        [SerializeField] private NetworkObject gameplayPrefab;

        public int Id => id;
        
        public Transform stageOffset;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public GameObject IntroPrefab => introPrefab;
        public NetworkObject GameplayPrefab => gameplayPrefab;
    }
}