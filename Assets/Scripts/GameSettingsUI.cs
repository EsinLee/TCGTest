using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingsUI : MonoBehaviour
{
    #region Parameter declare
    public static GameSettingsUI Instance { get; private set; }

    [Header("Settings Panel")]
    [SerializeField]
    private GameObject SettingsPanel;
    
    [SerializeField]
    private Button SettingsThumbButton;

    [SerializeField]
    private Button ResumeButton;

    [SerializeField]
    private Button MenuButton;

    [SerializeField]
    private Button ExitButton;

    [Header("Sound control")]
    [SerializeField]
    private SoundManager soundmanager;

    [SerializeField]
    private Button muteButton;

    [SerializeField]
    private Sprite muteImg;

    [SerializeField]
    private Sprite unmuteImg;

    [SerializeField]
    private Slider bgmSlider;

    [SerializeField]
    private Slider seSlider;

    [SerializeField]
    private Text bgmValueText;

    [SerializeField]
    private Text seValueText;

    [Header("Resolution")]
    [SerializeField]
    private TMP_Dropdown resolutionSizeDropDown;
    public static readonly string[] resolutionSizeList = new string[] {"1920 * 1080", "1280 * 720", "854 * 480"};

    [Header("Loading Panel")]
    [SerializeField]
    private GameObject loadingUI;
    [SerializeField]
    private UnityEngine.UI.Slider progressSlider;
    [SerializeField]
    private Text progressValueText;
    
    #endregion    

    public event EventHandler<OnResolutionResetEventArgs> OnResolutionReset; // 遊戲前置
    public class OnResolutionResetEventArgs : EventArgs
    {
        public int[] newResolution;
        public bool isWindowed;
    }

    void Awake(){
        if (Instance == null) {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        } else {
            Debug.Log("More than 1 GridGM instance!");
            DestroyImmediate(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        VolumeChanged();

        SettingsThumbButton.onClick.AddListener(()=>{
            if(SettingsPanel.activeSelf){
                HideSettingsPanel();
            } else {
                DisplaySettingsPanel();
            }
        });

        ResumeButton.onClick.AddListener(()=>{
            if(SettingsPanel.activeSelf){
                HideSettingsPanel();
            }
        });

        ExitButton.onClick.AddListener(()=>{
            CustomYesNoDialog.Instance.DisplayAlertDialog("Are you sure you want to exit the game?",
            async ()=>{
                CloseApplication();
            },
            ()=>{
                // Do nothing
            });
        });

        bgmSlider.onValueChanged.AddListener((float value)=>{
            soundmanager.Bgm.volume = value;
            bgmValueText.text = (int)(value * 100) + "%";
        });
        seSlider.onValueChanged.AddListener((float value)=>{
            soundmanager.Se.volume = value;
            seValueText.text = (int)(value * 100) + "%";
        });
        muteButton.onClick.AddListener(()=>{
            MuteClick();
        });

        InitializeResolutionSizeDropdownControl();
        // Set dropdown event
        resolutionSizeDropDown.onValueChanged.AddListener(async (int index) => {
            int width = int.Parse(resolutionSizeList[index].Split(" * ")[0]);
            int height = int.Parse(resolutionSizeList[index].Split(" * ")[1]);
            Screen.SetResolution(width, height, FullScreenMode.Windowed);

            OnResolutionReset?.Invoke(this, new OnResolutionResetEventArgs(){
                newResolution = new int[]{width, height},
                isWindowed = false
            });

        });
        resolutionSizeDropDown.value = 1;
        resolutionSizeDropDown.onValueChanged.Invoke(1);
    }

    public void DisplaySettingsPanel()
    {
        soundmanager.PlaySE(soundmanager.buttonCilckSe);
        if (SettingsPanel != null)
            SettingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        soundmanager.PlaySE(soundmanager.buttonCilckSe);
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
    }

    public void CloseApplication()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // If current scene dosn't connected to server.
        if(currentSceneIndex == 0){
            Application.Quit();

            // If running in the unity editor, stop playing the Scene.
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        } else {
            StartCoroutine(WaitForShutdownCoroutine(5.0f));
            /*
            if (NetworkManager.Singleton == null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            NetworkManager.Singleton.Shutdown();
            float timeout = 5.0f; // 5秒後超時
            float startTime = Time.time;

            while (NetworkManager.Singleton.ShutdownInProgress && Time.time - startTime < timeout)
            {
                Debug.LogWarning("連線關閉中");
                // 可選地加入一些延遲，以防止緊密迴圈
                System.Threading.Thread.Sleep(100);
            }

            if (NetworkManager.Singleton.ShutdownInProgress)
            {
                Debug.LogWarning("關閉過程超時。");
            }
            else
            {
                Debug.Log("關閉過程成功完成。");

                Application.Quit();

                // If running in the unity editor, stop playing the Scene.
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }*/
        }
    }

    IEnumerator WaitForShutdownCoroutine(float timeout)
    {
        float startTime = Time.time;
        NetworkManager.Singleton.Shutdown();
        while (NetworkManager.Singleton.ShutdownInProgress && Time.time - startTime < timeout)
        {
            Debug.LogWarning("Disconnecting...");
            yield return new WaitForSeconds(0.1f); // Prevents tight loops
        }

        if (NetworkManager.Singleton.ShutdownInProgress)
        {
            Debug.LogWarning("Disconnect time out.");
        }
        else
        {
            Debug.Log("Disconnected");
            Application.Quit();

            // If running in the unity editor, stop playing the Scene.
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }

    public void VolumeChanged()
    {
        soundmanager.Se.volume = seSlider.value;
        seValueText.text = (int)(seSlider.value * 100) + "%";
        soundmanager.Bgm.volume = bgmSlider.value;
        bgmValueText.text = (int)(bgmSlider.value * 100) + "%";
        soundmanager.muteStat = false;
        muteButton.image.sprite = soundmanager.muteStat ? muteImg : unmuteImg;
        /*
                for (int i = 0; i < scrollbar_bg.Length; i++) {
                    scrollbar_bg [i].GetComponent<Image> ().color = new Color32 (230, 230, 0, 255);
                }*/
    }

    public void MuteClick()
    {
        soundmanager.PlaySE(soundmanager.buttonCilckSe);
        //print("Mute Button clicked.");
        soundmanager.muteStat = !soundmanager.muteStat;
        muteButton.image.sprite = soundmanager.muteStat ? muteImg : unmuteImg;
        if (soundmanager.muteStat)
        {
            soundmanager.pre_bgm_Volume = soundmanager.Bgm.volume;
            soundmanager.pre_se_Volume = soundmanager.Se.volume;
            soundmanager.Se.volume = 0;
            soundmanager.Bgm.volume = 0;
            /*
                        for (int i = 0; i < scrollbar_bg.Length; i++) {
                            scrollbar_bg [i].GetComponent<Image> ().color = new Color32 (150, 150, 150, 255);
                        }*/
        }
        else
        {
            soundmanager.Bgm.volume = soundmanager.pre_bgm_Volume;
            soundmanager.Se.volume = soundmanager.pre_se_Volume;
            /*
                        for (int i = 0; i < scrollbar_bg.Length; i++) {
                            scrollbar_bg [i].GetComponent<Image> ().color = new Color32 (230, 230, 0, 255);
                        }*/
        }
    }

    private void InitializeResolutionSizeDropdownControl() {
        List<TMP_Dropdown.OptionData> deckOptionData = resolutionSizeDropDown.options;
        // Modify options of deck list Dropdown control.
        deckOptionData.Clear();
        foreach(var reso in resolutionSizeList){
            deckOptionData.Add(new TMP_Dropdown.OptionData(reso));
        }
        resolutionSizeDropDown.options = deckOptionData;
    }

    private IEnumerator LoadingScene_Coroutine(int sceneIndex)
    {
        progressSlider.value = 0;
        loadingUI.SetActive(true);
        int displayProgress = 0;
        int toProgress = 0;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        if ((progressValueText != null) && (progressSlider != null))
        {
            // 場景暫時停駐
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                toProgress = (int)operation.progress * 100;
                while (displayProgress < toProgress)
                {
                    ++displayProgress;
                    progressSlider.value = (float)displayProgress / 100;
                    progressValueText.text = $"{displayProgress}%";

                    yield return null;
                }
            }

            toProgress = 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                progressSlider.value = (float)displayProgress / 100;
                progressValueText.text = "Loading..." + displayProgress + "%";

                yield return null;
            }
            // 繼續切換場景
            operation.allowSceneActivation = true;
        }
        else
        {
            yield return null;
        }
    }
}
