using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using _MessageType;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameState : MonoBehaviour
{
    const string Player1_Name = "PLAYER_1";
    const string Player2_Name = "PLAYER_2";
    [SerializeField] float MESSAGE_SEND_DELAY = 1.0f;

    //Real ingame objects
    Transform otherPlayer;
    Transform myPlayer;
    //Server -> Blue
    //Client -> Red

    //State Game
    [HideInInspector] public bool isGamePaused;
    
    float startResetHoldTime;
    float R_HOLDING_TIME = 3.0f;

    

    //DataTank dataTank;
    [SerializeField] GameObject canvasWin;
    [SerializeField] GameObject canvasEnd;

    [SerializeField] GameObject canvas;
    [SerializeField] GameObject canvasBarra;


    [SerializeField] TextMeshProUGUI pingText;
    [SerializeField] TextMeshProUGUI healthText;
  

    bool setColorRestart = false;
    bool inWin = false;
    bool inLose = false;
    bool isResetting = false;
    void Start()
    {
        isGamePaused = false;
        inWin = false;
        inLose = false;
        isResetting = false;
        startResetHoldTime = 0;


        //dataTank = FindAnyObjectByType<DataTank>();

        GetPlayers();



        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.POSITION] += MessagePosition;
        MessageManager.messageDistribute[MessageType.ANIMATIONSTATE] += MessageAnimation;
        MessageManager.messageDistribute[MessageType.KILL] += MessageKill;
        MessageManager.messageDistribute[MessageType.REVIVE] += MessageRevive;
        MessageManager.messageDistribute[MessageType.HITENEMY] += MessageHitEnemy;
        MessageManager.messageDistribute[MessageType.KILLENEMY] += MessageKillEnemy;
        MessageManager.messageDistribute[MessageType.SHOOT] += MessageShoot;
        MessageManager.messageDistribute[MessageType.HITPLAYER] += MessageHitPlayer;
        MessageManager.messageDistribute[MessageType.PAUSE] += MessagePause;
        MessageManager.messageDistribute[MessageType.UNPAUSE] += MessagePause;
        MessageManager.messageDistribute[MessageType.RESET] += MessageReset;
        MessageManager.messageDistribute[MessageType.WIN] += MessageWin;
        MessageManager.messageDistribute[MessageType.LOSE] += MessageLose;

        StartCoroutine(SendMyState());

        setColorRestart = true;
    }

    private void OnDestroy()
    {
        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.POSITION] -= MessagePosition;
        MessageManager.messageDistribute[MessageType.ANIMATIONSTATE] -= MessageAnimation;
        MessageManager.messageDistribute[MessageType.KILL] -= MessageKill;
        MessageManager.messageDistribute[MessageType.SHOOT] -= MessageShoot;
        MessageManager.messageDistribute[MessageType.REVIVE] -= MessageRevive;
        MessageManager.messageDistribute[MessageType.HITPLAYER] -= MessageHitPlayer;
        MessageManager.messageDistribute[MessageType.HITENEMY] -= MessageHitEnemy;
        MessageManager.messageDistribute[MessageType.KILLENEMY] -= MessageKillEnemy;
        MessageManager.messageDistribute[MessageType.PAUSE] -= MessagePause;
        MessageManager.messageDistribute[MessageType.UNPAUSE] -= MessagePause;
        MessageManager.messageDistribute[MessageType.RESET] -= MessageReset;
        MessageManager.messageDistribute[MessageType.PONG] -= HandlePong;
        MessageManager.messageDistribute[MessageType.WIN] -= MessageWin;
        MessageManager.messageDistribute[MessageType.LOSE] -= MessageLose;
    }

    void Update()
    {
        if(inLose || inWin)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                ResetGame();    
                MessageManager.SendMessage(MessageType.RESET);
               
            }
            return;
        }

        if (!inLose && !inWin)
        {
            if (!myPlayer.gameObject.activeSelf && !otherPlayer.gameObject.activeSelf)
            {
                MessageManager.SendMessage(MessageType.LOSE);
                Lose();
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SendPauseGame(!isGamePaused);
        }

        if (Input.GetKeyDown(KeyCode.Z)) {

            Win();
            MessageManager.SendMessage(MessageType.WIN); 
        
        }
        //Hold R
        if (Input.GetKeyDown(KeyCode.R)) startResetHoldTime = Time.time;
        if (Input.GetKey(KeyCode.R) && (Time.time - startResetHoldTime) >= R_HOLDING_TIME)
        {
            MessageManager.SendMessage(MessageType.RESET);
            ResetGame();
        }
        if (setColorRestart)
        {
           // dataTank.SetSettingsTanks();
            setColorRestart = false;
        }
    }

    void HandlePong(Message message)
    {
        //// Buscar el tiempo en que se envió el Ping correspondiente
        //PingMessage ping = message as PingMessage;

        //float sentTime = MessageManager.Find(m => m.id == message.id).time;
        //float rtt = Time.time - sentTime; // Tiempo total de ida y vuelta
        //float lag = rtt / 2; // Dividir por 2 para obtener el lag estimado

        //if (ping != null)
        //{
        //    float sentTime = ping.time;
        //    float rtt = Time.time - sentTime; // Tiempo de ida y vuelta
        //    float lag = rtt / 2; // Dividir por 2 para estimar el lag

        //    pingText.text = "Ip: + ";
        //    Debug.Log($"RTT: {rtt * 1000} ms, Lag: {lag * 1000} ms");
        //}
        //else
        //{
        //    Debug.LogWarning("No se encontró un mensaje Ping correspondiente.");
        //}
    }

    void MessagePosition(Message message)
    {
        Position p = message as Position;
        otherPlayer.position = p.pos;
        otherPlayer.rotation = Quaternion.Euler(0, p.rot,0);
        
    }

    void MessageAnimation(Message message)
    {
        AnimationStateMessage animation = message as AnimationStateMessage;
        if (animation.PlayerId != MessageManager.playerID) // Asegúrate de que no estás aplicando tu propia animación
        {
            otherPlayer.GetComponent<PlayerController>().animator.SetFloat("Horizontal", animation.Horizontal);
            otherPlayer.GetComponent<PlayerController>().animator.SetFloat("Vertical", animation.Vertical);
            
        }
    }

    void MessageRevive(Message message)
    {

        if (message.playerID != MessageManager.playerID)
        {
            myPlayer.gameObject.SetActive(true);
            myPlayer.GetComponent<PlayerController>().Revive();

        }


    }

    
    void MessageKill(Message message)
    {

        if (message.playerID != MessageManager.playerID)
        {

            otherPlayer.GetComponent<PlayerController>().Dead();
           
        }
            
       
    }

    void MessageKillEnemy(Message message)
    {

        KillEnemyMessage killEnemyMessage = message as KillEnemyMessage;

        if (killEnemyMessage.playerID != MessageManager.playerID)
        {

            EnemyManager.instance.KillEnemy(killEnemyMessage.EnemyID);
        }

    }
    void MessageHitEnemy(Message message)
    {

        HitEnemy hitEnemyMessage = message as HitEnemy;


        if (hitEnemyMessage.playerID != MessageManager.playerID)
        {
            EnemyManager.instance.HitEnemy(hitEnemyMessage.hitEnemyID);
        }


    }

    void MessageHitPlayer(Message message)
    {
        HitPlayer shootMessage = message as HitPlayer;

        // Verificar si el ID del jugador impactado coincide con el jugador local
        if (shootMessage.hitPlayerID == MessageManager.playerID)
        {
            Debug.Log("You've been hit!");

            myPlayer.GetComponent<PlayerController>().TakeDmg();

        }

    }

    void MessageShoot(Message message)
    {
        Shoot shootMessage = message as Shoot;

        // Verificar si el ID del jugador impactado coincide con el jugador local
        if (shootMessage.playerID != MessageManager.playerID)
        {
            

            otherPlayer.GetComponent<PlayerController>().Shoot(shootMessage.hitPoint);

        }
        else
        {
            Debug.Log($"Player {shootMessage.playerID} was hit!");
        }
    }

    void MessagePause(Message message)
    {
        //Pause or unpause
        SetPause(message.type == MessageType.PAUSE);
    }

    void MessageReset(Message message)
    {

            ResetGame();
            
    }
    void MessageWin(Message message)
    {
        Win();        
    }
    void MessageLose(Message message)
    {
        Lose();

    }

    void Win()
    {
        inWin = true;
        canvasWin.SetActive(true);
        canvas.SetActive(false);
        canvasBarra.SetActive(false);
    }

    void Lose()
    {
        if (inLose) return;
        inLose = true;
        canvasEnd.SetActive(true);
        canvas.SetActive(false);
        canvasBarra.SetActive(false);
    }
    void GetPlayers()
    {
        PlayerController[] ts = FindObjectsOfType<PlayerController>();
        foreach (PlayerController t in ts)
        {
            if (t.gameObject.name == Player1_Name)
            {
                if (MessageManager.playerID == 0)
                {
                    myPlayer = t.gameObject.transform;
                    t.SetPlayerId(0);
                   
                }
                else
                {
                    otherPlayer = t.gameObject.transform;
                    t.BlockMovement();

                    t.SetPlayerId(0);

                }
            }
            else if (t.gameObject.name == Player2_Name)
            {
                if (MessageManager.playerID == 1)
                {
                    myPlayer = t.gameObject.transform;
                    t.SetPlayerId(1);
                    
                }
                else
                {
                    otherPlayer = t.gameObject.transform;
                    t.BlockMovement();

                    t.SetPlayerId(1);
                }
            }
        }
    }

    public bool IsOtherTank(Transform t)
    {
        return t == otherPlayer;
    }

    IEnumerator SendMyState()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(MESSAGE_SEND_DELAY);
            MessageManager.SendMessage(new Position(myPlayer.position,
                myPlayer.rotation.eulerAngles.y));
        }
    }

    //
    //  INGAME FUNCTIONALITIES
    //

    public void SendPauseGame(bool pause)
    {
        SetPause(pause);
        //MessageManager.SendMessage(new Message(pause ? MessageType.PAUSE : MessageType.UNPAUSE));

        //if (pause)
        //{
        //    FindObjectOfType<BulletManager>().StopBullets();
        //}
        //else
        //{
        //    FindObjectOfType<BulletManager>().ReplayBullets();
        //}
    }

    void SetPause(bool pause)
    {
        
        isGamePaused = pause;
    }

    void KillGame()
    {
        //BulletScript[] bullets = FindObjectsOfType<BulletScript>();
        //foreach (BulletScript b in bullets)
        //{
        //    Destroy(b.gameObject);
        //}

        StopCoroutine(SendMyState());

    }

    public void ResetGame()
    {
        if (isResetting) return; // Evitar múltiples llamadas
        isResetting = true;
        KillGame();

        foreach (GameObject enemy in EnemyManager.instance.enemies)
        {
            Destroy(enemy); 
        }
        EnemyManager.instance.enemies.Clear();
        
        EnemyManager.instance.id = 0;
        EnemyManager.instance.removedEnemiesCount = 0;

        //Change the scene to loading scene     The same as this
        SceneManager.LoadScene("MainScene");
    }

}
