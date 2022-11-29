using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
    public bool solo;
    public int numberOfUsers;
    public List<PlayerBase> players = new List<PlayerBase>();// Lista com todos os players e tipos de player

    //Uma lista que contem tudo que a gente precisa sobre cada personagem separado,
    //Por agora, contem id e seu correspondende prefab
    public List<CharacterBase> characterList = new List<CharacterBase>();



    //Nos usamos essa funcao pra encontrar os personagens pelo id deles
    public CharacterBase returnCharacterWithID(string id)
    {
        CharacterBase retVal = null;

        for (int i = 0; i < characterList.Count; i++)
        {
            if (string.Equals(characterList[i].charId, id))
            {
                retVal = characterList[i];
                break;
            }
        }

        return retVal;
    }

    //Nos usamos esse aqui pra retorna o player pelo personagem criado, states
    public PlayerBase returnPlayerFromStates(StateManager states)
    {
        PlayerBase retVal = null;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerStates == states)
            {
                retVal = players[i];
                break;
            }
        }
        return retVal;
    }
    public PlayerBase returnOppositePlater(PlayerBase pl)
    {
        PlayerBase retVall = null;

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i]!=pl)
            {
                retVall = players[i];
                break;
            }
        }
        return retVall;
    }

    public int ReturnCharacterInt(GameObject prefab)
    {
        int retVal = 0;

        for (int i = 0; i < characterList.Count; i++)
        {
            if(characterList[i].prefab == prefab)
            {
                retVal = i;
                break;
            }
        }
        return retVal;
    }

    public static CharacterManager instace;
    public static CharacterManager GetInstance()
    {
        return instace;
    }

    void Awake()
    {
        instace = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
    [System.Serializable]
    public class CharacterBase
    {
        public string charId;
        public GameObject prefab;
    }

    [System.Serializable]
    public class PlayerBase
    {
        public string playerId;
        public string inputId;
        public PlayerType playerType;
        public bool hasCharacter;
        public GameObject playerPrefab;
        public StateManager playerStates;
        public int score;

        public enum PlayerType
        {
            user,
            ai,
            simulation
        }
       
    }
