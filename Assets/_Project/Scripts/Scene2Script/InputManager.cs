using UnityEditor.VersionControl;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    InputSystem playerControl;
    PlayerLocomotion playerLocomotion;
    
    public Vector2 movementInput;
    public Vector2 cameraInput;
    public float cameraXInput;
    public float cameraYInput;

    public float verticalInput;
    public float horizontalInput;
    public float moveAmount;
    public bool b_Input=false;
    public bool jump_Input=false;

    private void Awake(){
        playerLocomotion=GetComponent<PlayerLocomotion>();
    }
    
    private void OnEnable(){
        if(playerControl==null){

            playerControl=new InputSystem();
            playerControl.Player.Move.performed += i=>movementInput=i.ReadValue<Vector2>();
            playerControl.Player.Look.performed += i=>cameraInput=i.ReadValue<Vector2>();

            playerControl.Player.Sprint.performed += i=> b_Input=true;
            playerControl.Player.Sprint.canceled += i=> b_Input=false;
            playerControl.Player.Jump.performed += i=> jump_Input=true;
            playerControl.Player.Jump.canceled += i=> jump_Input=false;
            
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
        //animatorManager.UpdateAnimatorValues(0,moveAmount,playerLocomotion.isSprinting);

    }
    private void HandleSprintInput(){
        if(b_Input && moveAmount>0.5f){
            playerLocomotion.isSprinting=true;
        }
        else{
            playerLocomotion.isSprinting=false;
            b_Input=false;
        }
    }
    private void HandleJumpInput(){
        if(jump_Input && playerLocomotion.isGrounded){
            
            playerLocomotion.HandleJumping();
            playerLocomotion.isJumping=true;
            if(playerLocomotion.isJumping){
                playerLocomotion.inAirTimer+=Time.deltaTime;
            }
            
        }
        else if(playerLocomotion.isGrounded && playerLocomotion.isJumping){
            playerLocomotion.isJumping=false;
            playerLocomotion.inAirTimer=0;
            jump_Input=false;
        }
    }
}
