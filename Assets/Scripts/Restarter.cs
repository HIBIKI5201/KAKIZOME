using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Restarter : MonoBehaviour
{
    private int _index;

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        _index = scene.buildIndex;
    }

    private void Update()
    {
        if (Keyboard.current.anyKey.isPressed)
        {
            SceneManager.LoadScene(_index);
        }
    }
}
