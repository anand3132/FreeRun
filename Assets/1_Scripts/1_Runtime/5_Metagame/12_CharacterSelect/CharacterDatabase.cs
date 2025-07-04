using System.Linq;
using UnityEngine;

namespace RedGaint.Network.Runtime
{

    [CreateAssetMenu(fileName = "New Character Database", menuName = "Characters/Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private Character[] characters = new Character[0];

        public Character[] GetAllCharacters() => characters;

        public Character GetCharacterById(string id)
        {
            foreach (var character in characters)
            {
                if ( string .Equals(character.Id,id))
                {
                    return character;
                }
            }

            return null;
        }

        public bool IsValidCharacterId(string id)
        {
            return characters.Any(x => string.Equals(x.Id , id));
        }
    }
}