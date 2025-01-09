using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;
    PlayerManager playerManager;
    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;
    CapsuleCollider playerCollider;
  
    

    [Header("Movement Speeds")]
        public float runningSpeed = 3;
        public float walkingSpeed = 1.5f;
        public float sprintingSpeed = 7;
        public float rotationSpeed = 15;
        public float climbingSpeed = 1;

        [Header("Ground")]
        float groundCheckRadiusMultiplier = 0.9f;
        float groundCheckDistance = 0.05f;
        RaycastHit groundCheckHit=new RaycastHit();


        [Header("Falling")]
        public float inAirTimer;
        public float leapingVelocity;
        public float fallingSpeed;
        public LayerMask groundLayer;
        public float rayCastHeighOffset = 0.5f;

        [Header("Jumping Speeds")]
        public float jumpHeight = 3;
        public float gravityIntensity = -15f;     

        [Header("Movement Flags")]
        public bool isSprinting;
        public bool isGrounded;
        public bool isJumping;
        public bool isClimbing;
        public bool isOnStairs;

    private void Awake()
    {
        inputManager=GetComponent<InputManager>();
        playerManager=GetComponent<PlayerManager>();
        playerRigidbody=GetComponent<Rigidbody>();
        playerCollider=GetComponent<CapsuleCollider>();
        cameraObject=Camera.main.transform;
    }
    private void Update(){

       isGrounded= CheckGround();

      

      //  Debug.Log($"isGrounded: {isGrounded}, isJumping: {isJumping}, inAirTimer: {inAirTimer}");

    }

   
   private void HandleMovement()
    {
        moveDirection=cameraObject.forward* inputManager.verticalInput;
        moveDirection += cameraObject.right*inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y=0;

        if(isSprinting)
        {
            moveDirection*= sprintingSpeed;

        }
        else{
            if(inputManager.moveAmount >=0.5f)
            {
                moveDirection *=runningSpeed;
            }
            else{
                moveDirection *=walkingSpeed;
            }
        }

        Vector3 movementVelocity=moveDirection;
        playerRigidbody.linearVelocity=movementVelocity;
    }
    private void HandleRotation(){
        if(isJumping)
        return;

        Vector3 targetDirection = Vector3.zero;
        targetDirection=cameraObject.forward*inputManager.verticalInput;
        targetDirection+=cameraObject.right*inputManager.horizontalInput;
        targetDirection.Normalize();

        targetDirection.y=0;
        if(targetDirection==Vector3.zero){
            targetDirection=transform.forward;
        }
        Quaternion targetRotation=Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation=Quaternion.Slerp(transform.rotation, targetRotation,rotationSpeed*Time.deltaTime);
        transform.rotation=playerRotation;
        
        }
        private bool CheckGround(){
            float sphereCastRadius=playerCollider.radius*groundCheckRadiusMultiplier;
            float sphereCastTravelDistance=playerCollider.bounds.extents.y-sphereCastRadius+groundCheckDistance;
            return Physics.SphereCast(playerRigidbody.position,sphereCastRadius,Vector3.down, out groundCheckHit, sphereCastTravelDistance);
        }       

        private void HandleFallingandLanding(){
            Vector3 rayCastOrigin =transform.position;
            Vector3 targetPosition=transform.position;
            rayCastOrigin.y+=rayCastHeighOffset; 
            if (!isGrounded && isJumping)
            {
                fallingSpeed += Time.deltaTime * -gravityIntensity;
                playerRigidbody.AddForce(Vector3.down * fallingSpeed * playerRigidbody.mass , ForceMode.Force); // Yerçekimi kuvvetini uyguluyoruz
            }
            else if(isGrounded){
               fallingSpeed=10f; 
            }                           
            
         
        }

        public void HandleJumping(){

                               
            if(isGrounded && !isJumping){
               playerRigidbody.AddForce(transform.up* Mathf.Sqrt(jumpHeight*-2f*gravityIntensity*playerRigidbody.mass),ForceMode.Impulse);
               print("Jumping");
                
                isGrounded=false;   
                
                           
            }
           
           
        }

      
        public void HandleAllMovement()
        {
            
           HandleFallingandLanding();
            
            HandleMovement();
            HandleRotation();
           
            
        }
}
