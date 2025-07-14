#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FS_ThirdPerson
{
    public partial class FSSystemsSetup
    {
        public static FSSystemInfo ParkourAndClimbingSystemSetup = new FSSystemInfo
        (
            characterType: CharacterType.Player,
            enabled: false,
            name: "Parkour And Climbing System",
            prefabName: "Parkour Controller",
            welcomeEditorShowKey: "ParkourAndClimbingSystem_WelcomeWindow_Opened",
            systemProjectSettings: new SystemProjectSettingsData
            (
                 layers: new List<string> { "Ledge" },
                 tags: new List<string>() { "NarrowBeam", "SwingableLedge" }
            ),

            mobileControllerPrefabName: "Parkour Mobile Controller"
        );

        static string ParkourAndClimbingSystemWelcomeEditorKey => ParkourAndClimbingSystemSetup.welcomeEditorShowKey;


        [InitializeOnLoadMethod]
        public static void LoadParkourAndClimbingSystem()
        {
            if (!string.IsNullOrEmpty(ParkourAndClimbingSystemWelcomeEditorKey) && !EditorPrefs.GetBool(ParkourAndClimbingSystemWelcomeEditorKey, false))
            {
                SessionState.SetBool(welcomeWindowOpenKey, false);
                EditorPrefs.SetBool(ParkourAndClimbingSystemWelcomeEditorKey, true);
                FSSystemsSetupEditorWindow.OnProjectLoad();
            }
        }
    }
}
#endif