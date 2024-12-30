using UnityEngine;

namespace Platformer
{
    public class AnimatorManager : MonoBehaviour
    {
        public Animator animator;
        int horizontal;
        int vertical;
        private void Awake()
        {
            animator = GetComponent<Animator>();
            horizontal = Animator.StringToHash("Horizontal");
            vertical = Animator.StringToHash("Vertical");
        }

        public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting){

            //Animation snapping?
            float snappedHorizontal;
            float snappedVertical;
            #region snappedHorizontal
            if(horizontalMovement > 0 && horizontalMovement < 0.55f){
                snappedHorizontal = 0.5f;
            }
            else if(horizontalMovement > 0.55f){
                snappedHorizontal = 1;
            }
            else if(horizontalMovement < 0 && horizontalMovement > -0.55f){
                snappedHorizontal = -0.5f;
            }
            else if(horizontalMovement < -0.55f){
                snappedHorizontal = -1;
            }
            else{
                snappedHorizontal = 0;
            }
            #endregion
            #region snappedVertical
            if(verticalMovement > 0 && verticalMovement < 0.55f){
                snappedVertical = 0.5f;
            }
            else if(verticalMovement > 0.55f){
                snappedVertical = 1;
            }
            else if(verticalMovement < 0 &&verticalMovement > -0.55f){
               snappedVertical = -0.5f;
            }
            else if(verticalMovement < -0.55f){
                snappedVertical = -1;
            }
            else{
                snappedVertical = 0;
            }
            #endregion
            
            if(isSprinting){
                snappedVertical=2;
                snappedHorizontal=horizontalMovement;
            }
            animator.SetFloat(horizontal,snappedHorizontal,0.3f, Time.deltaTime);
            animator.SetFloat(vertical,snappedVertical,0.3f, Time.deltaTime);
        }

        public void playTargetAnimation(string targetAnimation,bool isInteracting){
            animator.SetBool("isInteracting",isInteracting); //
            animator.CrossFade(targetAnimation,0.2f);

        }
        
    }
}
