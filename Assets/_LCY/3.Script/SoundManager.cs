using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum BGMTrackName
{
    None = 0,
    StartGame,
    Tutorial,
    Volcano,
    Chemical,
}

[System.Serializable]
public class MusicTrack
{
    public BGMTrackName trackName;
    public AudioClip audioClip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance = null;

    [Header("BGM")]
    [SerializeField] private List<MusicTrack> musicTracks;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Header("SFX")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("SFX 3D Defaults")]
    [SerializeField] private float sfxMinDistance = 1f;
    [SerializeField] private float sfxMaxDistance = 20f;
    [SerializeField] private AudioRolloffMode sfxRolloff = AudioRolloffMode.Linear;

    // 기존 BGM 재생 상태
    private AudioSource bgmA;
    private AudioSource bgmB;
    private bool isPlaying = true;

    // 기존 단일 SFX소스 : 더 이상 사용 안 함 (호환용)
    private AudioSource sfxSource;

    private Dictionary<BGMTrackName, AudioClip> musicClipDict;

    // 추가 SFX 풀 & 트래킹
    [Header("SFX Pool Settings")]
    [SerializeField] private int sfxInitialPool = 8;
    [SerializeField] private bool sfx2D = true;

    private readonly List<AudioSource> sfxPool = new();
    private int nextSfxId = 1;

    // id -> source
    private readonly Dictionary<int, AudioSource> sfxById = new();
    // clip -> ids(동시에 같은 클립 여러번 재생 가능)
    private readonly Dictionary<AudioClip, HashSet<int>> sfxIdsByClip = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSound();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSound()
    {
        // BGM
        bgmA = gameObject.AddComponent<AudioSource>();
        bgmB = gameObject.AddComponent<AudioSource>();
        bgmA.outputAudioMixerGroup = bgmMixerGroup;
        bgmB.outputAudioMixerGroup = bgmMixerGroup;
        bgmA.playOnAwake = false;
        bgmB.playOnAwake = false;
        bgmA.ignoreListenerPause = true;
        bgmB.ignoreListenerPause = true;

        // (호환용) 단일 SFX 소스 - 더 이상 쓰지 않음
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxSource.playOnAwake = false;

        // SFX 풀 생성
        for (int i = 0; i < Mathf.Max(1, sfxInitialPool); i++)
            sfxPool.Add(CreateSfxSource(i));

        // BGM 딕셔너리
        musicClipDict = new Dictionary<BGMTrackName, AudioClip>();
        foreach (var track in musicTracks)
            musicClipDict[track.trackName] = track.audioClip;
    }

    private AudioSource CreateSfxSource(int index)
    {
        var go = new GameObject($"SFX_{index:00}");
        go.transform.SetParent(transform, false);
        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = sfxMixerGroup;
        src.playOnAwake = false;
        src.spatialBlend = sfx2D ? 0f : 1f; // 2D/3D 선택
        src.ignoreListenerPause = true;
        return src;
    }

    private void Start()
    {
        PlayBGM(BGMTrackName.StartGame);
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }

    private void OnDisable()
    { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Scn.StartGame":
                PlayBGM(BGMTrackName.StartGame);
                break;
            case "Scn.Tutoral":
                PlayBGM(BGMTrackName.Tutorial);
                break;
            case "Scn.Volcano":
                PlayBGM(BGMTrackName.Volcano);
                break;
            case "Scn.Chemical":
                PlayBGM(BGMTrackName.Chemical);
                break;
            default:
                PlayBGM(BGMTrackName.None);
                break;
        }
    }

    private AudioSource RentSfxSource()
    {
        // 재생 중이 아닌 소스 우선
        foreach (var src in sfxPool)
        {
            if (src == null) continue;
            if (!src.isPlaying) return src;
        }

        // 모두 사용 중이면 하나 더 생성(스파이크 방지용)
        var extra = CreateSfxSource(sfxPool.Count);
        sfxPool.Add(extra);
        return extra;
    }

    public void PlayBGM(BGMTrackName trackName, bool loop = true)
    {
        if (!musicClipDict.TryGetValue(trackName, out var clipToPlay) || clipToPlay == null) return;

        // 양쪽 소스 모두 확인
        if ((bgmA && bgmA.isPlaying && bgmA.clip == clipToPlay) ||
            (bgmB && bgmB.isPlaying && bgmB.clip == clipToPlay))
            return;

        StopAllCoroutines();
        NormalizeBgmState(); // 추가
        StartCoroutine(Crossfade(clipToPlay, loop));
    }

    private void NormalizeBgmState()
    {
        bool a = bgmA && bgmA.isPlaying && bgmA.clip != null;
        bool b = bgmB && bgmB.isPlaying && bgmB.clip != null;
        if (a && !b) isPlaying = true;
        else if (!a && b) isPlaying = false;
        else
        {
            float av = bgmA ? bgmA.volume : 0f;
            float bv = bgmB ? bgmB.volume : 0f;
            isPlaying = av >= bv;
        }
    }

    private IEnumerator Crossfade(AudioClip newClip, bool loop)
    {
        var from = isPlaying ? bgmA : bgmB;
        var to = isPlaying ? bgmB : bgmA;

        float fromStart = (from && from.isPlaying) ? from.volume : 1f; // 실제 현재값 사용
        float toStart = 0f;

        to.clip = newClip;
        to.loop = loop;
        to.volume = toStart;
        to.Play();

        float t = 0f;
        while (t < crossfadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / crossfadeDuration);

            // equal-power: 중앙에서 덜 꺼지고 덜 붕
            float a = Mathf.Cos(k * Mathf.PI * 0.5f);
            float b = Mathf.Sin(k * Mathf.PI * 0.5f);

            if (from) from.volume = a * fromStart;
            to.volume = Mathf.Lerp(toStart, 1f, b);

            yield return null;
        }

        if (from) { from.Stop(); from.clip = null; from.volume = 1f; }
        to.volume = 1f;
        isPlaying = !isPlaying;
    }


    // SFX API
    /// <summary>
    /// SFX 재생(인스턴스 반환). 같은 클립이 여러 번 겹쳐도 각각 정지 가능.
    /// </summary>

    //[SerializeField, Min(0f)] float loopFadeOut = 0.25f;
    //private int _loopSfxId = 0; // 루프 사운드 인스턴스 ID
    public int PlaySFX(
        AudioClip clip, bool loop = false, float volume = 1f, float pitch = 1f,
        float? spatialBlendOverride = null, float? minDistance = null, float? maxDistance = null, AudioRolloffMode? rolloff = null)
    {
        if (!clip) return 0;

        var src = RentSfxSource();
        ApplySpatial(src, spatialBlendOverride, minDistance, maxDistance, rolloff);

        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.loop = loop;
        src.Play();

        int id = nextSfxId++;
        sfxById[id] = src;

        if (!sfxIdsByClip.TryGetValue(clip, out var set))
        {
            set = new HashSet<int>();
            sfxIdsByClip[clip] = set;
        }
        set.Add(id);

        if (!loop) StartCoroutine(ReleaseWhenDone(id, clip, src));
        return id;
    }


    // 월드 좌표에서 재생
    public int PlaySFXAt(
        AudioClip clip, Vector3 worldPos, bool loop = false, float volume = 1f, float pitch = 1f,
        float? spatialBlendOverride = null, float? minDistance = null, float? maxDistance = null, AudioRolloffMode? rolloff = null)
    {
        if (!clip) return 0;

        var src = RentSfxSource();
        src.transform.SetParent(transform, false);
        src.transform.position = worldPos;

        ApplySpatial(src, spatialBlendOverride, minDistance, maxDistance, rolloff);

        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.loop = loop;
        src.Play();

        int id = nextSfxId++;
        sfxById[id] = src;
        if (!sfxIdsByClip.TryGetValue(clip, out var set)) { set = new HashSet<int>(); sfxIdsByClip[clip] = set; }
        set.Add(id);

        if (!loop) StartCoroutine(ReleaseWhenDone(id, clip, src));
        return id;
    }


    // 타겟을 따라다니며 재생
    public int PlaySFXOn(
        AudioClip clip, Transform target, bool loop = false, float volume = 1f, float pitch = 1f,
        float? spatialBlendOverride = null, float? minDistance = null, float? maxDistance = null, AudioRolloffMode? rolloff = null)
    {
        if (!clip || !target) return 0;

        var id = PlaySFX(clip, loop, volume, pitch, spatialBlendOverride, minDistance, maxDistance, rolloff);
        var src = sfxById[id];
        src.transform.SetParent(target, false);
        src.transform.localPosition = Vector3.zero;
        return id;
    }


    /// <summary>
    /// (호환) 기존 시그니처 유지. 반환 id는 쓰지 않음.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, loop: false);
    }

    /// <summary>
    /// 특정 '클립'으로 재생 중인 모든 인스턴스를 정지.
    /// </summary>
    public void StopSFX(AudioClip clip, bool immediate = true, float fadeOutSeconds = 0.1f)
    {
        if (!clip) return;
        if (!sfxIdsByClip.TryGetValue(clip, out var set) || set.Count == 0) return;

        // 복사해두고 순회(정지 중에 set이 바뀌므로)
        var ids = new List<int>(set);
        foreach (var id in ids)
            StopSFXById(id, immediate, fadeOutSeconds);
    }

    /// <summary>
    /// 단일 인스턴스만 정지(id는 PlaySFX 반환값).
    /// </summary>
    public void StopSFXById(int id, bool immediate = true, float fadeOutSeconds = 0.1f)
    {
        if (!sfxById.TryGetValue(id, out var src) || src == null) return;

        var clip = src.clip; // 정리 전에 미리 보관
        if (immediate || fadeOutSeconds <= 0f)
        {
            src.Stop();
            CleanupSfxId(id, clip);
        }
        else
        {
            StartCoroutine(FadeOutAndCleanup(id, clip, src, fadeOutSeconds));
        }
    }

    /// <summary>
    /// 모든 SFX 정지.
    /// </summary>
    public void StopAllSFX(bool immediate = true, float fadeOutSeconds = 0.1f)
    {
        var ids = new List<int>(sfxById.Keys);
        foreach (var id in ids)
            StopSFXById(id, immediate, fadeOutSeconds);
    }

    // ───────── 내부 유틸 ─────────
    private IEnumerator ReleaseWhenDone(int id, AudioClip clip, AudioSource src)
    {
        // 재생 종료까지 대기
        while (src && src.isPlaying) yield return null;
        if (src)
        {
            src.Stop();
            CleanupSfxId(id, clip);
        }
    }

    private IEnumerator FadeOutAndCleanup(int id, AudioClip clip, AudioSource src, float sec)
    {
        float t = 0f;
        float start = src.volume;
        while (t < sec && src)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, t / sec);
            yield return null;
        }
        if (src)
        {
            src.Stop();
            src.volume = start; // 다음 재사용 시 원복
            CleanupSfxId(id, clip);
        }
    }

    private void CleanupSfxId(int id, AudioClip clip)
    {
        if (sfxById.TryGetValue(id, out var src) && src)
        {
            src.clip = null;
            // 풀에 자동 반환: 이번 구현에선 같은 오브젝트의 AudioSource들을 재사용하므로 Stop만 하면 끝.
            // (필요 시 여기서 추가 초기화)
        }
        sfxById.Remove(id);

        if (clip && sfxIdsByClip.TryGetValue(clip, out var set))
        {
            set.Remove(id);
            if (set.Count == 0) sfxIdsByClip.Remove(clip);
        }
    }

    private void ApplySpatial(AudioSource src,
    float? spatialBlendOverride, float? minDistance, float? maxDistance, AudioRolloffMode? rolloff)
    {
        float blend = spatialBlendOverride.HasValue
            ? Mathf.Clamp01(spatialBlendOverride.Value)
            : (sfx2D ? 0f : 1f);

        src.spatialBlend = blend;

        if (blend > 0f) // 3D일 때만 거리/롤오프 적용
        {
            src.rolloffMode = rolloff ?? sfxRolloff;
            src.minDistance = Mathf.Max(0.01f, (minDistance ?? sfxMinDistance));
            src.maxDistance = Mathf.Max(src.minDistance + 0.01f, (maxDistance ?? sfxMaxDistance));
        }
    }


#if UNITY_EDITOR
    [ContextMenu("DEBUG/Print SFX Pool Status")]
    private void DebugPrintSfxPool()
    {
        int free = 0, busy = 0, nul = 0;
        foreach (var s in sfxPool)
        {
            if (s == null) { nul++; continue; }
            if (s.isPlaying) busy++; else free++;
        }
        Debug.Log($"[SFX Pool] total={sfxPool.Count}, free={free}, busy={busy}, null={nul}");
    }
#endif
}