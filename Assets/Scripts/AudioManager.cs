using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource _source;

    private void Awake()
    {
        Instance = this;
    }
    public void PlaySound(AudioClip clip) {
        _source.PlayOneShot(clip);
    }
}
