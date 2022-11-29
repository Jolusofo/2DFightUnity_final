using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    WaitForSeconds oneSec;
    public Transform[] spawnPositions;//Posicao que os personagens irao nascer

    CameraManager camM;
    CharacterManager charM;
    LevelUI levelUI;//Os elementos visuais do game estao aqui pra ser acessado mais rapido

    public int maxTurns = 2;
    int currentTurn = 1;// O turno correspondende na tela, comeca no 1

    //Variaveis para o countdown
    public bool countdown;
    public int maxTurnTimer = 30;
    int currentTimer;
    float internalTimer;

    void Start() {
        //As referencias do singletons
        charM = CharacterManager.GetInstance();
        levelUI = LevelUI.GetInstance();
        camM = CameraManager.GetInstance();

        //Inicializar o WaitForSeconds
        oneSec = new WaitForSeconds(1);

        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        StartCoroutine("StartGame");

    }

	void FixedUpdate()
	{
		//Comparando o x do primeiro player, se eh menor que o inimigo fica na direita
        if(charM.players[0].playerStates.transform.position.x < 
           charM.players[1].playerStates.transform.position.x)
        {
            charM.players[0].playerStates.lookRight = true;
            charM.players[1].playerStates.lookRight = false;
        }
        else
        {
            charM.players[0].playerStates.lookRight = false;
            charM.players[1].playerStates.lookRight = true;
        }
	}
	void Update()
    {
        if(countdown)//Se habilitadamos o countdown
        {
            HandleTurnTimer();//controla o temporizador
        }
    }

    void HandleTurnTimer()
    {
        levelUI.LevelTimer.text = currentTimer.ToString();

        internalTimer += Time.deltaTime;//Cada um segundo

        if(internalTimer > 1)
        {
            currentTimer--;// Tira o tempo do round
            internalTimer = 0;
        }
        if(currentTimer <= 0)//Se countdown acabar
        {
            EndTurnFunction(true);//Acabe o turno
            countdown = false;
        }
    }

    IEnumerator StartGame()
    {
        //Quando comecamos pela primeira vez o jogo

        //Precisamos criar os jogadores primeiro
        yield return CreatePlayers();

        //Entao inicializa o round
        yield return InitTurn();
    }

    IEnumerator InitTurn()
    {
        //Para inicializar o turno

        //Desabilida o texto que anuncia o turno
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        //Reseta o contador
        currentTimer = maxTurnTimer;
        countdown = false;

        //E comeca a inicializar os players
        yield return InitPlayers();

        //E entao comeca o coroutine para habilitar os controles de cada jogador

        yield return EnableCountrol();

    }
    IEnumerator CreatePlayers()
    {

        //Vai pra todos os jogadores que temos na nossa lista
        for (int i = 0; i < charM.players.Count; i++)
        {
            //E instancia os prefabs
            GameObject go = Instantiate(charM.players[i].playerPrefab
                , spawnPositions[i].position, Quaternion.identity)
                as GameObject;

            //E precisamos pegar as referencias
            charM.players[i].playerStates = go.GetComponent<StateManager>();

            charM.players[i].playerStates.healthSlider = levelUI.healthSliders[i];

            camM.players.Add(go.transform);
        }

        yield return null;
    }

    IEnumerator InitPlayers()
    {
        //Agora, a unica coisa que precisamos fazer eh reiniciar a vida dos players
        for (int i = 0; i < charM.players.Count; i++)
        {
            charM.players[i].playerStates.health = 100;
            charM.players[i].playerStates.handleAnim.anim.Play("Locomotion");

            //Vai colocar o player no local que ele terminou
            charM.players[i].playerStates.transform.position = spawnPositions[i].position;

        }
        yield return null;
    }

    IEnumerator EnableCountrol()
    {

        //Comeca com texto que anuncia
        levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
        levelUI.AnnouncerTextLine1.text = "Turno " + currentTurn;
        levelUI.AnnouncerTextLine1.color = Color.white;
        yield return oneSec;
        yield return oneSec;

        //Muda a UI de texto e cor cada segundo que passa
        levelUI.AnnouncerTextLine1.text = "3";
        levelUI.AnnouncerTextLine1.color = Color.green;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "2";
        levelUI.AnnouncerTextLine1.color = Color.yellow;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "1";
        levelUI.AnnouncerTextLine1.color = Color.red;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "FIGHT!";
        levelUI.AnnouncerTextLine1.color = Color.red;

        //E para todo player habilitar que precisa ta aberto para controlar
        for (int i = 0; i < charM.players.Count; i++)
        {
            //Para o user players, habilitar o input handler por exemplo
            if(charM.players[i].playerType == PlayerBase.PlayerType.user)
            {
                InputHandler ih = charM.players[i].playerStates.gameObject.GetComponent<InputHandler>();
                ih.playerInput = charM.players[i].inputId;
                ih.enabled = true;
            }


            if (charM.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                AICharacter ai = charM.players[i].playerStates.gameObject.GetComponent<AICharacter>();
                ai.enabled = true;

                ai.enStates = charM.returnOppositePlater(charM.players[i]).playerStates;
            }
        }
        //Depois de um secundo, desabilitar o annoucer text
        yield return oneSec;
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        countdown = true;
    }

    void DisableControl()
    {
        // Para desabilitar os controles, voce precisa desabilitar os componetes que faz personagem
        for (int i = 0; i < charM.players.Count; i++)
        {
            //Precisa resetar as variaveis no State Manager
            charM.players[i].playerStates.ResetStateInputs();

            //Para os users players, esse input handle
            if(charM.players[i].playerType == PlayerBase.PlayerType.user)
            {
                charM.players[i].playerStates.GetComponent<InputHandler>().enabled = false;

            }
            if (charM.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                charM.players[i].playerStates.gameObject.GetComponent<AICharacter>().enabled = false;
            }
        }
    }

    public void EndTurnFunction(bool timeOut = false)
    {
        /*Eh uma funcao em que toda vez que quisemos terminar o turno
             * Mas precisamos saber se precisamos ir pelo timeout ou nao
             */
        countdown = false;
        //Reseta o timer text
        levelUI.LevelTimer.text = maxTurnTimer.ToString();

        //Se for timeout
        if (timeOut)
        {
            //Adiciona primeiro o text
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "Acabou o tempo!";
            levelUI.AnnouncerTextLine1.color = Color.cyan;
        }
        else
        {
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "K.O";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        //Desabilida os controles
        DisableControl();

        //E comeca a contar para final do turno
        StartCoroutine("EndTurn");
    }

    IEnumerator EndTurn()
    {
        //Espere 3 segundos pra limpar o text 
        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        //Encontre o player que ganhou

        PlayerBase vPlayer = FindWinningPlayer();

        if(vPlayer == null)// Se a nossa funcao retornar null
        {
            levelUI.AnnouncerTextLine1.text = "Empate!";
            levelUI.AnnouncerTextLine1.color = Color.blue;
        }
        else
        {
            //Se nao aquele jogador ganhou
            levelUI.AnnouncerTextLine1.text = vPlayer.playerId + " Ganhou!!";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        //Checa se o vencedor teve algum dano
        if (vPlayer != null)
        {
            //Se nao, entao ganhou um do caralho
            if (vPlayer.playerStates.health == 100)
            {
                levelUI.AnnouncerTextLine2.gameObject.SetActive(true);
                levelUI.AnnouncerTextLine2.text = "GG IZI DEMAIS BRO";
            }
        }
        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        currentTurn++;

        bool matchOver = isMatchOver();

        if(!matchOver)
        {
            StartCoroutine("InitTurn");
        }
        else
        {
            for (int i = 0; i < charM.players.Count; i++)
            {
                charM.players[i].score = 0;
                charM.players[i].hasCharacter = false;
            }

            if(charM.solo)
            {
                if (vPlayer == charM.players[0])
                {
                    MySceneManager.GetInstance().LoadNextOnProgression();
                }

                else
                {
                    MySceneManager.GetInstance().RequestLevelLoad(SceneType.main, "game_over");
                }
            }
            else
            {
                MySceneManager.GetInstance().RequestLevelLoad(SceneType.main, "intro");
            }
        }
    }

    bool isMatchOver()
    {
        bool retVal = false;

        for (int i = 0; i < charM.players.Count; i++)
        {
            if(charM.players[i].score >= maxTurns)
            {
                retVal = true;
                break;
            }
        }
        return retVal;
    }

    public PlayerBase FindWinningPlayer()
    {

        PlayerBase retVall = null;

        StateManager targetPlayer = null;

        //Checa primeiro se os dois players tem a mesma vida
        if(charM.players[0].playerStates.health != charM.players[1].playerStates.health)
        {
            //Se nao, entao checa quem tem a menor vida, o outro vira o ganhador
            if(charM.players[0].playerStates.health <charM.players[1].playerStates.health)
            {
                charM.players[1].score++;
                targetPlayer = charM.players[1].playerStates;
                levelUI.AddWinIndicator(1);
            }
            else
            {
                charM.players[0].score++;
                targetPlayer = charM.players[0].playerStates;
                levelUI.AddWinIndicator(0);
            }
            retVall = charM.returnPlayerFromStates(targetPlayer);
        }
        return retVall;
    }

    public static LevelManager instance;
    public static LevelManager GetInstance()
    {
        return instance;
    }
	void Awake()
	{
        instance = this;
	}
}
