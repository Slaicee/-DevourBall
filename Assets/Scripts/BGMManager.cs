using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource bgmSource;         
    public AudioClip[] bgmList;            
    public int currentIndex = 0;

    public AudioSource rainSource;         
    public AudioClip rainLoop;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 播放初始BGM
        PlayBGM(0);

        // 播放雨声（循环）
        if (rainSource != null && rainLoop != null)
        {
            rainSource.clip = rainLoop;
            rainSource.loop = true;
            rainSource.Play();
        }
    }

    private void Update()
    {
        if (!bgmSource.isPlaying)
        {
            PlayNextBGM();
        }
    }

    public void PlayBGM(int index)
    {
        if (bgmList == null || bgmList.Length == 0) return;

        currentIndex = index % bgmList.Length;
        bgmSource.clip = bgmList[currentIndex];
        bgmSource.loop = false;   
        bgmSource.Play();
    }

    public void PlayNextBGM()
    {
        PlayBGM(currentIndex + 1);
    }
}
