using UnityEngine;
using UnityEngine.UI;

public class StartMenuAudioSettingsUI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private AudioSettingsManager manager;
    private bool suppressCallbacks;

    private void Awake()
    {
        manager = AudioSettingsManager.InstanceOrCreate();
    }

    private void OnEnable()
    {
        if (manager == null)
        {
            manager = AudioSettingsManager.InstanceOrCreate();
        }

        SetSliderValuesFromManager();
        RegisterCallbacks();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    private void UnregisterCallbacks()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    private void SetSliderValuesFromManager()
    {
        if (manager == null) return;

        suppressCallbacks = true;
        if (masterSlider != null) masterSlider.value = manager.MasterVolume;
        if (musicSlider != null) musicSlider.value = manager.MusicVolume;
        if (sfxSlider != null) sfxSlider.value = manager.SfxVolume;
        suppressCallbacks = false;
    }

    private void OnMasterChanged(float value)
    {
        if (suppressCallbacks || manager == null) return;
        manager.SetMasterVolume(value);
    }

    private void OnMusicChanged(float value)
    {
        if (suppressCallbacks || manager == null) return;
        manager.SetMusicVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        if (suppressCallbacks || manager == null) return;
        manager.SetSfxVolume(value);
    }
}
