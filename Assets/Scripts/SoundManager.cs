using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Eat sounds (by tag)")]
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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.6f;
    }

    public void PlayEatSound(string tag)
    {
        PlayEatSound(EatableTypeExtensions.TagToType(tag));
    }

    public void PlayEatSound(Eatable.EatableType type)
    {
        AudioClip clip = GetClipByType(type);
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    private AudioClip GetClipByType(Eatable.EatableType type)
    {
        return type switch
        {
            Eatable.EatableType.Human => eatHuman,
            Eatable.EatableType.Animal => eatAnimal,
            Eatable.EatableType.Plant => eatPlant,
            Eatable.EatableType.Vehicle => eatVehicle,
            Eatable.EatableType.Building => eatBuilding,
            Eatable.EatableType.Street => eatStreet,
            _ => eatOther,
        };
    }
}
