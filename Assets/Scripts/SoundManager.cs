using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public AudioSource Bgm;
    public AudioSource Se;
    [SerializeField] private Transform sePrefab;

    [Header("Button Click SE")]
    public AudioClip buttonCilckSe;

    [HideInInspector]
    public bool muteStat;

    public float pre_bgm_Volume;
    public float pre_se_Volume;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Debug.Log("More than 1 GridGM instance!");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    public void PlaySE(AudioClip au)
    {
        Transform sfxTransform = Instantiate(sePrefab);
        sfxTransform.GetComponent<AudioSource>().resource = buttonCilckSe;
        sfxTransform.GetComponent<AudioSource>().volume = Se.volume;
        Destroy(sfxTransform.gameObject, 5f);
    }
}
