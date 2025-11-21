using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class WeatherController : MonoBehaviour
{
    [Header("날씨 제어 관련 스크립트")]
    [SerializeField] public Volume volume;                  // 씬의 볼룸 오브젝트
    [SerializeField] public Light Light;                    // 씬의 햇빛 오브젝트

    [Header("날씨 제어 관련 오브젝트")]
    [SerializeField] public GameObject RainEffect;          // 비 이펙트

    [Header("날씨 제어 관련 변수")]
    [SerializeField] public float rotationSpeed;            // 태양 회전 속도 (시간흐름)
    [SerializeField] public float CloudyTime;               // 흐림 날씨 지속 시간
    [SerializeField] public float RainyTime;                // 폭우 날씨 지속 시간

    /* 날씨 제어 설정을 담아둘 변수 */
    private HDRISky hdriSky;
    private Fog fog;
    private CloudLayer cloudLayer;

    private float timer;
    private bool isRainy;
    private float nextWeather;

    const float cloudyBrightness = 1500f;
    const float rainyBrightness = 100f;

    void Start()
    {
        if (volume.profile.TryGet<HDRISky>(out var hdriSky))
            this.hdriSky = hdriSky;
        if (volume.profile.TryGet<Fog>(out var fog))
            this.fog = fog;
        if (volume.profile.TryGet<CloudLayer>(out var cloudLayer))
            this.cloudLayer = cloudLayer;

        isRainy = true;
        timer = 0f;
    }

    void Update()
    {
        RotateSky();
        ChangeWeather();
    }

    public bool GetIsRainy() { return isRainy;}
    
    /* 하늘 회전 */
    void RotateSky()
    {
        if(hdriSky != null)
        {
            // 하늘 회전
            float currentRotation = hdriSky.rotation.value;
            float newRotation = currentRotation + rotationSpeed * Time.deltaTime;

            hdriSky.rotation.value += newRotation;

            // 노출 변화 (시간에 따른 밝기 변화)
            float sinTime = Mathf.Sin(Time.time);
            float exposureValue = (sinTime + 1f) / 2f; 
            
            hdriSky.exposure.value = exposureValue;
        }
    }

    /* 날씨 변경 판정 체크 */
    void ChangeWeather()
    {
        timer += Time.deltaTime;

        if (timer >= nextWeather)
        {
            timer = 0f;
            SmoothTransitionWeather();

            if(isRainy)
                Debug.Log("Change to Rainy");
            else
                Debug.Log("Change to Cloudy");
                
            NextChangeTime();
        }
    }

    void NextChangeTime()
    {
        if(isRainy)
        {
            nextWeather = RainyTime;
        }
        else
        {
            nextWeather = CloudyTime;
        }
    }

    /* 날씨 변경 */
    IEnumerator SmoothTransitionWeather()
    {
        float targetBrightness = isRainy ? cloudyBrightness : rainyBrightness;
        float nowBrightness = Light.intensity;

        while(Mathf.Abs(Light.intensity - targetBrightness) > 0.1f)
        {
            Light.intensity = Mathf.Lerp(nowBrightness, targetBrightness, 5.0f * Time.deltaTime);
        }

        if(isRainy)
            RainEffect.SetActive(true);
        else
            RainEffect.SetActive(false);
        isRainy = !isRainy;
        
        yield return null;
    }
}
