

using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl2 : MonoBehaviour
{
    
    public Transform playerTransform; // Oyuncunun pozisyonunu takip 

    [Header("NavMesh Referans")]
    public NavMeshAgent agent; // NavMesh kullanarak hareket 
    //public Animator animator;
    [SerializeField] private BoxCollider RestrictedArea;
    public float lookDirection = 0f; // Bakacağı yön

    


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
    private bool isPlayer;
    
    
    [Header("Attack")]
    public float attackDistance = 2f; // Oyuncuya saldırma mesafesi

    [Header("Movement Flags")]
    public bool isAttacking;
    public bool isGrounded;
    public bool isChasing;
 

    [Header("Ground")]
    float groundCheckRadiusMultiplier = 0.9f;
    float groundCheckDistance = 0.05f;
    public LayerMask groundLayer;
    RaycastHit groundCheckHit=new RaycastHit();
       

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject restrictedAreaObj=GameObject.FindGameObjectWithTag("Restricted Area");

        if (restrictedAreaObj != null)
        {
            RestrictedArea = restrictedAreaObj.GetComponent<BoxCollider>();
        }

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

        Vector3 startPoint= new Vector3(RestrictedArea.bounds.center.x,-1.5f,RestrictedArea.bounds.center.z);
       

           //agent.isStopped=false;
           //agent.speed = walkingSpeed;
           agent.updateRotation=false;
           agent.SetDestination(startPoint);

    }
    private void Update(){

        isGrounded= CheckGround();
        HandleMovement();
        
        
        //ReturnPatrol();
        
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

            agent.SetDestination(playerTransform.position);
                //animasyon oynatılacak.
            }      
           
    }

 
    private void HandleRotation(Vector3 direction){
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
      
        transform.rotation= Quaternion.Slerp(transform.rotation,targetRotation, rotationSpeed*Time.deltaTime);
    }
  


    private void DetectPlayer(){
    
    for(int i=0 ; i<rayCount;i++){
        float t= (float)i/ (rayCount-1);
        float angle=Mathf.Lerp(startAngle,endAngle,t);

       Vector3 directionRay=Quaternion.Euler(0,angle,0)*transform.right; 
       Vector3 rayOrigin= new Vector3 (0,transform.position.y/2,0);
       if(Physics.Raycast(rayOrigin,directionRay,out RaycastHit hit ,rayLength+30f, LayerMask.GetMask("Player"))){
    
            isPlayer=true;
            Debug.DrawLine(transform.position, hit.point, Color.blue);
            Debug.Log("Player");
            

        }
            
        else{
            isPlayer=false;
        }
       
       }
    }

    private void ReturnPatrol(){
        if(!isChasing){
           
            agent.SetDestination(lastPatrolPoint);
        }
    }


    private Vector3 GetRandomPointInBounds(Transform areaTransform)
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
