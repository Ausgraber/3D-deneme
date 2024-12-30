using UnityEngine;

namespace Platformer
{
    public class StepUpDown : MonoBehaviour
    {
        public Vector3 heightOffset;
        public float stairHeight;
        public float climbSpeed;
        public float dist;
        public float Div;
        public LayerMask Stairs;
        public Transform playerTransform;


        void Start()
        {
        
        }

        // Update is called once per frame
        void FixedUpdate()
        {   
            float dirY= Input.GetAxisRaw("Vertical");
            RaycastHit hit;
            if(Physics.Raycast(playerTransform.position+heightOffset, playerTransform.forward, out hit, dist, Stairs))
            {
               Vector3 climbPosition= new Vector3(playerTransform.position.x,playerTransform.position.y,playerTransform.position.z);
               if(dirY==1){
                playerTransform.position= Vector3.Lerp(playerTransform.position,climbPosition+playerTransform.forward/Div,climbSpeed);
               }
            }
        }
    }
}
