using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace Platformer
{

    
    public class StairUpDown : MonoBehaviour
    {

    [Header("Stairs")]
        [SerializeField] [Range(0.01f,3f)] float maxStepHeight = 0.5f;
        [SerializeField] [Range(0.01f,3f)] float minStepDepth = 0.3f;
        float stairHeighPaddingMultiplier = 1.5f;
        bool firstStep=true;
        float firstStepVelocityDistanceMultiplier = 0.1f;
        bool playerIsAscending;
        bool isOnStairs;
        bool playerIsDescending;
        float ascendingStairMovementMultiplier = 0.35f;
        float descendingStairMovementMultiplier = 0.7f;
        float maximumAngleOfApproachToAscend=45f;
        float playerHalfHeightToGround=0f;
        float maxAscendRayDistance=0f;
        float maxDescendRayDistance=0f;
        int numberOfStepDetectRays=0;
        float rayIncrementAmount=0f;
        float playerCenterToGroundDistance=0f;
        public float stairSpeed = 1;
        public float stairDetectionDistance = 3.5f;
        public LayerMask stairLayer;
        
        InputManager inputManager;
        PlayerLocomotion playerLocomotion;    
        Rigidbody playerRigidbody;
        CapsuleCollider playerCollider;


    public void Awake(){
    playerRigidbody = GetComponent<Rigidbody>();
    playerCollider = GetComponent<CapsuleCollider>();
    inputManager=GetComponent<InputManager>();
    
    maxAscendRayDistance=maxStepHeight/Mathf.Cos(maximumAngleOfApproachToAscend*Mathf.Deg2Rad);
    maxDescendRayDistance=maxStepHeight/Mathf.Cos(80f*Mathf.Deg2Rad);
    numberOfStepDetectRays=Mathf.RoundToInt(((maxStepHeight*100f)*0.5f)+1f);
    rayIncrementAmount=maxStepHeight/numberOfStepDetectRays;

    }
    public void LateUpdate(){
        CheckForStair();
        HandleStairs();
    }
    public void CheckForStair()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stairDetectionDistance, stairLayer);
        isOnStairs = hitColliders.Length > 0;
        if (isOnStairs)
        {
            Debug.Log("Stairs Detected");
        }
    }

    public void HandleStairs()
    {
        Vector3 calculatedStepInput = inputManager.movementInput;
        
        playerHalfHeightToGround = playerCollider.bounds.extents.y;
        if (playerCenterToGroundDistance < playerCollider.bounds.extents.y)
        {
            playerHalfHeightToGround = playerCenterToGroundDistance;
        }
        if(playerIsAscending){
        // Önce yokuş yukarı kontrolü yap
        calculatedStepInput = AscendStairs(calculatedStepInput);

        }
        
        // Eğer yokuş yukarı değilse, yokuş aşağı kontrolü yap
        if (!playerIsAscending)
        {
            calculatedStepInput = DescendStairs(calculatedStepInput);

        }
    }

    private Vector3 AscendStairs(Vector3 calculatedStepInput)
    {
        // Hareket yoksa hemen dön
        if (inputManager.moveAmount == 0)
        {
            ResetStairState();
            return calculatedStepInput;
        }

        float calculatedVelDistance = firstStep ? 
            (playerRigidbody.linearVelocity.magnitude * firstStepVelocityDistanceMultiplier) + playerCollider.radius 
            : playerCollider.radius;

        List<RaycastHit> rayThatsHit = CastAscendingRays(calculatedVelDistance);

        if (rayThatsHit.Count > 0)
        {
            return HandleAscendingStairHit(rayThatsHit, calculatedStepInput, calculatedVelDistance);
        }
        
        ResetStairState();
        return calculatedStepInput;
    }

    private Vector3 DescendStairs(Vector3 calculatedStepInput)
    {
        // Hareket yoksa hemen dön
        if (inputManager.moveAmount == 0)
        {
            ResetDescendingState();
            return calculatedStepInput;
        }

        List<RaycastHit> rayThatHit = CastDescendingRays();

        if (rayThatHit.Count > 0)
        {
            return HandleDescendingStairHit(rayThatHit, calculatedStepInput);
        }
        if(rayThatHit==null || rayThatHit.Count==0)
        ResetDescendingState();
        return calculatedStepInput;
    }

    private List<RaycastHit> CastAscendingRays(float calculatedVelDistance)
    {
        float ray = 0f;
        List<RaycastHit> rayThatsHit = new List<RaycastHit>();
        
        for (int x = 1; x <= numberOfStepDetectRays; x++, ray += rayIncrementAmount)
        {
            Vector3 rayLower = new Vector3(
                playerRigidbody.position.x,
                ((playerRigidbody.position.y - playerHalfHeightToGround) + ray),
                playerRigidbody.position.z
            );
            
            RaycastHit hitLower;
            if (Physics.Raycast(rayLower, playerRigidbody.transform.TransformDirection(inputManager.movementInput), 
                out hitLower, calculatedVelDistance + maxAscendRayDistance))
            {
                float stairSlopeAngle = Vector3.Angle(hitLower.normal, playerRigidbody.transform.up);
                if (stairSlopeAngle >= 85f && stairSlopeAngle <= 95f)
                {
                    rayThatsHit.Add(hitLower);
                    Debug.DrawRay(rayLower, playerRigidbody.transform.TransformDirection(inputManager.movementInput), Color.red, 0.5f);
                }
            }
        }
        
        return rayThatsHit;
    }

    private Vector3 HandleAscendingStairHit(List<RaycastHit> rayThatsHit, Vector3 calculatedStepInput, float calculatedVelDistance)
    {
        Vector3 rayUpper = new Vector3(
            playerRigidbody.position.x,
            (((playerRigidbody.position.y - playerHalfHeightToGround) + maxStepHeight) + rayIncrementAmount),
            playerRigidbody.position.z
        );
        
        RaycastHit hitUpper;
        Physics.Raycast(rayUpper, playerRigidbody.transform.TransformDirection(inputManager.movementInput),
            out hitUpper, calculatedVelDistance + (maxAscendRayDistance * 2f));

        if (!hitUpper.collider || (hitUpper.distance - rayThatsHit[0].distance) < minStepDepth)
        {
            if (Vector3.Angle(rayThatsHit[0].normal, playerRigidbody.transform.TransformDirection(inputManager.movementInput)) 
                < maximumAngleOfApproachToAscend)
            {
                return ProcessAscendingMovement(calculatedStepInput, rayThatsHit);
            }
        }

        ResetStairState();
        return calculatedStepInput;
    }

    private Vector3 ProcessAscendingMovement(Vector3 calculatedStepInput, List<RaycastHit> rayThatsHit)
    {
        playerIsAscending = true;
        isOnStairs = true;
        playerIsDescending=false;
        Vector3 playerRelX = Vector3.Cross(inputManager.movementInput, Vector3.up);

        if (firstStep)
        {
            calculatedStepInput = Quaternion.AngleAxis(45.0f, playerRelX) * calculatedStepInput;
            Debug.Log("First Step!");
            firstStep = false;
            Debug.Log("First Step değil!  ");
        }
        else
        {
            float stairHeight = rayThatsHit.Count * rayIncrementAmount * stairHeighPaddingMultiplier;
            float avgDistance = rayThatsHit.Average(r => r.distance);
            float tanAngle = Mathf.Atan2(stairHeight, avgDistance) * Mathf.Rad2Deg;
            
            calculatedStepInput = Quaternion.AngleAxis(tanAngle, playerRelX) * calculatedStepInput;
            calculatedStepInput *= ascendingStairMovementMultiplier;
        }

        return calculatedStepInput;
    }

    private void ResetStairState()
    {
        playerIsAscending = false;
        firstStep = true;
        isOnStairs = false;
    }

    private void ResetDescendingState()
    {
        playerIsDescending = false;
        isOnStairs = false;
    }

    private List<RaycastHit> CastDescendingRays()
    {
        float ray = 0f;
        List<RaycastHit> rayThatHit = new List<RaycastHit>();
        
        for (int x = 1; x <= numberOfStepDetectRays; x++, ray += rayIncrementAmount)
        {
            Vector3 rayLower = new Vector3(
                playerRigidbody.position.x,
                ((playerRigidbody.position.y - playerHalfHeightToGround) + ray),
                playerRigidbody.position.z
            );
            
            RaycastHit hitLower;
            if (Physics.Raycast(rayLower, playerRigidbody.transform.TransformDirection(-inputManager.movementInput),
                out hitLower, playerCollider.radius + maxDescendRayDistance))
            {
                float stairSlopeAngle = Vector3.Angle(hitLower.normal, -playerRigidbody.transform.up);
                if (stairSlopeAngle >= 40f)
                {
                    rayThatHit.Add(hitLower);
                    Debug.DrawRay(rayLower, playerRigidbody.transform.TransformDirection(-inputManager.movementInput), Color.yellow, 0.5f);
                }
            }
        }
        
        return rayThatHit;
    }

    private Vector3 HandleDescendingStairHit(List<RaycastHit> rayThatHit, Vector3 calculatedStepInput)
    {
        Vector3 rayUpper = new Vector3(
            playerRigidbody.position.x,
            (((playerRigidbody.position.y - playerHalfHeightToGround) + maxStepHeight) + rayIncrementAmount),
            playerRigidbody.position.z
        );
        
        RaycastHit hitUpper;
        Physics.Raycast(rayUpper, playerRigidbody.transform.TransformDirection(-inputManager.movementInput),
            out hitUpper, playerCollider.radius + (maxDescendRayDistance * 2f));

        if (!hitUpper.collider || (hitUpper.distance - rayThatHit[0].distance) > minStepDepth)
        {
            if (!playerLocomotion.isGrounded && hitUpper.distance < playerCollider.radius + (maxDescendRayDistance * 2f))
            {   
                if(rayThatHit!=null || rayThatHit.Count!=0){
                    return ProcessDescendingMovement(calculatedStepInput, rayThatHit);
                }
               
                
            }
        }

        ResetDescendingState();
        return calculatedStepInput;
    }

    private Vector3 ProcessDescendingMovement(Vector3 calculatedStepInput, List<RaycastHit> rayThatHit)
    {
        playerIsDescending = true;
        isOnStairs = true;
        playerIsAscending=false;

        Vector3 playerRelX = Vector3.Cross(inputManager.movementInput, Vector3.up);
        float stairHeight = rayThatHit.Count * rayIncrementAmount * stairHeighPaddingMultiplier;
        float avgDistance = rayThatHit.Average(r => r.distance);
        float tanAngle = Mathf.Atan2(stairHeight, avgDistance) * Mathf.Rad2Deg;
        
        calculatedStepInput = Quaternion.AngleAxis(tanAngle - 90f, playerRelX) * calculatedStepInput;
        calculatedStepInput *= descendingStairMovementMultiplier;
        
        return calculatedStepInput;
    }
        }
    }
