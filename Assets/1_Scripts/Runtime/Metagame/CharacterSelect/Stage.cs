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
        private Dictionary<string, GameObject> currentCharacters = new Dictionary<string, GameObject>();

        private Dictionary<string, int>
            currentCharacterIndexes =
                new Dictionary<string, int>(); // To keep track of character indexes for each table

        public bool LogThisClass { get; } = false;
        private GameObject stageRoot;
        private string turntablePrefabPath =GlobalStaticVariables.TurntablePrefabPath;
        
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
        public void ShowCharacterOnTable(string tableId, string characterId)
        {
            var character = characterDatabase.GetCharacterById(characterId);
            if (character == null)
            {
                Debug.LogWarning($"Character with ID {characterId} not found.");
                return;
            }

            var table = GetTableById(tableId);
            if (table == null)
            {
                Debug.LogWarning($"Table with ID {tableId} not found.");
                return;
            }

            // Remove previous character if exists
            if (currentCharacters.TryGetValue(tableId, out GameObject oldChar) && oldChar != null)
            {
                Destroy(oldChar);
                currentCharacters.Remove(tableId);
            }

            // Instantiate the new character's intro prefab on the table's model hook
            var newChar = Instantiate(character.IntroPrefab, table.modelHook);
            currentCharacters[tableId] = newChar;

            // Track the index of the current character
            if (!currentCharacterIndexes.ContainsKey(tableId))
            {
                currentCharacterIndexes[tableId] = 0;
            }
        }

        public void UpdateTableUserName(string tableId, string userName)
        {
            var table = GetTableById(tableId);
            table.tableName.text = userName;
        }

        public Character GetCurrentCharacterOnStage(string tableId)
        {
            if (currentCharacterIndexes.TryGetValue(tableId, out int characterIndex))
            {
                var characters = characterDatabase.GetAllCharacters();
                if (characterIndex >= 0 && characterIndex < characters.Length)
                {
                    return characters[characterIndex];
                }
            }

            return null; // No character currently tracked for this table
        }

        /// <summary>
        /// Show the next character in the character database on the specified table.
        /// </summary>
        public void ShowNextCharacterOnTable(string tableId)
        {
            var table = GetTableById(tableId);
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
        public void ShowPreviousCharacterOnTable(string tableId)
        {
            var table = GetTableById(tableId);
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
        public void ShowAllCharactersInRotation(string tableId)
        {
            var table = GetTableById(tableId);
            if (table == null)
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
        public void RemoveCharacterFromTable(string tableId)
        {
            if (currentCharacters.TryGetValue(tableId, out GameObject charObj))
            {
                Destroy(charObj);
                currentCharacters.Remove(tableId);
                currentCharacterIndexes.Remove(tableId);
            }
        }

        private Table GetTableById(string tableId)
        {
            return tables.Find(t => t.tableId == tableId);
        }
    } //Stage


} //RedGaint.Network.Runtime
