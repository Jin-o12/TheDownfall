using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingTextConnecter : MonoBehaviour
{
    [Header("objects")]
    [SerializeField] TMP_Text valueText;
    [SerializeField] Slider slider;

    void Update()
    {
        valueText.text = slider.value.ToString("F0");
    }
}
