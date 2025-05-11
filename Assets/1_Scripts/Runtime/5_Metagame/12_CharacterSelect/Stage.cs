using System.Collections.Generic;
using RedGaint.Utility;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public class Stage : Singleton<Stage>, IBugsBunny
    {
        [Header("Stage Settings")] public CharacterDatabase characterDatabase;
        public List<Table> tables = new List<Table>();
        public Transform stageCameraPosition;
        private Dictionary<int, GameObject> currentCharacters = new Dictionary<int, GameObject>();

        private Dictionary<int, int>
            currentCharacterIndexes =
                new Dictionary<int, int>(); // To keep track of character indexes for each table

        public bool LogThisClass { get; } = false;
        private GameObject stageRoot;
        private string turntablePrefabPath =GlobalStaticVariables.TurntablePrefabPath;

        public void Awake()
        {
           // LoadStage();
           LoadAllTables();

        }
        public void LoadStage()
        {
            if (stageRoot != null)
            {
                Debug.LogWarning("Stage already loaded.");
                return;
            }

            var prefab = Resources.Load<GameObject>(turntablePrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Turntable prefab not found at path: Resources/{turntablePrefabPath}");
                return;
            }

            stageRoot = Instantiate(prefab, stageCameraPosition.position, Quaternion.identity);
            tables.Clear();
            tables.AddRange(stageRoot.GetComponentsInChildren<Table>(true));
        }

        public void LoadAllTables()
        {
            for (int id = 0; id < tables.Count; id++)
            {
                tables[id].tableId = id;
            }
        }

        public void ShutdownStage()
        {
            foreach (var characterObj in currentCharacters.Values)
            {
                if (characterObj != null)
                {
                    Destroy(characterObj);
                }
            }

            currentCharacters.Clear();
            currentCharacterIndexes.Clear();
            tables.Clear();

            if (stageRoot != null)
            {
                Destroy(stageRoot);
                stageRoot = null;
            }
        }

        /// <summary>
        /// Shows a character on the given table. Replaces the previous character if any.
        /// </summary>

        public void ShowCharacterOnTable(int tableId, string characterId)
        {
            SetCharacterOnTable(tableId, characterId);
            ShowCharacterOnTable(tableId);
        }
        public void ShowCharacterOnTable(int tableId)
        {
            if (currentCharacters.TryGetValue(tableId, out GameObject charObj))
            {
                charObj.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"No character set for Table {tableId}");
            }
        }
        public void FocusCharacterOnTable(int tableId)
        {
            if(!TryGetTableById(tableId, out Table table))
               return;
            if (table == null)
            {
                BugsBunny.LogYellow($"Cannot focus on table {tableId}: not found.");
                return;
            }

            if (table.cameraFocusPoint != null)
            {
                Camera.main.transform.position = table.cameraFocusPoint.position;
                Camera.main.transform.rotation = table.cameraFocusPoint.rotation;
            }
            else
            {
                BugsBunny.LogYellow($"Table {tableId} has no camera focus point assigned.");
            }
        }
        public void FocusStage()
        {
            if (stageCameraPosition != null)
            {
                Camera.main.transform.position = stageCameraPosition.position;
                Camera.main.transform.rotation = stageCameraPosition.rotation;
            }
            else
            {
                BugsBunny.LogYellow("Stage camera position not set.");
            }
        }


        public void UpdateTableUserName(int tableId, string userName)
        {
            if (!TryGetTableById(tableId, out Table table))
            {
                BugsBunny.LogYellow($"Table with ID {tableId} not found.");
                return;
            }
            table.tableName.text = userName;
        }

        public bool TryGetCurrentCharacterOnStage(int tableId, out Character character)
        {
            character = null;

            if (currentCharacterIndexes.TryGetValue(tableId, out int characterIndex))
            {
                var characters = characterDatabase.GetAllCharacters();
                if (characterIndex >= 0 && characterIndex < characters.Length)
                {
                    character = characters[characterIndex];
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Show the next character in the character database on the specified table.
        /// </summary>
        public void ShowNextCharacterOnTable(int tableId)
        {
            if(!TryGetTableById(tableId, out Table table))
                return;
            if (table == null)
            {
                Debug.LogWarning($"Table with ID {tableId} not found.");
                return;
            }

            // Get the current index of the displayed character
            if (!currentCharacterIndexes.ContainsKey(tableId))
            {
                currentCharacterIndexes[tableId] = 0;
            }

            int currentIndex = currentCharacterIndexes[tableId];
            var characters = characterDatabase.GetAllCharacters();

            // If there are no characters, do nothing
            if (characters.Length == 0) return;

            // Calculate the index of the next character
            int nextIndex =
                (currentIndex + 1) % characters.Length; // Loop back to the first character after the last one

            // Show the next character
            ShowCharacterOnTable(tableId, characters[nextIndex].Id);

            // Update the index for the next character
            currentCharacterIndexes[tableId] = nextIndex;
        }

        /// <summary>
        /// Show the previous character in the character database on the specified table.
        /// </summary>
        public void ShowPreviousCharacterOnTable(int tableId)
        {
            if (!TryGetTableById(tableId, out Table table))
            {
                Debug.LogWarning($"Table with ID {tableId} not found.");
                return;
            }

            // Get the current index of the displayed character
            if (!currentCharacterIndexes.ContainsKey(tableId))
            {
                currentCharacterIndexes[tableId] = 0;
            }

            int currentIndex = currentCharacterIndexes[tableId];
            var characters = characterDatabase.GetAllCharacters();

            // If there are no characters, do nothing
            if (characters.Length == 0) return;

            // Calculate the index of the previous character
            int previousIndex =
                (currentIndex - 1 + characters.Length) %
                characters.Length; // Loop to the last character if at the first one

            // Show the previous character
            ShowCharacterOnTable(tableId, characters[previousIndex].Id);

            // Update the index for the previous character
            currentCharacterIndexes[tableId] = previousIndex;
        }

        /// <summary>
        /// Show a rotating selection of characters for character selection UI.
        /// Only on a specific table (e.g., selectionTableId).
        /// </summary>
        public void ShowAllCharactersInRotation(int tableId)
        {
            if (!TryGetTableById(tableId, out Table table))
            {
                Debug.LogWarning($"Table with ID {tableId} not found.");
                return;
            }
            // Clean up existing characters
            if (currentCharacters.TryGetValue(tableId, out GameObject oldChar) && oldChar != null)
            {
                Destroy(oldChar);
                currentCharacters.Remove(tableId);
            }

            var characters = characterDatabase.GetAllCharacters();
            if (characters.Length > 0)
            {
                var firstChar = Instantiate(characters[0].IntroPrefab, table.modelHook);
                currentCharacters[tableId] = firstChar;
                currentCharacterIndexes[tableId] = 0;
            }
        }

        /// <summary>
        /// Removes the current character from the specified table.
        /// </summary>
        public void RemoveCharacterFromTable(int tableId)
        {
            if (currentCharacters.TryGetValue(tableId, out GameObject charObj))
            {
                Destroy(charObj);
                currentCharacters.Remove(tableId);
                currentCharacterIndexes.Remove(tableId);
            }
        }

        private bool TryGetTableById(int tableId, out Table table)
        {
            table = tables.Find(t => t.tableId == tableId);
            return table != null;
        }

        public void SetCharacterOnTable(int tableId, string characterId)
        {
            var character = characterDatabase.GetCharacterById(characterId);
            if (character == null)
            {
                Debug.LogWarning($"Character with ID {characterId} not found.");
                return;
            }

            if (!TryGetTableById(tableId, out Table table))
            {
                Debug.LogWarning($"Table with ID {tableId} not found.");
                return;
            }
            if (currentCharacters.TryGetValue(tableId, out GameObject oldChar) && oldChar != null)
            {
                Destroy(oldChar);
                currentCharacters.Remove(tableId);
            }

            var newChar = Instantiate(character.IntroPrefab, table.modelHook);
            newChar.SetActive(false); // Do not show yet
            table.currentCharacter = newChar;
            table.characterID = characterId;

            currentCharacters[tableId] = newChar;

            if (!currentCharacterIndexes.ContainsKey(tableId))
                currentCharacterIndexes[tableId] = 0;
        }

        /// <summary>
        /// Returns the index of an available table that is not currently allocated.
        /// Returns -1 if no table is available.
        /// </summary>
        public int GetAvailableTable()
        {
            for (int i = 0; i < tables.Count; i++)
            {
                int tableId = tables[i].tableId;
                if (!currentCharacters.ContainsKey(tableId))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Clears all character models from all tables and resets their state.
        /// </summary>
        public void ClearAllTables()
        {
            foreach (var table in tables)
            {
                int tableId = table.tableId;

                // Destroy character GameObject if present
                if (currentCharacters.TryGetValue(tableId, out GameObject characterObj) && characterObj != null)
                {
                    Destroy(characterObj);
                }

                // Reset table-specific data
                table.currentCharacter = null;
                table.characterID = string.Empty;

                // Clear tracking
                currentCharacters.Remove(tableId);
                currentCharacterIndexes.Remove(tableId);
            }
        }

    } //Stage
    
} //RedGaint.Network.Runtime
