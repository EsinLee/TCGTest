using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderCardFilterPanel : MonoBehaviour
{
    [Header("Color")]
    public Toggle colorWhiteToggle;
    public Toggle colorGreenToggle;
    public Toggle colorRedToggle;
    public Toggle colorBlueToggle;
    public Toggle colorPurpleToggle;
    public Toggle colorYellowToggle;
    public Toggle colorNullToggle;
    public Button colorSelectAllButton;
    public Button colorCancelAllButton;
    
    [Header("Type")]
    public Toggle typeDebutToggle;
    public Toggle typeFirstToggle;
    public Toggle typeSecondToggle;
    public Toggle typeSpotToggle;
    public Toggle typeBuzzToggle;
    public Toggle typeOshiToggle;
    public Button typeSelectAllButton;
    public Button typeCancelAllButton;
    
    [Header("Support card")]
    public Toggle supStaffToggle;
    public Toggle supEventToggle;
    public Toggle supItemToggle;
    public Toggle supFanToggle;
    public Toggle supMascotToggle;
    public Toggle supToolToggle;
    public Button supSelectAllButton;
    public Button supCancelAllButton;

    [Header("Cheer card")]
    public Toggle cheerToggle;

    [Header("Reset")]
    public Button resetButton;
}
