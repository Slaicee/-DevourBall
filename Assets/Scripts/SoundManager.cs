using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("吞噬音效（按Tag对应拖入）")]
    public AudioClip eatHuman;
    public AudioClip eatAnimal;
    public AudioClip eatPlant;
    public AudioClip eatVehicle;
    public AudioClip eatBuilding;
    public AudioClip eatStreet;
    public AudioClip eatOther;

    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;

        // 创建一个独立AudioSource用于播放短音效
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.6f;
    }

    public void PlayEatSound(string tag)
    {
        AudioClip clip = GetClipByTag(tag);
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    AudioClip GetClipByTag(string tag)
    {
        switch (tag)
        {
            case "Human": return eatHuman;
            case "Animal": return eatAnimal;
            case "Plant": return eatPlant;
            case "Vehicle": return eatVehicle;
            case "Building": return eatBuilding;
            case "Street": return eatStreet;
            default: return eatOther;
        }
    }
}
