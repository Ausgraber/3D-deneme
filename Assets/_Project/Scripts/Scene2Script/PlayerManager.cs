using UnityEngine;

public class PlayerManager : MonoBehaviour
{   
    InputManager inputManager;
    PlayerLocomotion playerLocomotion;
    CameraManager cameraManager;

    //public bool isInteracting;
    
    

    private void Awake()
    {
        inputManager=GetComponent<InputManager>();
        playerLocomotion=GetComponent<PlayerLocomotion>();
        cameraManager=FindAnyObjectByType<CameraManager>();

    }

    // Update is called once per frame
    private void Update()
    {
        inputManager.HandleAllInputs();
    }
    private void FixedUpdate(){
        playerLocomotion.HandleAllMovement();
    }
    private void LateUpdate(){
        cameraManager.HandleAllCameraMovement();
    }
}
