using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResourcesManager : MonoBehaviour
{
    public static GameResourcesManager Instance { get; private set; }

    public Dictionary<string, Texture2D> imageDictionary = new();
    public Dictionary<string, Texture2D> memberColorImageDictionary = new();
    public Dictionary<string, Texture2D> memberBloomLevelImageDictionary = new();
    public Dictionary<string, CardJsonData> cardDataDictionary = new();
    
    [SerializeField]private Animator dataLoadingStatusUI;
    [Header("Loading Panel")]
    [SerializeField]
    private GameObject loadingUI;
    [SerializeField]
    private UnityEngine.UI.Slider progressSlider;
    [SerializeField]
    private Text progressValueText;
    private string alertMessage = "";
    private bool isDataLoaded = false;
    
    private string playerSelectedDeckPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("More than 1 GameResourcesManager instance!");
            DestroyImmediate(gameObject);
        }
    }

    private async void Start()
    {
        LoadResoueceAndSwitchToNextScene();
        alertMessage = "The game data is incomplete,\n please check and try again.\n Click \'No\' will quit the game.";
    }

    void Update()
    {
        // Check if data is loaded and a key is pressed
        if (isDataLoaded && Input.anyKey)
        {
            //SceneManager.LoadScene(1);
            isDataLoaded = false;
            SwitchToPlayScene(1);
        }
    }

    #region load game data
    public async Task<bool> LoadAllCardImagesIntoDictionary()
    {
        string relativePath = "Textures/Cards";
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        // Make sure the directory exists
        if (Directory.Exists(fullPath))
        {
            string[] fileEntries = Directory.GetFiles(fullPath, "*.png");
            foreach (string filePath in fileEntries)
            {
                // Read image files.
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);

                // Use the file name as index of dictionary
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                imageDictionary[fileName] = texture;
            }
            Debug.Log("Images loaded: " + imageDictionary.Count);
            //DebugConsoleForBuildWindow.Instance.Log("Images loaded: " + imageDictionary.Count);

            return true;
        }
        else
        {
            Debug.LogError("Directory does not exist: " + fullPath);
            //DebugConsoleForBuildWindow.Instance.LogError("Directory does not exist: " + fullPath);

            alertMessage += "\n\nDirectory does not exist: " + fullPath;
            return false;
        }
    }

    private async Task<bool> LoadAllMemberColorImagesIntoDictionary()
    {
        string relativePath = "Textures/MemberColors";
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        // Make sure the directory exists
        if (Directory.Exists(fullPath))
        {
            string[] fileEntries = Directory.GetFiles(fullPath, "*.png");
            foreach (string filePath in fileEntries)
            {
                // Read image files.
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);

                // Use the file name as index of dictionary
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                memberColorImageDictionary[fileName] = texture;
                //Debug.Log("Member color images loading... " + fileName);
            }
            Debug.Log("Member color images loaded: " + memberColorImageDictionary.Count);
            //DebugConsoleForBuildWindow.Instance.Log("Member color images loaded: " + memberColorImageDictionary.Count);

            return true;
        }
        else
        {
            Debug.LogError("Directory does not exist: " + fullPath);
            //DebugConsoleForBuildWindow.Instance.LogError("Directory does not exist: " + fullPath);

            alertMessage += "\n\nDirectory does not exist: " + fullPath;
            return false;
        }
    }

    private async Task<bool> LoadAllMemberBloomLevelImagesIntoDictionary()
    {
        string relativePath = "Textures/BloomLevels";
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        // Make sure the directory exists
        if (Directory.Exists(fullPath))
        {
            string[] fileEntries = Directory.GetFiles(fullPath, "*.png");
            foreach (string filePath in fileEntries)
            {
                // Read image files.
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);

                // Use the file name as index of dictionary
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                memberBloomLevelImageDictionary[fileName] = texture;
                //Debug.Log("Member color images loading... " + fileName);
            }
            Debug.Log("Member bloom images loaded: " + memberBloomLevelImageDictionary.Count);
            //DebugConsoleForBuildWindow.Instance.Log("Member bloom images loaded: " + memberBloomLevelImageDictionary.Count);

            return true;
        }
        else
        {
            Debug.LogError("Directory does not exist: " + fullPath);
            //DebugConsoleForBuildWindow.Instance.LogError("Directory does not exist: " + fullPath);

            alertMessage += "\n\nDirectory does not exist: " + fullPath;
            return false;
        }
    }

    private async Task<bool> LoadAllCardDataIntoDictionary()
    {
        string filePath = Path.Combine(Application.dataPath, "Resources/CardDataList.json");

        if (File.Exists(filePath))
        {
            // Read the JSON file content
            string jsonContent = File.ReadAllText(filePath);
            // 解析JSON並轉換為陣列
            var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<CardsJsonDataCollection>("{\"CardJsonDatas\":" + jsonContent + "}");
            cardDataDictionary = jsonData.CardJsonDatas;
            Debug.Log("Card Data loaded: " + cardDataDictionary.Count);
            //DebugConsoleForBuildWindow.Instance.Log("Card Data loaded: " + cardDataDictionary.Count);

            // Print the dictionary
            /*foreach (var kvp in cardDataDictionary)
            {
                Debug.Log($"Id: {kvp.Key}, Name: {kvp.Value.Name}");
            }*/

            return true;
        }
        else
        {
            Debug.LogError("File does not exist: " + filePath);
            //DebugConsoleForBuildWindow.Instance.LogError("File does not exist: " + filePath);
            alertMessage += "\n\nFile does not exist: " + filePath;
            return false;
        }
    }

    /// <summary>
    /// Compare the dimensions of the 'cardDataDictionary' and the 'imageDictionary'.<br></br>
    /// 1. Output true if the dimensions are the same and none of them are 0; otherwise, output false.<br></br>
    /// 2. Output different data names if the dimensions are not the same.
    /// </summary>
    /// <returns>bool</returns>
    private async Task<bool> CompareDictionaryId(){
        if(cardDataDictionary.Count > 0 && imageDictionary.Count > 0) {
            if(cardDataDictionary.Count != imageDictionary.Count){
                string errorCardKey = "";
                foreach(var dd in imageDictionary){
                    if(!cardDataDictionary.ContainsKey(dd.Key)){
                        errorCardKey += $"Unknow Key: {dd.Key}";
                        Debug.Log($"Unknow Key: {dd.Key}");
                        //DebugConsoleForBuildWindow.Instance.LogError($"Unknow Key: {dd.Key}");
                    }
                }
                
                alertMessage += "\n\n" + errorCardKey;
                return false;
            } else {
                Debug.Log($"Card data count equals to card images: {cardDataDictionary.Count} \n ==============================================");
                return true;
            }
        } else {
            return false;
        }
    }

    private async void LoadResoueceAndSwitchToNextScene(){
        bool loadCardImageBool = await LoadAllCardImagesIntoDictionary();
        bool loadColorImageBool = await LoadAllMemberColorImagesIntoDictionary();
        bool loadBloomImageBool = await LoadAllMemberBloomLevelImagesIntoDictionary();
        bool loadCardDataBool = await LoadAllCardDataIntoDictionary();
        bool compareDictionaryDataBool = await CompareDictionaryId();

        if(loadCardImageBool && loadColorImageBool && loadBloomImageBool && loadCardDataBool && compareDictionaryDataBool){
            // switch to 'MainMenuScene'
            isDataLoaded = true;
            dataLoadingStatusUI.SetBool("IsLoaded", isDataLoaded);
        } else {
            CustomYesNoDialog.Instance.DisplayAlertDialog("The game data is incomplete,\n please check and try again.\n Click \'No\' will quit the game.\n\n" + alertMessage,
            async () => {
                // Reload data again.
                LoadResoueceAndSwitchToNextScene();
            },
            () => {
                // Close game window
                #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
        }
    }
    #endregion load game data

    public void SetPlayerSelectedDeck(string path)
    {
        playerSelectedDeckPath = path;
    }

    public string GetPlayerSelectedDeck()
    {
        return playerSelectedDeckPath;
    }

    #region switch to other scene
    private void SwitchToPlayScene(int sceneIndex)
    {
        if(GameResourcesManager.Instance.GetPlayerSelectedDeck() != ""){
            //soundmanager.PlaySE(soundmanager.buttonCilckSe);
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
    #endregion switch to other scene
}
