using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetObject : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") == true)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Application.OpenURL("https://forms.gle/XoRZPWFpwxzbp8Ka9");
                Application.Quit();
            }
            gameObject.SetActive(false);
        }
    }
}
