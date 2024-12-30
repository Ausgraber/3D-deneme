
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;


namespace Platformer
{
    public class PlayerLocomotion : MonoBehaviour
    {
        InputManager inputManager;
        PlayerManager playerManager;
        AnimatorManager animatorManager;
        Vector3 moveDirection;
        Transform cameraObject;
        Rigidbody playerRigidbody;
        CapsuleCollider playerCollider;
        StairUpDown stairUpDown;

        [Header("Falling")]
        public float inAirTimer;
        public float leapingVelocity;
        public float fallingSpeed;
        public LayerMask groundLayer;
        public float rayCastHeighOffset = 0.5f;

        [Header("Movement Flags")]
        public bool isSprinting;
        public bool isGrounded;
        public bool isJumping;
        public bool isClimbing;
        public bool isOnStairs;

        [Header("Movement Speeds")]
        public float runningSpeed = 3;
        public float walkingSpeed = 1.5f;
        public float sprintingSpeed = 7;
        public float rotationSpeed = 15;
        public float climbingSpeed = 1;

        [Header("Jumping Speeds")]
        public float jumpHeight = 3;
        public float gravityIntensity = -15f;     
       

        private void Awake()
        {
            inputManager = GetComponent<InputManager>();
            playerManager = GetComponent<PlayerManager>();
            playerRigidbody = GetComponent<Rigidbody>();
            animatorManager = GetComponent<AnimatorManager>();
            playerCollider = GetComponent<CapsuleCollider>();
            stairUpDown=GetComponent<StairUpDown>();
            cameraObject = Camera.main.transform;

        }

        public void HandleAllMovement()
        {
            HandleFallingandLanding();

            if (playerManager.isInteracting) return;
            HandleMovement();
            HandleRotation();
        }
        private void FixedUpdate()
        {
           stairUpDown.CheckForStair();
           stairUpDown.LateUpdate();

          //  Debug.DrawRay(playerRigidbody.position,playerRigidbody.transform.TransformDirection(moveDirection),Color.red,0.5f);
        }
        private void HandleMovement()
        {
            moveDirection = cameraObject.forward * inputManager.verticalInput;
            moveDirection += cameraObject.right * inputManager.horizontalInput;
            moveDirection.Normalize();
            moveDirection.y = 0; // y ekseninde hareket etmemesi için

            // Koşuyorsa koşu hızı, yürüyorsa yürüme hızı
            if (isSprinting)
            {
                moveDirection *= sprintingSpeed;
            }
            else
            {
                if (inputManager.moveAmount >= 0.5f)
                {
                    moveDirection *= runningSpeed;
                }
                else
                {
                    moveDirection *= walkingSpeed;
                }
            }

            Vector3 movementVelocity = moveDirection;
            playerRigidbody.linearVelocity = movementVelocity;
        }

        private void HandleRotation()
        {
            if (isJumping)
                return ;

            Vector3 targetDirection = Vector3.zero;
            targetDirection = cameraObject.forward * inputManager.verticalInput;
            targetDirection += cameraObject.right * inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = playerRotation;
        }

        private void HandleFallingandLanding()
        {
            Vector3 rayCastOrigin = transform.position;
            Vector3 targetPosition;
            targetPosition = transform.position;
            rayCastOrigin.y += rayCastHeighOffset;

            if (!isGrounded && !isJumping )
            {
                if (!playerManager.isInteracting)
                {
                    animatorManager.playTargetAnimation("JumpAir_Normal_InPlace_SwordAndShield", true);
                }
                inAirTimer += Time.deltaTime;
                playerRigidbody.AddForce(transform.forward * leapingVelocity);
                playerRigidbody.AddForce(Vector3.down * fallingSpeed * inAirTimer);
            }

            if (Physics.SphereCast(rayCastOrigin, 0.2f, Vector3.down, out RaycastHit hit, 0.5f))
            {
                if (!isGrounded && playerManager.isInteracting)
                {
                    animatorManager.playTargetAnimation("JumpEnd_Normal_InPlace_SwordAndShield", true);
                }
                Vector3 rayCastHitPoint = hit.point; // Yerden yukarıda olan bir nokta
                targetPosition.y = rayCastHitPoint.y;
                inAirTimer = 0;
                isGrounded = true;
                isJumping = false; // Reset isJumping flag
            }
            else
            {
                isGrounded = false;
            }

            if (isGrounded && !isJumping)
            {
                if (playerManager.isInteracting || inputManager.moveAmount > 0)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / 0.2f);
                }
                else
                {
                    transform.position = targetPosition;
                }
            }
           
        }

        public void HandleJumping()
        {
            if (isGrounded && !isJumping)
            {
                animatorManager.animator.SetBool("isJumping", true);
                animatorManager.playTargetAnimation("JumpFull_Normal_RM_SwordAndShield", false);

                float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
                Vector3 playerVelocity = moveDirection*walkingSpeed;

                playerVelocity.y = jumpingVelocity;
                playerRigidbody.linearVelocity = playerVelocity;
                
                isJumping = true; 
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Stairs"))
            {
           
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Stairs"))
            {
                
            }
        }

        
    }
}


