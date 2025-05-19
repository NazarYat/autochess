using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneLoaderOnClick : MonoBehaviour
{
    [Tooltip("Name of the scene to load")]
    public string sceneToLoad;

    // This method is called from the EventTrigger component
    public void OnPointerClick()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Load the new scene
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single); // Unloads current scene
        }
        else
        {
            Debug.LogWarning("Scene name is not set.");
        }
    }
}