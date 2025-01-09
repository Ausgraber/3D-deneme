
using System.Collections;

using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour
{
    
    public Transform playerTransform; // Oyuncunun pozisyonunu takip 

    [Header("Patrol References")]
    public NavMeshAgent agent; // NavMesh kullanarak hareket edeceğiz
    //public Animator animator;
    [SerializeField]public Transform[] patrolAreas; // veya Collider[] patrolAreas
    private int currentAreaIndex = 0;

    [Header("Movement Speeds")]
    //public float walkingSpeed = 1.5f;
    public float rotationSpeed = 15f;

    [Header("Patrol")]
    public float patrolRadius = 5f; // Devriye gezeceği alanın yarıçapı
    public Vector3 startingPosition; // Düşmanın başlangıç noktası
    public float changeTargetTimer = 10f;
    public float timer;
    private Vector3 lastPatrolPoint;

    public float startAngle = -45f;  // Başlangıç açısı 
    public float endAngle = 45f;     // Bitiş açısı 
    public int rayCount = 10;        // Yay boyunca kaç ray gönderilecek
    public float rayLength = 10f;    // Ray uzunluğu
    public LayerMask hitLayer;       // Çarpma yapılacak layer  
    private bool isObstacle;
    
    
    [Header("Attack")]
    public float attackDistance = 2f; // Oyuncuya saldırma mesafesi

    [Header("Movement Flags")]
    public bool isAttacking;
    public bool isGrounded;
    public bool isChasing;
    public bool isPatrolling;
    public bool wasPatrolling;

    [Header("Ground")]
    float groundCheckRadiusMultiplier = 0.9f;
    float groundCheckDistance = 0.05f;
    public LayerMask groundLayer;
    RaycastHit groundCheckHit=new RaycastHit();
       

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        //animator=GetComponent<Animator>();
        startingPosition = transform.position;
        timer=changeTargetTimer;
        
    }

    private void Start()
    {
         if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player bulunamadı.");
            }
        }
           agent.isStopped=false;
           //agent.speed = walkingSpeed;
           agent.updateRotation=false;

    }
    private void Update(){

      
        isGrounded= CheckGround();
        HandleMovement();
        ReturnPatrol();
        DetectObstacle();
       // HandleRotation();
       // Debug.Log($"isGrounded: {isGrounded}, isChasing: {isChasing}");

    }
  

    private void HandleMovement()
    {  
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        var dirPlayer = (playerTransform.position-transform.position).normalized;

        bool isFacingPlayer= Vector3.Dot(dirPlayer,transform.position)>0.5f;
              
        if(distanceToPlayer <= attackDistance)
            {
                isPatrolling=false;
                wasPatrolling=true;
                isChasing=false;
                agent.isStopped=true;
                HandleRotation(dirPlayer);
                
                //saldırı yapma kısmı kodla eklenecek
                    
                //animasyon oynatılacak.

            }
        else if(distanceToPlayer <=patrolRadius){                    
                    
            HandleRotation(dirPlayer);
            agent.isStopped=false;
            isChasing=true;
            isPatrolling=false;
            wasPatrolling=true;

            agent.SetDestination(playerTransform.position);
                //animasyon oynatılacak.
            }

        else if(patrolAreas!=null && !isPatrolling ){
           isChasing=false;
           PatrolControl();
        }
              
           
    }

    private void PatrolControl()
    {
        if(!isPatrolling){
            isPatrolling=true;
            StartCoroutine(PatrolMove());
        }
        else{
            isPatrolling=false;
            StopCoroutine(PatrolMove());
        }
    }
    private void HandleRotation(Vector3 direction){
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
      
        transform.rotation= Quaternion.Slerp(transform.rotation,targetRotation, rotationSpeed*Time.deltaTime);
    }

    IEnumerator PatrolMove(){
        currentAreaIndex=0;
        while(isPatrolling){

            wasPatrolling=false;
           
            Vector3 randomPoint= patrolAreas[currentAreaIndex].position;
           
            
            var dirPoint = (randomPoint-transform.position).normalized;
            float pointDistance= Vector3.Distance(transform.position,dirPoint);
            while(agent.remainingDistance > 0.1f && isPatrolling){
                if(pointDistance<patrolRadius){
                    HandleRotation(dirPoint);
                }
                
                yield return null;
            }
             
                 
          
            Debug.Log($"Yeni hedef: Index {currentAreaIndex}, Konum {randomPoint}");

            agent.isStopped=false;
            agent.ResetPath();
            agent.SetDestination(randomPoint);
            lastPatrolPoint=randomPoint;

            yield return new WaitUntil(()=> !agent.pathPending &&  agent.remainingDistance<=0.1f);
            
            Debug.Log("Hedefe ulaştı!!!");

            agent.isStopped=true;
            yield return new WaitForSeconds(2);
            agent.isStopped=false;
            currentAreaIndex=(currentAreaIndex+1)%patrolAreas.Length;
        }
        
            
    }

    private void DetectObstacle(){
    
      ;
       for(int i=0 ; i<rayCount;i++){
        float t= (float)i/ (rayCount-1);
        float angle=Mathf.Lerp(startAngle,endAngle,t);

        Vector3 directionRay=Quaternion.Euler(0,angle,0)*transform.right; 

       if(Physics.Raycast(transform.position,directionRay,out RaycastHit hit ,rayLength,hitLayer)){
            isObstacle=true;
            Debug.DrawLine(transform.position, hit.point, Color.red);
            Debug.Log("Engel");

            }
        else{
            isObstacle=false;
        }
       
       }

       
    }

    private void ReturnPatrol(){
        if(wasPatrolling && !isChasing){
            wasPatrolling=false;
            agent.SetDestination(lastPatrolPoint);
        }
    }

    private UnityEngine.Vector3 GetRandomPointInBounds(Transform areaTransform)
    {
        Bounds bounds= new Bounds(areaTransform.position, areaTransform.localScale);
        if(areaTransform.TryGetComponent(out Renderer renderer)){
            bounds=renderer.bounds;
        } 
        //bounds içerisinde rastgele bir nokta seç
        Vector3 randomPoint =new Vector3(
            Random.Range(bounds.min.x,bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z,bounds.max.z)

        );
        return randomPoint;
    }


           private bool CheckGround(){
            float sphereCastRadius=GetComponent<CapsuleCollider>().radius*groundCheckRadiusMultiplier;
            float sphereCastTravelDistance=GetComponent<CapsuleCollider>().bounds.extents.y-sphereCastRadius+groundCheckDistance;
            return Physics.SphereCast(transform.position,sphereCastRadius,Vector3.down, out groundCheckHit, sphereCastTravelDistance,groundLayer);
        } 
    
}
