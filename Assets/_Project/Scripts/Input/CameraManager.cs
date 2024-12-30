
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer
{
    public class CameraManager : MonoBehaviour
    {
        InputManager inputManager;

        public Transform targetTransform; // kameranın takip edeceği nesne
        public Transform cameraPivot; // kameranın dönmesi için pivot nesnesi
        public Transform cameraTransform; // kameranın kendisi
        public LayerMask collisionLayers; // kamera çarpışmaları için layerlar  
        private float defaultPosition;
        private Vector3 cameraFollowVelocity = Vector3.zero;
        private Vector3 cameraVectorPosition;

        public float cameraCollisionOffset = 0.2f; // kamera çarpışma offseti
        public float minCollisionOffset = 0.2f; // kamera çarpışma offseti
        public float cameraCollisionRadius = 2f;
        public float followSpeed = 0.2f;
        public float cameraLookSpeed = 2f;
        public float cameraPivotSpeed = 2f;


        public float lookAngle; // kameranın
        public float pivotAngle; // kameranın pivot açısı up down
        public float minPivotAngle = -35; // up down limitleri
        public float maxPivotAngle = 35;

        private void Awake()
        {
            inputManager = FindAnyObjectByType<InputManager>();
            targetTransform=FindAnyObjectByType<PlayerManager>().transform;
            cameraTransform=Camera.main.transform;
            defaultPosition= cameraTransform.localPosition.z;
        }
        private void FollowTarget(Transform target)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position,ref cameraFollowVelocity, followSpeed );
            transform.position = targetPosition;
        }
        private void RotateCamera(){

            Vector3 rotation;
            Quaternion targetRotation;
        
            lookAngle=lookAngle+(inputManager.cameraXInput*cameraLookSpeed);
            pivotAngle=pivotAngle-(inputManager.cameraYInput*cameraPivotSpeed);
            pivotAngle=Mathf.Clamp(pivotAngle,minPivotAngle,maxPivotAngle);

            rotation=Vector3.zero;
            rotation.y=lookAngle;
            targetRotation=Quaternion.Euler(rotation);
            transform.rotation=targetRotation; 

            rotation=Vector3.zero;
            rotation.x=pivotAngle;
            targetRotation=Quaternion.Euler(rotation);
            cameraPivot.localRotation=targetRotation;
        }
        public void HandleAllCameraMovement(){
            FollowTarget(targetTransform);
            RotateCamera();
            HandleAllCameraCollisions();
        }

        public void HandleAllCameraCollisions(){
            float targetPosition = defaultPosition;
            RaycastHit hit;
            Vector3 direction = cameraTransform.position - cameraPivot.position;
            direction.Normalize();
            if(Physics.SphereCast(cameraPivot.transform.position,cameraCollisionRadius,direction,
                    out hit,Mathf.Abs(targetPosition),collisionLayers)){

                float distance = Vector3.Distance(cameraPivot.position,hit.point);
                targetPosition = -(distance-cameraCollisionOffset);
            }
            if(Mathf.Abs(targetPosition)<minCollisionOffset){
                targetPosition -= minCollisionOffset; ;
            }

            cameraVectorPosition.z=Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
            cameraTransform.localPosition=cameraVectorPosition;

        }
    }
}
