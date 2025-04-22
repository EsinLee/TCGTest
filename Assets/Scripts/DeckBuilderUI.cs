using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using SFB;
using System;
using System.Text.RegularExpressions;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField]
    private Animator deckSwitchAnimator;
    [SerializeField]
    private Button deckSwitchButton;

    [Header("Card List")]
    [SerializeField]
    private GameObject cardListPrefabObj;
    [SerializeField]
    private GameObject cardListContent;
    
    [Header("Oshi Card")]
    [SerializeField]
    private MainDeckOshiCard mainDeckOshiCard;

    [Header("Main Deck")]
    [SerializeField]
    private GameObject mainDeckListPrefabObj;
    [SerializeField]
    private GameObject mainDeckListContent;
    [SerializeField]
    private Text mainDeckMaxNumText;
    [SerializeField]
    private Text mainDeckCurrentNumText;

    [Header("Cheer Deck")]
    [SerializeField]
    private GameObject cheerDeckListPrefabObj;
    [SerializeField]
    private GameObject cheerDeckListContent;
    [SerializeField]
    private Text cheerDeckMaxNumText;
    [SerializeField]
    private Text cheerDeckCurrentNumText;
    
    [Header("Sprite & Text")]
    [SerializeField]
    private Text[] colorTextList_w_g_r_b_p_y; // white, green, red, blue, purple, yellow
    [SerializeField]
    private Color[] colorDressList_w_g_r_b_p_y; // white, green, red, blue, purple, yellow, null, white_green
    [SerializeField]
    private Image previewCardImage;
    [SerializeField]
    private Sprite emptyCardImage;
    [SerializeField]
    private TextMeshProUGUI SelectedDeckTextMeshShowInMenuPanel;

    [Header("Filter")]
    [SerializeField]
    private DeckBuilderCardFilterPanel deckBuilderCardFilterPanel;
    private List<string> filterColorString = new ();
    private List<string> filterBloomLevelString = new ();
    private List<string> filterCardTypeString = new ();

    [Header("Json io UI")]
    [SerializeField]
    private Button saveAsJsonButton;
    [SerializeField]
    private Button loadJsonFileButton;
    [SerializeField]
    private InputField deckNameInputField;
    [SerializeField]
    private Dropdown deckListDropDown;
    [SerializeField]
    private Button addNewDeckButton;
    [SerializeField]
    private Button deleteDeckButton;
    [SerializeField]
    private Button clearDeckButton;

    private Dictionary<string, GameObject> cardListObj = new Dictionary<string, GameObject>(); // for display all of card to pick.
    private string deckName;
    private string oshiCard;
    private Dictionary<string, Color> cardDressColor = new Dictionary<string, Color>();
    private Dictionary<string, int> mainCardDeck = new Dictionary<string, int>();
    private Dictionary<string, GameObject> mainCardDeckListObj = new Dictionary<string, GameObject>();
    private Dictionary<string, int> cheerCardDeck = new Dictionary<string, int>();
    private Dictionary<string, GameObject> cheerCardDeckListObj = new Dictionary<string, GameObject>();
    private Dictionary<string, string> customDeckList = new Dictionary<string, string>();
    private string deckListFilePath = Path.Combine(Application.dataPath, "Resources/CustomDeck_List.json");
    //private Dictionary<string, System.Object> dicForOutputToJson = new Dictionary<string, System.Object>();
    private int mainDeckMaxmumNum;
    private int cheerDeckMaxmumNum;

    async void Start()
    {
        // Maximum number of card deck.
        mainDeckMaxmumNum = 50;
        cheerDeckMaxmumNum = 20;
        // Reset desk num UI
        mainDeckCurrentNumText.text = "0" + "/";
        mainDeckMaxNumText.text = mainDeckMaxmumNum.ToString();
        cheerDeckCurrentNumText.text = "0" + "/";
        cheerDeckMaxNumText.text = cheerDeckMaxmumNum.ToString();

        // Set deck view switch button
        deckSwitchButton.onClick.AddListener(()=>{
            bool currentStat = deckSwitchAnimator.GetBool("IsMainDeck");
            deckSwitchAnimator.SetBool("IsMainDeck", !currentStat);
        });

        string[] colorStringArray = new string[] {"white", "green","red","blue","purple","yellow","null","white_green",};
        if(colorStringArray.Length != colorDressList_w_g_r_b_p_y.Length) Debug.LogError("-->> colorStringArray.Length != colorDressColorList_w_g_r_b_p_y.Length");
        // Initialize dress color dictioary
        for(int i = 0; i < colorStringArray.Length; i++){
            Color currentColor = colorDressList_w_g_r_b_p_y[i];
            cardDressColor.Add(colorStringArray[i], currentColor);
        }
        
        // white, green, red, blue, purple, yellow
        // Reset cheer desk UI
        foreach(Text numTex in colorTextList_w_g_r_b_p_y){
            numTex.text = "0";
        }

        // Populate Card List
        if (GameResourcesManager.Instance.imageDictionary.Count <= 0)
        {
            GameResourcesManager.Instance.LoadAllCardImagesIntoDictionary();
            PopulateCardList();
        }
        else
        {
            PopulateCardList();
        }

        #region member color ui
        Toggle[] colorToggles = new Toggle[]{
            deckBuilderCardFilterPanel.colorWhiteToggle,
            deckBuilderCardFilterPanel.colorGreenToggle,
            deckBuilderCardFilterPanel.colorRedToggle,
            deckBuilderCardFilterPanel.colorBlueToggle,
            deckBuilderCardFilterPanel.colorPurpleToggle,
            deckBuilderCardFilterPanel.colorYellowToggle,
            deckBuilderCardFilterPanel.colorNullToggle,
        };
        //if(colorStringArray.Length != colorToggles.Length) Debug.LogError("-->> colorStringArray.Length != colorToggles.Length");
        // Set value change event to Toggle buttons (member color)
        for(int i = 0; i < colorToggles.Length; i++){
            string currentColor = colorStringArray[i];
            colorToggles[i].onValueChanged.AddListener((isOn) => UpdataFilterKeyWords(isOn, "color", currentColor));
        }
        deckBuilderCardFilterPanel.colorSelectAllButton.onClick.AddListener(() => {
            for(int i = 0; i < colorToggles.Length; i++){
                colorToggles[i].isOn = true;
            }
            //filterColorString.Clear();
        });
        deckBuilderCardFilterPanel.colorCancelAllButton.onClick.AddListener(() => {
            for(int i = 0; i < colorToggles.Length; i++){
                //filterColorString.Add(colorStringArray[i]);
                colorToggles[i].isOn = false;
            }
        });
        #endregion

        #region bloom level ui
        Toggle[] bloomLevelToggles = new Toggle[]{
            deckBuilderCardFilterPanel.typeDebutToggle,
            deckBuilderCardFilterPanel.typeFirstToggle,
            deckBuilderCardFilterPanel.typeSecondToggle,
            deckBuilderCardFilterPanel.typeSpotToggle
        };
        string[] bloomLevelStringArray = new string[] {"Debut", "1st","2nd","Spot"};
        // Set value change event to Toggle buttons (bloom level)
        for(int i = 0; i < bloomLevelStringArray.Length; i++){
            string currentbloomLevelString = bloomLevelStringArray[i];
            bloomLevelToggles[i].onValueChanged.AddListener((isOn) => UpdataFilterKeyWords(isOn, "bloomLevel", currentbloomLevelString));
        }
        deckBuilderCardFilterPanel.typeSelectAllButton.onClick.AddListener(() => {
            for(int i = 0; i < bloomLevelStringArray.Length; i++){
                bloomLevelToggles[i].isOn = true;
            }
            //filterBloomLevelString.Clear();
        });
        deckBuilderCardFilterPanel.typeCancelAllButton.onClick.AddListener(() => {
            for(int i = 0; i < bloomLevelStringArray.Length; i++){
                //filterBloomLevelString.Add(bloomLevelStringArray[i]);
                bloomLevelToggles[i].isOn = false;
            }
        });
        #endregion

        #region card type ui
        Toggle[] cardTypeToggles = new Toggle[]{
            deckBuilderCardFilterPanel.typeBuzzToggle,
            deckBuilderCardFilterPanel.typeOshiToggle,
            deckBuilderCardFilterPanel.supStaffToggle,
            deckBuilderCardFilterPanel.supEventToggle,
            deckBuilderCardFilterPanel.supItemToggle,
            deckBuilderCardFilterPanel.supFanToggle,
            deckBuilderCardFilterPanel.supMascotToggle,
            deckBuilderCardFilterPanel.supToolToggle,
            deckBuilderCardFilterPanel.cheerToggle
        };
        string[] cardTypeStringArray = new string[] {"Buzzホロメン", "推しホロメン", "サポート・スタッフ", "サポート・イベント", "サポート・アイテム", "サポート・ファン", "サポート・マスコット", "サポート・ツール", "エール"};
        // Set value change event to Toggle buttons (card type)
        for(int i = 0; i < cardTypeStringArray.Length; i++){
            string currentCardTypeString = cardTypeStringArray[i];
            cardTypeToggles[i].onValueChanged.AddListener((isOn) => UpdataFilterKeyWords(isOn, "cardType", currentCardTypeString));
        }
        deckBuilderCardFilterPanel.supSelectAllButton.onClick.AddListener(() => {
            for(int i = 0; i < cardTypeStringArray.Length; i++){
                cardTypeToggles[i].isOn = true;
            }
            //filterCardTypeString.Clear();
        });
        deckBuilderCardFilterPanel.supCancelAllButton.onClick.AddListener(() => {
            for(int i = 0; i < cardTypeStringArray.Length; i++){
                //filterCardTypeString.Add(cardTypeStringArray[i]);
                cardTypeToggles[i].isOn = false;
            }
        });
        #endregion

        // set button event (reset all of filter toggles)
        deckBuilderCardFilterPanel.resetButton.onClick.AddListener(() => {
            for(int i = 0; i < colorToggles.Length; i++){
                colorToggles[i].isOn = true;
            }
            for(int i = 0; i < bloomLevelToggles.Length; i++){
                bloomLevelToggles[i].isOn = true;
            }
            for(int i = 0; i < cardTypeToggles.Length; i++){
                cardTypeToggles[i].isOn = true;
            }
            //filterCardTypeString.Clear();
        });

        // set button event (save deck data as json file)
        addNewDeckButton.onClick.AddListener(async ()=> {
            string newName = deckNameInputField.text;
            if(!(newName.Length > 0)){
                CustomAlertDialog.Instance.DisplayAlertDialog("Deck name cannot be null.",1500);
                return;
            }
            if(cheerCardDeck.Values.Sum() == cheerDeckMaxmumNum && mainCardDeck.Values.Sum() == mainDeckMaxmumNum && oshiCard != ""){
                // Generate new Json file name as the Key in dictionary.
                string deckKey = Guid.NewGuid().ToString();

                // Write data into Json file
                string filePath = Path.Combine(Application.dataPath,$"Resources/{deckKey}.json");
                CustomDeckJsonModel data = new CustomDeckJsonModel {Oshi = oshiCard, MainDeck = mainCardDeck, CheerDeck = cheerCardDeck};
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented); // Serialize the DataContainer to a JSON string
                await SaveJsonFile(filePath, jsonString);
                
                // modify deck list Json file
                customDeckList.Add(deckKey, newName);
                jsonString = JsonConvert.SerializeObject(customDeckList, Formatting.Indented); // Serialize the DataContainer to a JSON string
                await SaveJsonFile(deckListFilePath, jsonString);
                RefreshDeckListDropdownControl();
                deckListDropDown.value = deckListDropDown.options.Count() - 1;
                
                CustomAlertDialog.Instance.DisplayAlertDialog("Data saved successfully.",1500);
            }else {
                Debug.Log($"Can not save data. oshi: {oshiCard} - main: {mainCardDeck.Values.Sum()} - cheer: {cheerCardDeck.Values.Sum()}");
                CustomAlertDialog.Instance.DisplayAlertDialog($"Can not save data. \noshi: {oshiCard} \nmain: {mainCardDeck.Values.Sum()} - cheer: {cheerCardDeck.Values.Sum()}", 1500);
            }
        });

        // set button event (save deck data as json file)
        saveAsJsonButton.onClick.AddListener(async ()=> {
            if(cheerCardDeck.Values.Sum() == cheerDeckMaxmumNum && mainCardDeck.Values.Sum() == mainDeckMaxmumNum && oshiCard != ""){
                int selectedDeckIndex = deckListDropDown.value;
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(customDeckList);
                string selectedDeckName = deckListDropDown.options[selectedDeckIndex].text;
                string filePath = Path.Combine(Application.dataPath,$"Resources/{list[selectedDeckIndex].Key}.json");
                CustomDeckJsonModel data = new CustomDeckJsonModel {Oshi = oshiCard, MainDeck = mainCardDeck, CheerDeck = cheerCardDeck};
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented); // Serialize the DataContainer to a JSON string
                await SaveJsonFile(filePath, jsonString);

                // if deck name changed then rewrite the custom deck Json.
                string newName = deckNameInputField.text;
                if(selectedDeckName != newName) {
                    customDeckList[list[selectedDeckIndex].Key] = newName;
                    jsonString = JsonConvert.SerializeObject(customDeckList, Formatting.Indented); // Serialize the DataContainer to a JSON string
                    await SaveJsonFile(deckListFilePath, jsonString);
                    deckListDropDown.options[selectedDeckIndex].text = newName;
                    deckListDropDown.RefreshShownValue();
                }

                CustomAlertDialog.Instance.DisplayAlertDialog("Data saved successfully.",1500);
                //File.WriteAllText(filePath, jsonString); // Write the JSON string to a file
            }else {
                Debug.Log($"Can not save data. oshi: {oshiCard} - main: {mainCardDeck.Values.Sum()} - cheer: {cheerCardDeck.Values.Sum()}");
                CustomAlertDialog.Instance.DisplayAlertDialog($"Can not save data. \noshi: {oshiCard} \nmain: {mainCardDeck.Values.Sum()} - cheer: {cheerCardDeck.Values.Sum()}", 1500);
            }
        });

        // set button event (load date of json file to dictioary.)
        loadJsonFileButton.onClick.AddListener(async ()=> {
            var path = StandaloneFileBrowser.OpenFilePanel("Open JSON File", "", "json", false);
            if (path.Length > 0){
                // 讀取JSON文件內容
                string jsonContent = File.ReadAllText(path[0]);
                try{
                    await LoadDeckDataFromJsonFile(path[0], deckName);

                    // Generate new Json file name as the Key in dictionary.
                    string deckKey = Guid.NewGuid().ToString();

                    // Write data into Json file
                    string filePath = Path.Combine(Application.dataPath,$"Resources/{deckKey}.json");
                    CustomDeckJsonModel data = new CustomDeckJsonModel {Oshi = oshiCard, MainDeck = mainCardDeck, CheerDeck = cheerCardDeck};
                    string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented); // Serialize the DataContainer to a JSON string
                    await SaveJsonFile(filePath, jsonString);
                    
                    // modify deck list Json file
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path[0]);
                    customDeckList.Add(deckKey, fileNameWithoutExtension);
                    jsonString = JsonConvert.SerializeObject(customDeckList, Formatting.Indented); // Serialize the DataContainer to a JSON string
                    await SaveJsonFile(deckListFilePath, jsonString);
                    RefreshDeckListDropdownControl();
                    deckListDropDown.value = deckListDropDown.options.Count() - 1;
                    
                    CustomAlertDialog.Instance.DisplayAlertDialog("Data loaded successfully.",1500);
                } catch(Exception e){
                    Debug.LogError("Wrong json file.");
                    CustomAlertDialog.Instance.DisplayAlertDialog("Wrong file format.",1500);
                }
            }
        });

        // set button event (clear all data from id dictionary and gameobj dictionary)
        clearDeckButton.onClick.AddListener(async ()=> {
            await ClearEditingDeckData();
        });
        
        // set button event (delete current editing deck)
        deleteDeckButton.onClick.AddListener(async ()=> {
            CustomYesNoDialog.Instance.DisplayAlertDialog("Deletion cannot be undone,\ndo you want to continue?",
            async ()=>{
                // clear all of deck dictionary and gameobject
                await ClearEditingDeckData();

                // remove data from deck list dictionary and rewrite it
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(customDeckList);
                //Debug.Log($"list size {list.Count()}");
                int index = deckListDropDown.value;
                customDeckList.Remove(list[index].Key);
                string jsonString = JsonConvert.SerializeObject(customDeckList, Formatting.Indented); // Serialize the DataContainer to a JSON string
                await SaveJsonFile(deckListFilePath, jsonString);

                // remove option in dropdown, set dropdown value to first option
                await RefreshDeckListDropdownControl();
                //Debug.Log($"Option 0: {deckListDropDown.options[0].text}");
                if (deckListDropDown.options.Count > 0){
                    deckListDropDown.value = 0;
                    deckListDropDown.onValueChanged.Invoke(0); // Manually invoke the onValueChanged event
                } else {
                    // Load started deck or do something else...
                    deckNameInputField.text = "";
                    SelectedDeckTextMeshShowInMenuPanel.text = "";
                    // Set selected deck to GameManager
                    GameResourcesManager.Instance.SetPlayerSelectedDeck("");
                    Debug.Log($"Selected deck path : Null");
                }

                //Debug.Log($"Delete {list[index].Value}");
                CustomAlertDialog.Instance.DisplayAlertDialog($"Delete {list[index].Value}", 1500);
            },
            ()=>{
                // Do nothing
            });
        });

        // Set dropdown event
        deckListDropDown.onValueChanged.AddListener(async (int index) => {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(customDeckList);
            string path = Path.Combine(Application.dataPath, $"Resources/{list[index].Key}.json");
            await LoadDeckDataFromJsonFile(path, list[index].Value);
            SelectedDeckTextMeshShowInMenuPanel.text = list[index].Value;
            // Set selected deck to GameManager
            GameResourcesManager.Instance.SetPlayerSelectedDeck(path);
            //Debug.Log($"Selected deck path : {path}");
        });
        
        // Initialize deck list dropdown control
        await RefreshDeckListDropdownControl();
        if (deckListDropDown.options.Count > 0){
            deckListDropDown.value = 0;
            deckListDropDown.onValueChanged.Invoke(0); // Manually invoke the onValueChanged event
        } else {
            deckNameInputField.text = "";
            SelectedDeckTextMeshShowInMenuPanel.text = "";
            // Set selected deck to GameManager
            GameResourcesManager.Instance.SetPlayerSelectedDeck("");
            Debug.Log($"Selected deck path : Null");
        }

    }

    private void PopulateCardList()
    {
        if (GameResourcesManager.Instance.imageDictionary.Count > 0)
        {
            GameObject newObj;
            foreach (
                KeyValuePair<string, Texture2D> data in GameResourcesManager
                    .Instance
                    .imageDictionary
            )
            {
                Sprite sprite = Sprite.Create(
                    data.Value,
                    new Rect(0, 0, data.Value.width, data.Value.height),
                    new Vector2(0.5f, 0.5f)
                );
                newObj = (GameObject)Instantiate(cardListPrefabObj, transform);
                newObj.GetComponent<Image>().sprite = sprite;
                newObj.GetComponent<CardOfCardListPrefab>().id = data.Key;
                if (newObj.GetComponent<CardOfCardListPrefab>() != null) {
                    newObj.GetComponent<CardOfCardListPrefab>().OnPointerEnterEvent += () => {
                        previewCardImage.sprite = sprite;
                        previewCardImage.color = new Color(1,1,1,1);
                    };
                    newObj.GetComponent<CardOfCardListPrefab>().OnPointerExitEvent += () => {
                        previewCardImage.color = new Color(0,0,0,0);
                    };
                }


                //newObj.GetComponent<Image>().color = Random.ColorHSV();
                newObj.GetComponent<Button>().onClick.AddListener(() => {
                    AddCardToDack(data.Key, sprite);
                });

                // Add into dictionary
                cardListObj.Add(data.Key, newObj);

                // Set parent of new card
                newObj.transform.SetParent(cardListContent.transform);
            }
        }
        else
        {
            Debug.LogError("Couldn't generate the card list.");
            DebugConsoleForBuildWindow.Instance.LogError("Couldn't generate the card list.");
        }
    }

    private void refreshDeckListUI(string id, string cardType, Sprite sprite, int count){
        
        if(cardType == "推しホロメン"){
            mainDeckOshiCard.cardImage.sprite = sprite;
            mainDeckOshiCard.cardImage.color = new Color(1,1,1,1);
            mainDeckOshiCard.id = id;
            mainDeckOshiCard.colorImage.sprite = TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.memberColorImageDictionary[GameResourcesManager.Instance.cardDataDictionary[id].Color]);
            mainDeckOshiCard.colorImage.color = new Color(1,1,1,1);
            mainDeckOshiCard.dressImage.color = cardDressColor[GameResourcesManager.Instance.cardDataDictionary[id].Color];
        }
        else if(cardType == "ホロメン" || cardType == "Buzzホロメン"){
            mainDeckCurrentNumText.text = mainCardDeck.Values.Sum() + "/";
            if(mainCardDeckListObj.ContainsKey(id)){
                if(count > 0){
                    mainCardDeckListObj[id].GetComponent<MainDeckCardPrefab>().numText.text = count.ToString();
                }
                else {
                    Destroy(mainCardDeckListObj[id]);
                    mainCardDeckListObj.Remove(id);
                }
            } else {
                GameObject newObj = (GameObject)Instantiate(mainDeckListPrefabObj, transform);
                newObj.GetComponent<MainDeckCardPrefab>().cardImage.sprite = sprite;
                //Debug.Log(GameResourcesManager.Instance.cardDataDictionary[id].Color);
                newObj.GetComponent<MainDeckCardPrefab>().colorImage.sprite = TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.memberColorImageDictionary[GameResourcesManager.Instance.cardDataDictionary[id].Color]);
                newObj.GetComponent<MainDeckCardPrefab>().DressImage.color = cardDressColor[GameResourcesManager.Instance.cardDataDictionary[id].Color];
                newObj.GetComponent<MainDeckCardPrefab>().levelImage.sprite = TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.memberBloomLevelImageDictionary[GameResourcesManager.Instance.cardDataDictionary[id].BloomLevel]);
                newObj.GetComponent<MainDeckCardPrefab>().numText.text = count.ToString();
                if (newObj.GetComponent<MainDeckCardPrefab>() != null) {
                    newObj.GetComponent<MainDeckCardPrefab>().OnPointerEnterEvent += () => {
                        previewCardImage.sprite = sprite;
                        previewCardImage.color = new Color(1,1,1,1);
                    };
                    newObj.GetComponent<MainDeckCardPrefab>().OnPointerExitEvent += () => {
                        previewCardImage.color = new Color(0,0,0,0);
                    };
                }
                newObj.GetComponent<Button>().onClick.AddListener(() => {
                    RemoveCardFromDack(id);
                });

                mainCardDeckListObj.Add(id, newObj);
                // Set parent of new card
                newObj.transform.SetParent(mainDeckListContent.transform);
            }
        }
        else if(cardType.Contains("エール")){
            // refresh cheer deck list view
            colorTextList_w_g_r_b_p_y[0].text = cheerCardDeck.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color == "white").Sum(ids => ids.Value).ToString();
            colorTextList_w_g_r_b_p_y[1].text = cheerCardDeck.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color == "green").Sum(ids => ids.Value).ToString();
            colorTextList_w_g_r_b_p_y[2].text = cheerCardDeck.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color == "red").Sum(ids => ids.Value).ToString();
            colorTextList_w_g_r_b_p_y[3].text = cheerCardDeck.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color == "blue").Sum(ids => ids.Value).ToString();
            colorTextList_w_g_r_b_p_y[4].text = cheerCardDeck.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color == "purple").Sum(ids => ids.Value).ToString();
            colorTextList_w_g_r_b_p_y[5].text = cheerCardDeck.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color == "yellow").Sum(ids => ids.Value).ToString();
            cheerDeckCurrentNumText.text = cheerCardDeck.Values.Sum() + "/";

            // refresh cheer deck list view
            if(cheerCardDeckListObj.ContainsKey(id)){
                if(count > 0){
                    cheerCardDeckListObj[id].GetComponent<MainDeckCardPrefab>().numText.text = count.ToString();
                }
                else {
                    Destroy(cheerCardDeckListObj[id]);
                    cheerCardDeckListObj.Remove(id);
                }
            } else {
                GameObject newObj = (GameObject)Instantiate(mainDeckListPrefabObj, transform);
                newObj.GetComponent<MainDeckCardPrefab>().cardImage.sprite = sprite;
                newObj.GetComponent<MainDeckCardPrefab>().colorImage.sprite = TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.memberColorImageDictionary[GameResourcesManager.Instance.cardDataDictionary[id].Color]);
                newObj.GetComponent<MainDeckCardPrefab>().DressImage.color = cardDressColor[GameResourcesManager.Instance.cardDataDictionary[id].Color];
                newObj.GetComponent<MainDeckCardPrefab>().levelImage.color = new Color(0,0,0,0); // Hide bloom level image
                newObj.GetComponent<MainDeckCardPrefab>().numText.text = count.ToString();
                if (newObj.GetComponent<MainDeckCardPrefab>() != null) {
                    newObj.GetComponent<MainDeckCardPrefab>().OnPointerEnterEvent += () => {
                        previewCardImage.sprite = sprite;
                        previewCardImage.color = new Color(1,1,1,1);
                    };
                    newObj.GetComponent<MainDeckCardPrefab>().OnPointerExitEvent += () => {
                        previewCardImage.color = new Color(0,0,0,0);
                    };
                }
                newObj.GetComponent<Button>().onClick.AddListener(() => {
                    RemoveCardFromDack(id);
                });

                cheerCardDeckListObj.Add(id, newObj);
                // Set parent of new card
                newObj.transform.SetParent(cheerDeckListContent.transform);
            }
        }
        else if(cardType.Contains("サポート")){
            mainDeckCurrentNumText.text = mainCardDeck.Values.Sum() + "/";
            if(mainCardDeckListObj.ContainsKey(id)){
                if(count > 0){
                    mainCardDeckListObj[id].GetComponent<MainDeckCardPrefab>().numText.text = count.ToString();
                }
                else {
                    Destroy(mainCardDeckListObj[id]);
                    mainCardDeckListObj.Remove(id);
                }
            } else {
                GameObject newObj = (GameObject)Instantiate(mainDeckListPrefabObj, transform);
                newObj.GetComponent<MainDeckCardPrefab>().cardImage.sprite = sprite;
                newObj.GetComponent<MainDeckCardPrefab>().colorImage.color = new Color(0,0,0,0); // Hide color image
                newObj.GetComponent<MainDeckCardPrefab>().DressImage.color = new Color(1,1,1,1);
                newObj.GetComponent<MainDeckCardPrefab>().levelImage.color = new Color(0,0,0,0); // Hide bloom level image
                newObj.GetComponent<MainDeckCardPrefab>().numText.text = count.ToString();
                if (newObj.GetComponent<MainDeckCardPrefab>() != null) {
                    newObj.GetComponent<MainDeckCardPrefab>().OnPointerEnterEvent += () => {
                        previewCardImage.sprite = sprite;
                        previewCardImage.color = new Color(1,1,1,1);
                    };
                    newObj.GetComponent<MainDeckCardPrefab>().OnPointerExitEvent += () => {
                        previewCardImage.color = new Color(0,0,0,0);
                    };
                }
                newObj.GetComponent<Button>().onClick.AddListener(() => {
                    RemoveCardFromDack(id);
                });

                mainCardDeckListObj.Add(id, newObj);
                // Set parent of new card
                newObj.transform.SetParent(mainDeckListContent.transform);
            }
        }
        else {

        }
    }

    private void AddCardToDack(string id, Sprite sprite) {
        
        string cardType = GameResourcesManager.Instance.cardDataDictionary[id].cardType;

        if(cardType == "推しホロメン") {
            oshiCard = id;
            refreshDeckListUI(id, cardType, sprite, 0);
        } else if (cardType == "エール") {
            deckSwitchAnimator.SetBool("IsMainDeck", false);
            if(cheerCardDeck.Values.Sum() < cheerDeckMaxmumNum){
                if(cheerCardDeck.ContainsKey(id)){
                    cheerCardDeck[id] ++;
                    refreshDeckListUI(id, cardType, sprite, cheerCardDeck[id]);
                } else{
                    cheerCardDeck.Add(id, 1);
                    refreshDeckListUI(id, cardType, sprite, cheerCardDeck[id]);
                }
            }
        } else {
            deckSwitchAnimator.SetBool("IsMainDeck", true);
            if(mainCardDeck.Values.Sum() < mainDeckMaxmumNum){
                if(mainCardDeck.ContainsKey(id)){
                    if(mainCardDeck[id] < 4){
                        mainCardDeck[id] ++;
                        refreshDeckListUI(id, cardType, sprite, mainCardDeck[id]);
                    }
                } else{
                    mainCardDeck.Add(id, 1);
                    refreshDeckListUI(id, cardType, sprite, mainCardDeck[id]);
                }
            }
        }
    }

    private void RemoveCardFromDack(string id) {
        string cardType = GameResourcesManager.Instance.cardDataDictionary[id].cardType;

        if(cardType == "エール") {
            if(cheerCardDeck.ContainsKey(id)){
                cheerCardDeck[id] --;
                refreshDeckListUI(id, cardType, null, cheerCardDeck[id]);
                if(cheerCardDeck[id] <= 0) cheerCardDeck.Remove(id);
            } else{
                Debug.LogError("Error! The card is not in the cheer deck.");
            }
        } else {
            if(mainCardDeck.ContainsKey(id)){
                mainCardDeck[id] --;
                refreshDeckListUI(id, cardType, null, mainCardDeck[id]);
                if(mainCardDeck[id] <= 0) mainCardDeck.Remove(id);
            } else{
                Debug.LogError("Error! The card is not in the main deck.");
            }
        }
    }

    private void UpdataFilterKeyWords(bool isOn, string typeCode, string typeContent){
        // set filter keyword
        switch(typeCode){
            default:
            case "color":
                if(isOn){
                    //Debug.Log($"Color String Remove: {typeContent}");
                    filterColorString.Remove(typeContent);
                } else {
                    //Debug.Log($"Color String Add: {typeContent}");
                    filterColorString.Add(typeContent);
                }
                break;
            case "bloomLevel":
                if(isOn){
                    //Debug.Log($"Bloom Level String Remove: {typeContent}");
                    filterBloomLevelString.Remove(typeContent);
                } else {
                    //Debug.Log($"Bloom Level String Add: {typeContent}");
                    filterBloomLevelString.Add(typeContent);
                }
                break;
            case "cardType":
                if(isOn){
                    //Debug.Log($"Card Type string  Remove: {typeContent}");
                    filterCardTypeString.Remove(typeContent);
                } else {
                    //Debug.Log($"Card Type string Add: {typeContent}");
                    filterCardTypeString.Add(typeContent);
                }
                break;
        }

        FilterCardList();
    }

    private void FilterCardList() {
        // filter card
        IEnumerable<KeyValuePair<string, GameObject>> filteredCardListObj = Enumerable.Empty<KeyValuePair<string, GameObject>>();
        //IEnumerable<KeyValuePair<string, GameObject>> tempfilteredData = cardListObj;
        //Debug.Log($"Filter init - filteredCardListObj size - {filteredCardListObj.Count()}");
        
        // hide all of card in the list
        foreach(var obj in cardListObj){
            obj.Value.SetActive(true);
        }

        if(filterColorString.Count() > 0){
            foreach(var filterKey in filterColorString){
                filteredCardListObj = cardListObj.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].Color.Contains(filterKey));
                foreach(var obj in filteredCardListObj){
                    obj.Value.SetActive(false);
                }
            }
        }
        if(filterBloomLevelString.Count() > 0){
            foreach(var filterKey in filterBloomLevelString){
                filteredCardListObj = cardListObj.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].BloomLevel.Contains(filterKey));
                foreach(var obj in filteredCardListObj){
                    obj.Value.SetActive(false);
                }
            }
        }
        if(filterCardTypeString.Count() > 0){
            foreach(var filterKey in filterCardTypeString){
                filteredCardListObj = cardListObj.Where(ids => GameResourcesManager.Instance.cardDataDictionary[ids.Key].cardType.Contains(filterKey));
                foreach(var obj in filteredCardListObj){
                    obj.Value.SetActive(false);
                }
            }
        }

        /*
        // hide all of card in the list
        foreach(var obj in cardListObj){
            obj.Value.SetActive(true);
        }
        Debug.Log($"filteredCardListObj size - {filteredCardListObj.Count()}");
        // show filtered card in the list
        foreach(var obj in filteredCardListObj){
            obj.Value.SetActive(false);
        }
        */
    }

    private async Task RefreshDeckListDropdownControl() {
        List<Dropdown.OptionData> deckOptionData = deckListDropDown.options;
        // 讀取JSON文件內容
        string deckListJsonContent = File.ReadAllText(deckListFilePath);
        // 解析JSON並轉換為
        var deckListJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(deckListJsonContent);
        customDeckList = deckListJsonData;
        // Modify options of deck list Dropdown control.
        deckOptionData.Clear();
        foreach(var deck in deckListJsonData){
            deckOptionData.Add(new Dropdown.OptionData(deck.Value));
        }
        deckListDropDown.options = deckOptionData;
    }

    private async Task SaveJsonFile(string filePath, string jsonString)
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            await writer.WriteAsync(jsonString);
        }
    }

    private async Task LoadDeckDataFromJsonFile(string path, string deckName){
        if (File.Exists(path)){
            // 讀取JSON文件內容
            string jsonContent = File.ReadAllText(path);
            try{
                // 解析JSON並轉換為
                var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomDeckJsonModel>(jsonContent);
                await ClearEditingDeckData();
                
                deckName = deckName; 
                oshiCard = jsonData.Oshi; 
                mainCardDeck = jsonData.MainDeck; 
                cheerCardDeck = jsonData.CheerDeck;
                deckNameInputField.text = deckName;

                foreach(var card in mainCardDeck){
                    refreshDeckListUI(
                        card.Key, 
                        GameResourcesManager.Instance.cardDataDictionary[card.Key].cardType,
                        TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.imageDictionary[card.Key]),
                        card.Value);
                }
                foreach(var card in cheerCardDeck){
                    refreshDeckListUI(
                        card.Key, 
                        GameResourcesManager.Instance.cardDataDictionary[card.Key].cardType,
                        TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.imageDictionary[card.Key]),
                        card.Value);
                }
                refreshDeckListUI(
                        oshiCard, 
                        GameResourcesManager.Instance.cardDataDictionary[oshiCard].cardType,
                        TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.imageDictionary[oshiCard]),
                        0);
            } catch(Exception e){
                Debug.LogError("Wrong json file.");
                CustomAlertDialog.Instance.DisplayAlertDialog("Wrong file format.", 1500);
            }
        } else {
            Debug.LogError("File not exists.");
            CustomAlertDialog.Instance.DisplayAlertDialog("File not exists.", 1500);
        }
    }

    private async Task ClearEditingDeckData(){
        // clear main deck data list
        mainCardDeck.Clear();
        // destroy all of main deck gameobject
        foreach(var obj in mainCardDeckListObj){
            Destroy(obj.Value);
        }
        // clear main deck gameobject list
        mainCardDeckListObj.Clear();
        
        // clear cheer deck data list
        cheerCardDeck.Clear();
        // destroy all of cheer deck gameobject
        foreach(var obj in cheerCardDeckListObj){
            Destroy(obj.Value);
        }
        // clear cheer deck gameobject list
        cheerCardDeckListObj.Clear();

        // initialize ui
        oshiCard = "";
        mainDeckOshiCard.cardImage.sprite = emptyCardImage;
        mainDeckOshiCard.cardImage.color = new Color(0,0,0,0);
        mainDeckOshiCard.dressImage.color = new Color(0,0,0,0);
        mainDeckOshiCard.colorImage.color = new Color(0,0,0,0);
        foreach(Text numTex in colorTextList_w_g_r_b_p_y){
            numTex.text = "0";
        }
        mainDeckCurrentNumText.text = "0" + "/";
        mainDeckMaxNumText.text = mainDeckMaxmumNum.ToString();
        cheerDeckCurrentNumText.text = "0" + "/";
        cheerDeckMaxNumText.text = cheerDeckMaxmumNum.ToString();
    }

    private void SerializeJsonContent(
        string path,
        out string oshi,
        out Dictionary<string, int> mainDeck,
        out Dictionary<string, int> cheerDeck
    )
    {
        // 讀取JSON文件內容
        string jsonContent = File.ReadAllText(path);
        // 解析JSON並轉換為陣列
        var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomDeckJsonModel>(jsonContent);
        oshi = jsonData.Oshi;
        mainDeck = jsonData.MainDeck;
        cheerDeck = jsonData.CheerDeck;
    }
}
