using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("FMOD VCA Paths")]
    [Tooltip("Đường dẫn VCA quản lý BGM trong FMOD Studio")]
    public string bgmVcaPath = "vca:/BGM";
    [Tooltip("Đường dẫn VCA quản lý SFX trong FMOD Studio")]
    public string sfxVcaPath = "vca:/SFX";

    private VCA bgmVca;
    private VCA sfxVca;

    [Header("FMOD Event Paths (Sẵn đường dẫn - Không cần kéo gán Inspector)")]
    public string bgmPath = "event:/BGM/Background_Music";
    public string walkFootstepPath = "event:/SFX/Player_Footsteps";
    public string runFootstepPath = "event:/SFX/Player_Footsteps";
    public string attackPath = "event:/SFX/Player_Attack";

    private EventInstance bgmInstance;        // Instance quản lý nhạc nền
    private EventInstance footstepInstance;   // Instance quản lý bước chân (để dừng chủ động)

    [Header("UI Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("UI Toggles (Tích chọn = Bật âm thanh)")]
    public Toggle bgmMuteToggle;
    public Toggle sfxMuteToggle;

    private float currentBgmVol = 0.75f;
    private float currentSfxVol = 0.75f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Khởi tạo VCA an toàn
        try
        {
            bgmVca = RuntimeManager.GetVCA(bgmVcaPath);
            sfxVca = RuntimeManager.GetVCA(sfxVcaPath);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] Bỏ qua VCA chưa tạo trong FMOD: {e.Message}");
        }

        // Phát nhạc nền (BGM)
        PlayBGM();

        // Cài đặt Slider BGM
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            currentBgmVol = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
            bgmSlider.value = currentBgmVol;
            SetBGMVolume(currentBgmVol);
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        // Cài đặt Slider SFX
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            currentSfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxSlider.value = currentSfxVol;
            SetSFXVolume(currentSfxVol);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Cài đặt Toggle BGM
        if (bgmMuteToggle != null)
        {
            bgmMuteToggle.isOn = PlayerPrefs.GetInt("BGMEnabled", 1) == 1;
            ToggleBGM(bgmMuteToggle.isOn);
            bgmMuteToggle.onValueChanged.AddListener(ToggleBGM);
        }

        // Cài đặt Toggle SFX
        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.isOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
            ToggleSFX(sfxMuteToggle.isOn);
            sfxMuteToggle.onValueChanged.AddListener(ToggleSFX);
        }
    }

    #region --- MUSIC & SFX METHODS ---

    // Phát nhạc nền BGM
    public void PlayBGM()
    {
        if (string.IsNullOrEmpty(bgmPath)) return;

        if (!bgmInstance.isValid())
        {
            bgmInstance = RuntimeManager.CreateInstance(bgmPath);
        }
        bgmInstance.start();
    }

    // Dừng nhạc nền BGM
    public void StopBGM(bool fadeOut = true)
    {
        if (bgmInstance.isValid())
        {
            FMOD.Studio.STOP_MODE stopMode = fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE;
            bgmInstance.stop(stopMode);
        }
    }

    // Phát âm thanh bước chân
    public void PlayFootstep(bool isRunning = false)
    {
        // 1. Kiểm tra Mute & Âm lượng
        if (sfxMuteToggle != null && !sfxMuteToggle.isOn) return;
        if (currentSfxVol <= 0f) return;

        string targetPath = isRunning ? runFootstepPath : walkFootstepPath;

        if (string.IsNullOrEmpty(targetPath))
        {
            targetPath = walkFootstepPath;
        }

        if (string.IsNullOrEmpty(targetPath)) return;

        // 2. Dừng tiếng bước chân trước đó (nếu còn đang phát)
        StopFootstep();

        // 3. Khởi tạo Instance mới
        footstepInstance = RuntimeManager.CreateInstance(targetPath);
        footstepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        // 4. Gửi Parameter "IsSprinting" sang FMOD Studio
        // isRunning = true  -> IsSprinting = 1.0f (Âm thanh Chạy)
        // isRunning = false -> IsSprinting = 0.0f (Âm thanh Đi bộ)
        footstepInstance.setParameterByName("IsSprinting", isRunning ? 1.0f : 0.0f);

        footstepInstance.start();
    }

    // Dừng ngay lập tức âm thanh bước chân khi dừng di chuyển
    public void StopFootstep()
    {
        if (footstepInstance.isValid())
        {
            footstepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            footstepInstance.release();
        }
    }

    // Phát âm thanh tấn công (Attack)
    public void PlayAttack(Vector3 position = default)
    {
        if (sfxMuteToggle != null && !sfxMuteToggle.isOn) return;
        if (currentSfxVol <= 0f) return;

        if (!string.IsNullOrEmpty(attackPath))
        {
            Vector3 playPosition = (position == default) ? transform.position : position;
            RuntimeManager.PlayOneShot(attackPath, playPosition);
        }
    }

    // Dừng âm thanh SFX
    public void StopSFX()
    {
        StopFootstep();
    }

    #endregion

    #region --- VCA & VOLUME CONTROL ---

    public void SetBGMVolume(float value)
    {
        currentBgmVol = value;
        PlayerPrefs.SetFloat("BGMVolume", value);

        if (bgmMuteToggle != null && !bgmMuteToggle.isOn)
        {
            ApplyBgmVolume(0f);
            return;
        }

        ApplyBgmVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        currentSfxVol = value;
        PlayerPrefs.SetFloat("SFXVolume", value);

        if (sfxMuteToggle != null && !sfxMuteToggle.isOn)
        {
            ApplySfxVolume(0f);
            return;
        }

        ApplySfxVolume(value);
    }

    public void ToggleBGM(bool isEnabled)
    {
        PlayerPrefs.SetInt("BGMEnabled", isEnabled ? 1 : 0);
        float targetVol = isEnabled ? currentBgmVol : 0f;
        ApplyBgmVolume(targetVol);
    }

    public void ToggleSFX(bool isEnabled)
    {
        PlayerPrefs.SetInt("SFXEnabled", isEnabled ? 1 : 0);
        float targetVol = isEnabled ? currentSfxVol : 0f;
        ApplySfxVolume(targetVol);

        if (!isEnabled)
        {
            StopFootstep();
        }
    }

    private void ApplyBgmVolume(float volume)
    {
        if (bgmVca.isValid())
        {
            bgmVca.setVolume(volume);
        }

        if (bgmInstance.isValid())
        {
            bgmInstance.setVolume(volume);
        }
    }

    private void ApplySfxVolume(float volume)
    {
        if (sfxVca.isValid())
        {
            sfxVca.setVolume(volume);
        }
    }

    #endregion

    private void OnDestroy()
    {
        StopFootstep();

        if (bgmInstance.isValid())
        {
            bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgmInstance.release();
        }
    }
}