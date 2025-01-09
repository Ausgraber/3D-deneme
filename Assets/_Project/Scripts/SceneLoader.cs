using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class SceneLoader : MonoBehaviour
    {
      

        public void LoadScene(string sceneName){
            SceneManager.LoadScene(sceneName);
        }
        public void QuitMenu(){
            Application.Quit();
        }
    }
}
