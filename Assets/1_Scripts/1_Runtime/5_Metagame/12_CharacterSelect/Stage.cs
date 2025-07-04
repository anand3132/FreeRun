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

        public Dictionary<int, int>
            currentCharacterIndexes =
                new Dictionary<int, int>(); // To keep track of character indexes for each table

        public bool LogThisClass => false;
        private GameObject stageRoot;
        private string turntablePrefabPath =GlobalStaticVariables.TurntablePrefabPath;

        public void Awake()
        {
           // LoadStage();
           BugsBunnyLogger.Log("Stage Awake",this);
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
                BugsBunnyLogger.LogYellow($"Cannot focus on table {tableId}: not found.");
                return;
            }

            if (table.cameraFocusPoint != null)
            {
                Camera.main.transform.position = table.cameraFocusPoint.position;
                Camera.main.transform.rotation = table.cameraFocusPoint.rotation;
            }
            else
            {
                BugsBunnyLogger.LogYellow($"Table {tableId} has no camera focus point assigned.");
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
                BugsBunnyLogger.LogYellow("Stage camera position not set.");
            }
        }


        public void UpdateTableUserName(int tableId, string userName)
        {
            if (!TryGetTableById(tableId, out Table table))
            {
                BugsBunnyLogger.LogYellow($"Table with ID {tableId} not found.");
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
        public void ShowNextCharacterOnTable(int tableId, string characterId)
        {
            if (!TryGetTableById(tableId, out Table table))
                return;

            var characters = characterDatabase.GetAllCharacters();
            if (characters.Length == 0) return;

            // Find index of the current character ID
            int currentIndex = System.Array.FindIndex(characters, c => c.Id == characterId);
            if (currentIndex == -1)
            {
                Debug.LogWarning($"Character ID '{characterId}' not found in database. Defaulting to index 0.");
                currentIndex = 0;
            }

            // Calculate next index and update state
            int nextIndex = (currentIndex + 1) % characters.Length;
            currentCharacterIndexes[tableId] = nextIndex;

            // Show the next character
            ShowCharacterOnTable(tableId, characters[nextIndex].Id);
        }



        /// <summary>
        /// Show the previous character in the character database on the specified table.
        /// </summary>
        public void ShowPreviousCharacterOnTable(int tableId,string characterId)
        {
            if (!TryGetTableById(tableId, out Table table))
            {
                Debug.LogWarning($"Table with ID {tableId} not found.");
                return;
            }

            var characters = characterDatabase.GetAllCharacters();
            if (characters.Length == 0) return;

            // Determine current index using the table's character ID
            int currentIndex = System.Array.FindIndex(characters, c => c.Id == characterId);
            if (currentIndex == -1)
            {
                Debug.LogWarning($"Current character ID '{table.characterID}' not found. Defaulting to index 0.");
                currentIndex = 0;
            }

            // Calculate previous index with wrap-around
            int previousIndex = (currentIndex - 1 + characters.Length) % characters.Length;

            // Store index and show character
            currentCharacterIndexes[tableId] = previousIndex;
            ShowCharacterOnTable(tableId, characters[previousIndex].Id);
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
