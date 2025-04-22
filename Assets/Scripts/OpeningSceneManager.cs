using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class OpeningSceneManager : MonoBehaviour
{
    public Animator GameLogoAnimator;

    [Header("Root Button Panel")]
    [SerializeField]
    private GameObject rootButtonPanel;
    [SerializeField]
    private Button rootButtonPlay;
    [SerializeField]
    private Button rootButtonDeck;

    [SerializeField]
    private SoundManager soundmanager;

    [Header("Network Manager Panel")]
    [SerializeField]
    private GameObject networkManagerUI;

    [Header("Game Match Panel")]
    [SerializeField]
    private GameObject gameMatchUI;

    [Header("Loading Panel")]
    [SerializeField]
    private GameObject loadingUI;
    [SerializeField]
    private UnityEngine.UI.Slider progressSlider;
    [SerializeField]
    private Text progressValueText;

    [Header("Deck Builder Panel")]
    [SerializeField]
    private GameObject deckBuilderPanel;

    private string currentUIStat = "Root Button Panel";

    void Start()
    {
        currentUIStat = "Root Button Panel";

        if (GameLogoAnimator != null)
        {
            GameLogoAnimator.Play("Idle");
        }

        rootButtonDeck.onClick.AddListener(() => {
            DisplayDeckBuilderPanel();
        });

        rootButtonPlay.onClick.AddListener(() => {
            DisplayConnectionPanel();
        });
    }

    public void DisplayConnectionPanel()
    {
        if (networkManagerUI != null)
            currentUIStat = "Network Manager Panel";
            rootButtonPanel.SetActive(false);
            networkManagerUI.SetActive(true);
            //SwitchToPlayScene(1);
    }

    public void DisplayGameMatchPanel()
    {
        if (networkManagerUI != null)
            currentUIStat = "Game Match Panel";
            networkManagerUI.SetActive(false);
            gameMatchUI.SetActive(true);
            //SwitchToPlayScene(1);
    }

    public void DisplayDeckBuilderPanel()
    {
        if (deckBuilderPanel != null)
            currentUIStat = "Deck Builder Panel";
            rootButtonPanel.SetActive(false);
            //deckBuilderPanel.SetActive(true);
            deckBuilderPanel.transform.localPosition = new Vector3(0,0,0);
    }

    public void SwitchToPlayScene(int sceneIndex)
    {
        if(GameResourcesManager.Instance.GetPlayerSelectedDeck() != ""){
            soundmanager.PlaySE(soundmanager.buttonCilckSe);
            //SceneManager.LoadScene(sceneIndex);
            StartCoroutine(LoadingScene_Coroutine(sceneIndex));
            /*if (playSceneName != "")
            {
                LoadingScene(playSceneName);
                //SceneManager.LoadScene(playSceneName);
            }*/
        } else {
            CustomAlertDialog.Instance.DisplayAlertDialog("No deck selected.", 1500);
        }
    }

    public IEnumerator LoadingScene_Coroutine(int sceneIndex)
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

    public void BackToRootControlPanel(){
        currentUIStat = "Root Button Panel";
        rootButtonPanel.SetActive(true);
        //deckBuilderPanel.SetActive(false);
        deckBuilderPanel.transform.localPosition = new Vector3(0,1080,0);
        networkManagerUI.SetActive(false);
        gameMatchUI.SetActive(false);
    }

    public void BackToNetworkManagerUIPanel(){
        rootButtonPanel.SetActive(true);
        //deckBuilderPanel.SetActive(false);
        deckBuilderPanel.transform.localPosition = new Vector3(0,1080,0);
        networkManagerUI.SetActive(false);
        gameMatchUI.SetActive(false);
    }
}
