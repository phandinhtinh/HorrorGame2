using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject winPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Khởi tạo: Ẩn tất cả Panel khi bắt đầu game
        CloseAllPanels();
    }

    private void Update()
    {
        // Bấm phím Escape để Mở/Tắt Settings Panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu Win Panel đang bật thì không cho bấm Escape
            if (winPanel != null && winPanel.activeSelf) return;

            ToggleSettingsPanel();
        }
    }

    // Đóng tất cả các Panel và trả Game về trạng thái chơi bình thường
    public void CloseAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        SetGameState(false);
    }

    // Bật / Tắt Settings Panel
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isActive = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isActive);
            SetGameState(isActive);
        }
    }

    // Hiển thị Win Panel khi thắng
    public void ShowWinPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);

        SetGameState(true);
    }

    // Bật/tắt Pause game và hiện/ẩn con trỏ chuột
    private void SetGameState(bool isUIOpen)
    {
        Time.timeScale = isUIOpen ? 0f : 1f; // Tạm dừng game nếu mở UI
        Cursor.lockState = isUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isUIOpen;
    }

    // Chơi lại game (Nút Continue / Restart)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Trở về Menu chính (Nút Main Menu)
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}