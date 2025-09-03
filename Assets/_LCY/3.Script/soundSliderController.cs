using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class soundSliderController : MonoBehaviour
{
    [Header("AudioMixer & Sliders")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider bgmVolume;
    [SerializeField] private Slider sfxVolume;

    [Header("Exposed Parameter Names")]
    [SerializeField] private string masterParam = "MASTER";
    [SerializeField] private string bgmParam = "BGM";
    [SerializeField] private string sfxParam = "SFX";

    // dB 변환용
    private const float MIN_DB = -80f;    // 믹서 최저치
    private const float EPS = 0.0001f; // 0 방지

    private void OnEnable()
    {
        // 슬라이더 초기값을 "현재 믹서 값"으로 동기화
        SyncFromMixer();

        // 리스너 등록
        if (masterVolume) masterVolume.onValueChanged.AddListener(SetMasterVolume);
        if (bgmVolume) bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        if (sfxVolume) sfxVolume.onValueChanged.AddListener(SetSFXVolume);
    }

    private void OnDisable()
    {
        // 리스너 해제(중복 방지)
        if (masterVolume) masterVolume.onValueChanged.RemoveListener(SetMasterVolume);
        if (bgmVolume) bgmVolume.onValueChanged.RemoveListener(SetBGMVolume);
        if (sfxVolume) sfxVolume.onValueChanged.RemoveListener(SetSFXVolume);
    }

    // ───────── 초기 동기화 ─────────
    private void SyncFromMixer()
    {
        if (masterVolume) masterVolume.SetValueWithoutNotify(GetMixer01(masterParam));
        if (bgmVolume) bgmVolume.SetValueWithoutNotify(GetMixer01(bgmParam));
        if (sfxVolume) sfxVolume.SetValueWithoutNotify(GetMixer01(sfxParam));
    }

    private float GetMixer01(string param)
    {
        if (!mixer) return 1f;
        if (mixer.GetFloat(param, out float db))
            return Mathf.Clamp01(DbToLin(db));
        return 1f; // 파라미터가 없으면 기본 1
    }

    // ───────── UI 콜백 ─────────
    public void SetMasterVolume(float value) => SetMixer(masterParam, value);
    public void SetBGMVolume(float value) => SetMixer(bgmParam, value);
    public void SetSFXVolume(float value) => SetMixer(sfxParam, value);

    private void SetMixer(string param, float linear01)
    {
        if (!mixer) return;
        mixer.SetFloat(param, LinToDb(Mathf.Clamp01(linear01)));
    }

    // ───────── 유틸(선형↔dB) ─────────
    private static float LinToDb(float v) => (v > EPS) ? Mathf.Log10(v) * 20f : MIN_DB;
    private static float DbToLin(float db) => Mathf.Pow(10f, db / 20f);

    // (선택) 초기화 버튼용
    public void ResetToDefault()
    {
        if (masterVolume) masterVolume.SetValueWithoutNotify(1f);
        if (bgmVolume) bgmVolume.SetValueWithoutNotify(1f);
        if (sfxVolume) sfxVolume.SetValueWithoutNotify(1f);

        SetMasterVolume(1f);
        SetBGMVolume(1f);
        SetSFXVolume(1f);
    }
}
