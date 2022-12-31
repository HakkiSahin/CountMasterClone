using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Transform player;
    private int numberOfStickmans,numberOfEnemyStickmans;
    [SerializeField] private TextMeshPro CounterTxt;
    [SerializeField] private GameObject stickMan;
    //****************************************************

   [Range(0f,1f)] [SerializeField] private float DistanceFactor, Radius;
   
   //*********** move the player ********************
   
   public bool moveByTouch,gameState;
   private Vector3 mouseStartPos,playerStartPos;
   public float playerSpeed,roadSpeed;
   private Camera camera;

   [SerializeField] private Transform road;
   [SerializeField] private Transform enemy;
   private bool attack;
   public static PlayerManager PlayerManagerInstance;
   public ParticleSystem blood;
   public GameObject SecondCam;
   public bool FinishLine,moveTheCamera;

   public GameObject LosePanel, winPanel;
    void Start()
    {
        Time.timeScale = 1f;
        player = transform;
        
        numberOfStickmans = transform.childCount - 1;

        CounterTxt.text = numberOfStickmans.ToString();
        
        camera = Camera.main;

        PlayerManagerInstance = this;

        gameState = false;
    }
    
    void Update()
    {
        if (int.Parse(CounterTxt.text) <= 0)
        {
            Time.timeScale = 0;
            LosePanel.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            attack = false;
        }
        if (attack)
        {
            var enemyDirection = new Vector3(enemy.position.x,transform.position.y,enemy.position.z) - transform.position;

            for (int i = 1; i < transform.childCount; i++)
            {
                transform.GetChild(i).rotation = 
                    Quaternion.Slerp( transform.GetChild(i).rotation,Quaternion.LookRotation(enemyDirection,Vector3.up), Time.deltaTime * 3f );
            }

            if (enemy.GetChild(1).childCount > 1)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var Distance = enemy.GetChild(1).GetChild(0).position - transform.GetChild(i).position;

                    if (Distance.magnitude < 1.5f)
                    {
                        transform.GetChild(i).position = Vector3.Lerp(transform.GetChild(i).position, 
                            new Vector3(enemy.GetChild(1).GetChild(0).position.x,transform.GetChild(i).position.y,
                                enemy.GetChild(1).GetChild(0).position.z), Time.deltaTime * 1f );
                    }
                }
            }
            else
            {
                attack = false;
                roadSpeed = 2f;
                
                FormatStickMan();
                
                for (int i = 1; i < transform.childCount; i++)
                    transform.GetChild(i).rotation = Quaternion.identity;
                
               
                enemy.gameObject.SetActive(false);
              
            }

            if (transform.childCount == 1)
            {
                enemy.transform.GetChild(1).GetComponent<enemyManager>().StopAttacking();
                gameObject.SetActive(false);
             
            }
        }
        else
        {
            MoveThePlayer();
            
        }

        
        if (transform.childCount == 1 && FinishLine)
        {
            gameState = false;
        }
        
       
        if (gameState)
        {
          road.Translate(road.forward * Time.deltaTime * roadSpeed);
            
           // for (int i = 1; i < transform.childCount; i++)
           // {
           //     if (transform.GetChild(i).GetComponent<Animator>() != null)
           //         transform.GetChild(i).GetComponent<Animator>().SetBool("run",true);
           //    
           // }
        }

        if (moveTheCamera && transform.childCount > 1)
        {
            CinemachineTransposer cinemachineTransposer = SecondCam.GetComponent<CinemachineVirtualCamera>()
              .GetCinemachineComponent<CinemachineTransposer>();

            CinemachineComposer cinemachineComposer = SecondCam.GetComponent<CinemachineVirtualCamera>()
              .GetCinemachineComponent<CinemachineComposer>();

          cinemachineTransposer.m_FollowOffset = new Vector3(4.5f, Mathf.Lerp(cinemachineTransposer.m_FollowOffset.y,
              transform.GetChild(1).position.y + 2f, Time.deltaTime * 1f), -5f);
          
          cinemachineComposer.m_TrackedObjectOffset = new Vector3(0f,Mathf.Lerp(cinemachineComposer.m_TrackedObjectOffset.y,
              4f,Time.deltaTime * 1f),0f);
          
        }
       
    }
    
    void MoveThePlayer()
    {
        if (Input.GetMouseButtonDown(0) && gameState)
        {
            moveByTouch = true;
            
            Plane plane = new Plane(Vector3.up, 0f);

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            
            if (plane.Raycast(ray,out var distance))
            {
                mouseStartPos = ray.GetPoint(distance + 1f);
                playerStartPos = transform.position;
            }

        }
        
        if (Input.GetMouseButtonUp(0))
        {
            moveByTouch = false;
            
        }
        
        if (moveByTouch)
        { 
            Plane plane = new Plane(Vector3.up, 0f);
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            
            if (plane.Raycast(ray,out var distance))
            {
                Vector3 mousePos = ray.GetPoint(distance +  1f);
                   
                Vector3 move = mousePos - mouseStartPos;
                   
                Vector3 control = playerStartPos + move;


                if (numberOfStickmans > 50)
                    control.x = Mathf.Clamp(control.x, -0.7f, 0.7f);
                else
                    control.x = Mathf.Clamp(control.x, -1.1f, 1.1f);

                transform.position = new Vector3(Mathf.Lerp(transform.position.x,control.x,Time.deltaTime * playerSpeed)
                    ,transform.position.y,transform.position.z);
                  
            }
        }
    }

    public void FormatStickMan()
    {
        for (int i = 1; i < player.childCount; i++)
        {
            float x = DistanceFactor * Mathf.Sqrt(i) * Mathf.Cos(i * Radius);
            float z = DistanceFactor * Mathf.Sqrt(i) * Mathf.Sin(i * Radius);
            
            Vector3 NewPos = new Vector3(x,-0.55f,z);

            player.transform.GetChild(i).DOLocalMove(NewPos, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void MakeStickMan(int number)
    {
        for (int i = numberOfStickmans; i < number; i++)
        {
            Instantiate(stickMan, transform.position, quaternion.identity, transform);
        }

        numberOfStickmans = transform.childCount - 1;
        CounterTxt.text = numberOfStickmans.ToString();
        FormatStickMan();
    }


    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("gate"))
        {
            other.transform.parent.GetChild(0).GetComponent<BoxCollider>().enabled = false; // gate 1
            other.transform.parent.GetChild(1).GetComponent<BoxCollider>().enabled = false; // gate 2

            DoorManager gateManager = other.GetComponent<DoorManager>();

            numberOfStickmans = transform.childCount - 1;

            if (gateManager.multiply)
            {
                MakeStickMan(numberOfStickmans * gateManager.randomNumber);
            }
            else
            {
                MakeStickMan(numberOfStickmans + gateManager.randomNumber);

            }
        }

        if (other.CompareTag("enemy"))
        { 
            enemy = other.transform;
            attack = true;

            roadSpeed = 0.5f;
            
            other.transform.GetChild(1).GetComponent<enemyManager>().AttackThem(transform);

            StartCoroutine(UpdateTheEnemyAndPlayerStickMansNumbers());

        }

        if (other.CompareTag("Finish"))
        {
            SecondCam.SetActive(true);
            FinishLine = true;
            Tower.TowerInstance.CreateTower(transform.childCount - 1);
            transform.GetChild(0).gameObject.SetActive(false);
            
        }
    }

    IEnumerator UpdateTheEnemyAndPlayerStickMansNumbers()
    {

        numberOfEnemyStickmans = enemy.transform.GetChild(1).childCount - 1;
        numberOfStickmans = transform.childCount - 1;

        while (numberOfEnemyStickmans > 0 && numberOfStickmans > 0)
        {
            numberOfEnemyStickmans--;
            numberOfStickmans--;

            enemy.transform.GetChild(1).GetComponent<enemyManager>().CounterTxt.text = numberOfEnemyStickmans.ToString();
            CounterTxt.text = numberOfStickmans.ToString();
            
            yield return null;
        }

        if (numberOfEnemyStickmans == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).rotation = Quaternion.identity;
            }
        }
    }

    public void StartWinPanel()
    {
        StartCoroutine(WinPanel());
    }
    
    public IEnumerator WinPanel()
    {
        yield return new WaitForSeconds(2f);

        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
