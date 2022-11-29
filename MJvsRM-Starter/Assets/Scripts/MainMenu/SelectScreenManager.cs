using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> plInterfaces = new List<PlayerInterfaces>();
    public PotraitInfo[] potraitPrefabs;//Todas as entradas do potrait
    public int maxX;//Quantos potraits a gente no X e no Y
    public int maxY;
    PotraitInfo[,] charGrid;//A grid que faz pra selecao entre eles

    public GameObject potraitCanvas;// As canvas que conte todos os potrait

    bool loadLevel; //Se esta carregando o level
    public bool bothPlayersSelected;

    CharacterManager charManager;

    #region Singleton
    public static SelectScreenManager instace;
    public static SelectScreenManager GetInstance()
    {
        return instace;
    }
    void Awake()
    {
        instace = this;
    }
    #endregion

    void Start()
    {
        //Inicia pegando as referencias do character manager
        charManager = CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;

        charManager.solo = (numberOfPlayers == 1);

        //E criamos o grid
        charGrid = new PotraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        potraitPrefabs = potraitCanvas.GetComponentsInChildren<PotraitInfo>();

        //Assim pegamos todos os potraits
        for (int i = 0; i < potraitPrefabs.Length; i++)
        {
            //E assim pegamos o grid position de cada potrait
            potraitPrefabs[i].posX += x;
            potraitPrefabs[i].posY += y;

            charGrid[x, y] = potraitPrefabs[i];

            if (x < maxX - 1)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }
    }
    void Update()
    {
        if (!loadLevel)
        {
            for (int i = 0; i < plInterfaces.Count; i++)
            {
                if (i < numberOfPlayers)
                {
                    if (Input.GetButtonUp("Fire2" + charManager.players[i].inputId ))
                    {
                        plInterfaces[i].playerBase.hasCharacter = false;
                    }
                    if (!charManager.players[i].hasCharacter)
                    {
                        plInterfaces[i].playerBase = charManager.players[i];


                        HandleSelectorPosition(plInterfaces[i]);
                        HandleSelectScreenInput(plInterfaces[i], charManager.players[i].inputId);
                        HandleCharacterPreview(plInterfaces[i]);

                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;
                }

            }

        }

            if (bothPlayersSelected)
            {
                Debug.Log("loading");
                StartCoroutine("LoadLevel");//E vai dar start do coroutine para carregar o level
                loadLevel = true;
            }
            else
            {
                
                if(charManager.players[0].hasCharacter && charManager.players[1].hasCharacter)
                {
                bothPlayersSelected = true;
                }
                
            }


    }
    void HandleSelectScreenInput(PlayerInterfaces pl, string playerId)
    {
        #region Grid Navigation
        /*Para navegar no grid
            *Simplesmente mudamos o active x e y para selectionar o que ativar
            *E tambem suaviza o input se voce continuar pressionando o botao
            *Nao vai mudar mais que um por meio segundo
        */

        float vertical = Input.GetAxis("Vertical" + playerId);

        if(vertical != 0)
        {
            if (!pl.hitInputOnce)
            { 
                if (vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxY - 1;
                }
                else
                {
                    pl.activeY = (pl.activeY < maxY - 1) ? pl.activeY + 1 : 0;
                }

                pl.hitInputOnce = true;
            }
        }
        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if(horizontal != 0)
        {
            if(!pl.hitInputOnce)
            {
                if(horizontal >0)
                {
                    pl.activeX = (pl.activeX > 0) ? pl.activeX - 1 : maxX - 1;
                }
                else
                {
                    pl.activeX = (pl.activeX < maxX - 1) ? pl.activeX + 1 : 0;
                }
                pl.timerToReset = 0;
                pl.hitInputOnce = true;
            }
        }

        if(vertical == 0 && horizontal == 0)
        {
            pl.hitInputOnce = false;
        }
        if(pl.hitInputOnce)
        {
            pl.timerToReset += Time.deltaTime;

            if (pl.timerToReset > 0.8f)
            {
                pl.hitInputOnce = false;
                pl.timerToReset = 0;
            }
        }            
    #endregion
        if (Input.GetButtonUp("Fire1" + playerId))
        {
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");

            pl.playerBase.playerPrefab = charManager.returnCharacterWithID(pl.activePotrait.characterId).prefab;

            pl.playerBase.hasCharacter = true;
        }
        //Se meu jogador 1 selecionar o seu personagem, entao o segundo jogador ira ser selecionado
        if (charManager.players[0].hasCharacter == true)
        {
            if (charManager.players[1].hasCharacter == false)
            {
                pl.playerBase.playerPrefab = charManager.returnCharacterWithID(pl.activePotrait.characterId).prefab;
                pl.playerBase.hasCharacter = true;
                pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");


            }
        }
    }
    IEnumerator LoadLevel()
    {

        //Se um dos jogadores for AI, entao  escolhe qualquer personagem do prefab
        for (int i = 0; i < charManager.players.Count; i++)
        {
            if (charManager.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                if (charManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, potraitPrefabs.Length);

                    charManager.players[i].playerPrefab = charManager.returnCharacterWithID(potraitPrefabs[ranValue].characterId).prefab;

                    Debug.Log(potraitPrefabs[ranValue].characterId);
                }
            }
        }
        yield return new WaitForSeconds(2);// depois de dois segundos vai carregar o level

        if(charManager.solo)
        {
            MySceneManager.GetInstance().CreateProgression();
            MySceneManager.GetInstance().LoadNextOnProgression();
        }
        else
        {
            MySceneManager.GetInstance().RequestLevelLoad(SceneType.prog, "level_1");
        }
    }

    void HandleSelectorPosition(PlayerInterfaces pl)
    {
        pl.selector.SetActive(true); //Habilitado o selector

        pl.activePotrait = charGrid[pl.activeX, pl.activeY]; //Encontra o potrait ativado

        //E coloca o seletor em cima de uma posicao
        Vector2 selectorPosition = pl.activePotrait.transform.localPosition;
        selectorPosition = selectorPosition + new Vector2(potraitCanvas.transform.localPosition.x
                                                          , potraitCanvas.transform.localPosition.y);

        pl.selector.transform.localPosition = selectorPosition;
    }


    void HandleCharacterPreview(PlayerInterfaces pl)
    {
        //Se o previews potrait tivemos, nao eh o mesmo que esta ativa uma que temos
        //Entao significa que precisa trocar de personagem

        if(pl.previewPotrait != pl.activePotrait)
        {
            if(pl.createdCharacter != null)// Deleta o anterior
            {
                Destroy(pl.createdCharacter);
            }

            //E cria outro
            GameObject go = Instantiate (
                CharacterManager.GetInstance().returnCharacterWithID(pl.activePotrait.characterId).prefab,
                pl.charVisPos.position,
                Quaternion.identity) as GameObject;

            pl.createdCharacter = go;

            pl.previewPotrait = pl.activePotrait;


            if(!string.Equals(pl.playerBase.playerId, charManager.players[0].playerId))
            {
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }

        }
    }

    [System.Serializable]
    public class PlayerInterfaces
    {
        public PotraitInfo activePotrait;// A corrent que ativa potrait pro player 1
        public PotraitInfo previewPotrait;
        public GameObject selector;//Que seleciona o indicador do player 1
        public Transform charVisPos;//Vizualiza a posicao do player 1
        public GameObject createdCharacter;// Cria o personagem do player 1

        public int activeX;//Ativa o X e o Y pro player 1
        public int activeY;

        //Algumas variaveis pro alguns inputs
        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;

    }

}
