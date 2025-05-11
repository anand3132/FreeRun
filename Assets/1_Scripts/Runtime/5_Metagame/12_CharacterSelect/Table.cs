using TMPro;
using UnityEngine;
namespace RedGaint.Network.Runtime
{
    [System.Serializable]
    public class Table:MonoBehaviour
    {
        public int tableId;
        public TextMeshProUGUI tableName;
        public Transform modelHook;
        [HideInInspector] public GameObject currentCharacter;
        [HideInInspector] public string characterID;
        public Vector3 tablePosition;
        public Transform cameraFocusPoint;
    }

}