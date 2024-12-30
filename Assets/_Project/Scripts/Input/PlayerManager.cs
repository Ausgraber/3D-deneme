using UnityEngine;

namespace Platformer
{
    public class PlayerManager : MonoBehaviour
    {
        InputManager inputManager;
        PlayerLocomotion playerLocomotion;
        CameraManager cameraManager;
        Animator animator;
        public bool isInteracting;

        private void Awake(){
            inputManager=GetComponent<InputManager>();
            playerLocomotion=GetComponent<PlayerLocomotion>();
            cameraManager=FindAnyObjectByType<CameraManager>();
            animator=GetComponent<Animator>();

        }
        private void Update(){
           inputManager.HandleAllInputs();
        }

        private void FixedUpdate(){
            playerLocomotion.HandleAllMovement();
        }
        private void LateUpdate(){
            cameraManager.HandleAllCameraMovement();
            isInteracting=animator.GetBool("isInteracting"); // animatordeki isInteracting i al
            playerLocomotion.isJumping=animator.GetBool("isJumping");
            animator.SetBool("isGrounded",playerLocomotion.isGrounded);

        }
    }
}
