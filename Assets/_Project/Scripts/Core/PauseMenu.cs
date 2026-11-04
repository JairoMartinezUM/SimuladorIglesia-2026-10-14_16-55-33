using StarterAssets;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public string menuScene = "Menu";

    StarterAssetsInputs inputs;
    bool paused;

    void Start()
    {
        inputs = FindFirstObjectByType<StarterAssetsInputs>();
        pausePanel.SetActive(false);
        LockCursor(true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (!kb.escapeKey.wasPressedThisFrame && !kb.pKey.wasPressedThisFrame) return;

        if (paused) Resume();
        else if (LevelManager.Instance != null && LevelManager.Instance.State == GameState.Playing) Pause();
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        LockCursor(false);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        LockCursor(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;

        if (inputs != null)
        {
            inputs.cursorInputForLook = locked;
            if (!locked) inputs.look = Vector2.zero;
        }
    }
}
