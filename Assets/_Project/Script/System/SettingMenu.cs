using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingMenu : MonoBehaviour
{
    private UIController uiController;

    [Header("Volume Field")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private const float maxVolumeDb = -30.0f;

    void Awake()
    {
        // 저장된 볼륨 값을 불러와 슬라이더에 설정
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 리스너 연결
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();

        // 믹서값 설정
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterSlider.value) * 20 + maxVolumeDb);
        mainMixer.SetFloat("BGMVolume", Mathf.Log10(bgmSlider.value) * 20 + maxVolumeDb);
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxSlider.value) * 20 + maxVolumeDb);
    }

    public void OnEnable()
    {
        if (uiController == null)
            return;
    }

    /* 설정 메뉴 닫기 */
    public void CloseSettings()
    {
        uiController.PopUI();
    }

    public void SetMasterVolume(float sliderValue)
    {
        float dbValue = (Mathf.Log10(sliderValue) * 20) + maxVolumeDb;
        if (sliderValue == 0)
            dbValue = -80f;

        mainMixer.SetFloat("MasterVolume", dbValue);                        // 믹서 값 변경
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);                  // 볼륨값 로컬 변수로 저장
    }

    public void SetBGMVolume(float sliderValue)
    {
        float dbValue = (Mathf.Log10(sliderValue) * 20) + maxVolumeDb;
        if (sliderValue == 0)
            dbValue = -80f;
        
        mainMixer.SetFloat("BGMVolume", dbValue);
        PlayerPrefs.SetFloat("BGMVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dbValue = (Mathf.Log10(sliderValue) * 20) + maxVolumeDb;
        if (sliderValue == 0)
            dbValue = -80f;
        
        mainMixer.SetFloat("SFXVolume", dbValue);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }
}
