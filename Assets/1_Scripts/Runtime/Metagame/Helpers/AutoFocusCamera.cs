using Cinemachine;
using System.Collections.Generic;
using UnityEngine;


namespace RedGaint.Network.Runtime
{

    public class AutoFocusCinemachine : MonoBehaviour
    {
        [Header("References")]
        public CinemachineTargetGroup targetGroup;
        public Transform stageRoot; // Parent of all tables with characters

        [Header("Group Target Settings")]
        public float characterWeight = 1f;
        public float radius = 1.5f;

        private Dictionary<Transform, CinemachineTargetGroup.Target> activeTargets = new();

        public void RefreshTargetGroup()
        {
            List<CinemachineTargetGroup.Target> newTargets = new();

            foreach (Transform child in stageRoot)
            {
                Table table = child.GetComponent<Table>();
                if (table != null && table.currentCharacter != null)
                {
                    Transform character = table.currentCharacter.transform;

                    // Avoid duplicates
                    if (!activeTargets.ContainsKey(character))
                    {
                        var newTarget = new CinemachineTargetGroup.Target
                        {
                            target = character,
                            weight = characterWeight,
                            radius = radius
                        };
                        activeTargets[character] = newTarget;
                    }

                    newTargets.Add(activeTargets[character]);
                }
            }

            // Update target group
            targetGroup.m_Targets = newTargets.ToArray();
        }

        public void RemoveCharacterFromGroup(GameObject characterGO)
        {
            if (characterGO == null) return;

            if (activeTargets.Remove(characterGO.transform))
            {
                RefreshTargetGroup();
            }
        }
    }

}