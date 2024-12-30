
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Platformer
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField]Player_Control playerControl;
        PlayerLocomotion playerLocomotion;
        AnimatorManager animatorManager;

        public Vector2 movementInput;
        public Vector2 cameraInput;

        public float cameraXInput;
        public float cameraYInput;

        public float verticalInput;
        public float horizontalInput;
        public float moveAmount;
        public bool b_Input;
        public bool jump_Input;

        private void Awake(){
            animatorManager=GetComponent<AnimatorManager>();
            playerLocomotion=GetComponent<PlayerLocomotion>();
        }

        private void OnEnable(){

            if(playerControl==null){
            playerControl=new Player_Control();
            playerControl.PlayerMovement.Movement.performed += i=>movementInput=i.ReadValue<Vector2>();
            playerControl.PlayerMovement.Camera.performed += i=> cameraInput = i.ReadValue<Vector2>();

            playerControl.PlayerActions.Sprint.performed += i=> b_Input = true;
            playerControl.PlayerActions.Sprint.canceled += i=> b_Input = false;
            playerControl.PlayerActions.Jump.performed += i=> jump_Input = true;

            }
            playerControl.Enable();

        }
        private void OnDisable(){
            playerControl.Disable();
        }
        public void HandleAllInputs(){
            HandleMovementInput();
            HandleSprintInput();
            HandleJumpInput();
            //HandleAttackInput();
        }
        private void HandleMovementInput(){

            verticalInput=movementInput.y;
            horizontalInput=movementInput.x;
            
            cameraXInput=cameraInput.x;
            cameraYInput=cameraInput.y;
            

            moveAmount=Mathf.Clamp01(Mathf.Abs(horizontalInput)+Mathf.Abs(verticalInput));
            animatorManager.UpdateAnimatorValues(0,moveAmount,playerLocomotion.isSprinting);
        }

        private void HandleSprintInput(){
            if(b_Input && moveAmount>0.5f){
                playerLocomotion.isSprinting=true;
            }
            else{
                playerLocomotion.isSprinting=false;
            }
        }
        private void HandleJumpInput(){
            if(jump_Input){
                jump_Input=false;
                playerLocomotion.HandleJumping();

            }   
        }
    }
}
