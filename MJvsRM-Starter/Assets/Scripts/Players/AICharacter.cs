using UnityEngine;
using System.Collections;

public class AICharacter : MonoBehaviour
{
    #region Variables
    //Nossos componentes
    StateManager states;
    public StateManager enStates;

    public float changeStateTolerance = 3;//Decide quando ataca o quao perto esta do player adversario

    public float normalRate = 1;//O quao rapido sera pra AI decide o estado 
    float nrmTimer;

    public float closeRate = 0.5f;//O quao rapido AI vai decidar fechar o close state
    float clTimer;

    public float blockingRate = 1.5f;//Por quanto tempo ele vai bloquear
    float blTimer;

    public float aiStateLife = 1; //Quanto tempo ira ter pra resetar o estado da AI
    float aiTimer;

    bool initiateAI;//Quando a AI ira rodar
    bool closeCombat;// Se estamos em um combat corpo a corpo

    bool gotRandom;
    float storeRandom;

    //Variaveis para bloquear
    bool checkForBlocking;
    bool blocking;
    float blockMultiplier;

    //Quantas vezes iremos atacar
    bool randomizeAttacks;
    int numberOfAttacks;
    int curNumAttacks;

    //Variaveis do pulo
    public float Jumprate = 1;
    float jRate;
    bool jump;
    float jTimer;
    #endregion

    public AttackPatterns[] attackPatterns;

    //Nossa AI states
    public enum AIState
    {
        closeState,
        normalState,
        resetAI
    }

    public AIState aIState;

    void Start()
    {
        states = GetComponent<StateManager>();
    }

    void Update()
    {

        //Todas as nossas funcoes
        CheckDistance();
        States();
        AIAgent();
    }
    //Todos os nossos states
    void States()
    {
        //Esse switch vai decidir quando rodar ou nao
        switch(aIState)
        {
            case AIState.closeState:
                CloseState();
                break;
            case AIState.normalState:
                NormalState();
                break;
            case AIState.resetAI:
                ResetAI();
                break;
        }


        Jumping();
    }


    //Essa funcao gerencia as coisas que agent precisa fazer
    void AIAgent()
    {
        //Se tem algo pra fazer, significa que ciclco da AI rodou tudo
        if(initiateAI)
        {
            //Comeca resetando processo da AI
            aIState = AIState.resetAI;
            //Cria um multiplier
            float multiplier = 0;

            //Pega nosso valor aleatorio
            if(!gotRandom)
            {
                storeRandom = ReturnRandom();
                gotRandom = true;
            }

            //Se nao estamos em combate corpo a corpo
            if(!closeCombat)
            {
                //Temos 30% chance de mover
                multiplier += 30;
            }
            else
            {
                //Temos mais 30% chance de atacar
                multiplier -= 30;
            }

            //Comparamos nossa random value junto com nossos modificador
            if(storeRandom + multiplier < 50)
            {
                Attack();
            }
            else
            {
                Movement();
            }
        }
    }
    void Attack()
    {
        
        //Pegamos nossa random value
        if(!gotRandom)
        {
            storeRandom = ReturnRandom();
            gotRandom = true;
        }


        //Temos 75% de chance em fazer um attack normal
        //if(storeRandom < 75)
        //{
            //Vemos quando ataque iremos fazer
        if(!randomizeAttacks)
        {
            //Pegamos uma random int entre 1 e 4
            numberOfAttacks = (int)Random.Range(1, 4);
            randomizeAttacks = true;
        }

        //Se nao temos como atacar mais entao maximiza o tempo
        if(curNumAttacks <numberOfAttacks)
        {
            int attackNumber = Random.Range(0, attackPatterns.Length);

            StartCoroutine(OpenAttack(attackPatterns[attackNumber], 0));


            //E incrementamos nas vezes que atacamos
            curNumAttacks++;
        }
    }
    void Movement()
    {
        //Pegamos um random value
        if(!gotRandom)
        {
            storeRandom = ReturnRandom();
            gotRandom = true;
        }

        //Temos 90% chance de movemos perto do inimigo
        if(storeRandom < 90)
        {
            if (enStates.transform.position.x < transform.position.x)
                states.horizontal = -1;
            else
                states.horizontal = 1;
        }
        else//Ou se afasta dele
        {
            if (enStates.transform.position.x < transform.position.x)
                states.horizontal = 1;
            else
                states.horizontal = -1;
        }
    }

    //Essa funcao reseta todas as nossas variaveis
    void ResetAI()
    {
        aiTimer += Time.deltaTime;

        if(aiTimer > aiStateLife)
        {
            initiateAI = false;
            states.horizontal = 0;
            states.vertical = 0;
            aiTimer = 0;

            gotRandom = false;

            //E tambem tem a chance de mudar AI state de normal state para close state para deixar mais aleatorio
            storeRandom = ReturnRandom();
            if (storeRandom < 50)
                aIState = AIState.normalState;
            else
                aIState = AIState.closeState;

            curNumAttacks = 1;
            randomizeAttacks = false;
        }
    }
    //Checa a distancia da nossa posicao e do inimogo e muda o state de acordo com ela
    void CheckDistance()
    {
        float distance = Vector3.Distance(transform.position, enStates.transform.position);

        //Compara com a nossa tolerance
        if(distance < changeStateTolerance)
        {
            if (aIState != AIState.resetAI)
                aIState = AIState.closeState;

            //Se estamos perto, entao combate corpo a corpo
            closeCombat = true;
        }
        else
        {
            //Se nao estamos em processa de resetar AI, entao muda o estado
            if (aIState != AIState.resetAI)
                aIState = AIState.normalState;

            //Se estamos perto do inimigo entamo vamos mover pra tras
            if(closeCombat)
            {
                //Pega um valor random
                if(!gotRandom)
                {
                    storeRandom = ReturnRandom();
                    gotRandom = true;
                }

                //E temos 60% chance do agente AI seguir o inimigo
                if(storeRandom < 60)
                {
                    Movement();
                }
            }

            //Provavelmente nao estamos mais corpo a corpo
            closeCombat = false;
        }

    }
    //Logica de bloquear vem aqui
    void Blockig()
    {
        //Se recebemos dano
        if(states.gettingHit)
        {
            //pega um valor random
            if(!gotRandom)
            {
                storeRandom = ReturnRandom();
                gotRandom = true;
            }

            //Temos 50% chance de AI bloquear
            if(storeRandom <50)
            {
                blocking = true;
                states.gettingHit = false;

            }
        }
        if(blocking)
        {
            blTimer += Time.deltaTime;

            if(blTimer > blockingRate)
            {
                
                blTimer = 0;
            }
        }
    }
    //Normal state AI decide o estado do ciclo
    void NormalState()
    {
        nrmTimer += Time.deltaTime;

        if(nrmTimer >normalRate)
        {
            initiateAI = true;
            nrmTimer = 0;
        }
    }
	//Close state AI decide o ciclo do estado
	void CloseState()
    {
        clTimer += Time.deltaTime;

        if(clTimer >closeRate)
        {
            clTimer = 0;
            initiateAI = true;
        }
    }
    void Jumping()
    {
        if(!enStates.onGround)
        {
            float ranValue = ReturnRandom();

            if(ranValue <50)
            {
                jump = true;
            }
        }
        if(jump)
        {
            states.vertical = 1;
            jRate = ReturnRandom();
            jump = false;
        }
        else
        {
            //Precisamos resetar a vertical input se nao vai ficar sempre pulando
            states.vertical = 0;
        }
        //Temos nosso tempo que determina quantos segundo ira rodar se quisemos
        jTimer += Time.deltaTime;

        if(jTimer >Jumprate*10)
        {
            
            //Temos 50% chance de pular ou nao
            if(jRate <50)
            {
                jump = true;
            }
            else
            {
                jump = false;
            }

            jTimer = 0;
        }

    }
    float ReturnRandom()
    {
        float retVal = Random.Range(0, 101);
        return retVal;
    }
    IEnumerator OpenAttack(AttackPatterns a, int i)
    {
        int index = i;
        float delay = a.attacks[index].delay;
        states.attack1 = a.attacks[index].attack1;
        states.attack2 = a.attacks[index].attack2;
        yield return new WaitForSeconds(delay);

        states.attack1 = false;
        states.attack2 = false;

        if(index < a.attacks.Length - 1)
        {
            index++;
            StartCoroutine(OpenAttack(a, index));
             
        }
    }
}

[System.Serializable]
public class AttackPatterns
{
    public AttackBase[] attacks;
}

[System.Serializable]
public class AttackBase
{
    public bool attack1;
    public bool attack2;
    public float delay;
}