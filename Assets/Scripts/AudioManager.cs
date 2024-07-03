using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;//creamos una instancia para estar accediendo desde otras clases
    [SerializeField] private AudioSource _source;//hacemos referencia a un AudioSource

    private void Awake()
    {
        Instance = this;//definimos la instancia con el mismo objeto
    }
    public void PlaySound(AudioClip clip) {//creamos un metodo que reproducira un sonido, y como parametro sera el sonido
        _source.PlayOneShot(clip);//reproducimos un sonido atraves del AudioSource
    }
}
