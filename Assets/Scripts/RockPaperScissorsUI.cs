using UnityEngine;
using UnityEngine.UI;

public class RockPaperScissorsUI : MonoBehaviour
{
    [SerializeField]
    private Button scissorsButton;
    [SerializeField]
    private Button paperButton;
    [SerializeField]
    private Button rockButton;
    [SerializeField]
    private Button goFirstButton;
    [SerializeField]
    private Button goSecondButton;
    [SerializeField]
    private Text hintText;

    void Start() {
        InitializePanel();

        rockButton.onClick.AddListener(()=>{
            scissorsButton.gameObject.SetActive(false);
            paperButton.gameObject.SetActive(false);
            rockButton.enabled = false;
            //Debug.Log("You go Rock");
            GameManager.Instance.DoRPSRpc((int)GameManager.Instance.GetLocalPlayerType(), 0);
        });
        paperButton.onClick.AddListener(()=>{
            scissorsButton.gameObject.SetActive(false);
            paperButton.enabled = false;
            rockButton.gameObject.SetActive(false);
            //Debug.Log("You go Paper");
            GameManager.Instance.DoRPSRpc((int)GameManager.Instance.GetLocalPlayerType(), 1);
        });
        scissorsButton.onClick.AddListener(()=>{
            scissorsButton.enabled = false;
            paperButton.gameObject.SetActive(false);
            rockButton.gameObject.SetActive(false);
            //Debug.Log("You go Scissors");
            GameManager.Instance.DoRPSRpc((int)GameManager.Instance.GetLocalPlayerType(), 2);
        });

        goFirstButton.onClick.AddListener(()=>{
            Hide();
            GameManager.Instance.DetermineCurrentPlayablePlayerTypeRpc((int)GameManager.Instance.GetLocalPlayerType(), true);
        });
        goSecondButton.onClick.AddListener(()=>{
            Hide();
            GameManager.Instance.DetermineCurrentPlayablePlayerTypeRpc((int)GameManager.Instance.GetLocalPlayerType(), false);
        });
    }

    public void InitializePanel(){
        gameObject.SetActive(true);
        scissorsButton.gameObject.SetActive(true);
        paperButton.gameObject.SetActive(true);
        rockButton.gameObject.SetActive(true);
        goFirstButton.gameObject.SetActive(false);
        goSecondButton.gameObject.SetActive(false);
        hintText.text = "Choose one";
    }

    public void DisplayPanel(string hintString, bool isTie, bool isWin){
        if(isTie) {  // Tie or Beginning
            hintText.text = hintString;
            scissorsButton.enabled = true;
            paperButton.enabled = true;
            rockButton.enabled = true;

            scissorsButton.gameObject.SetActive(true);
            paperButton.gameObject.SetActive(true);
            rockButton.gameObject.SetActive(true);

            goFirstButton.gameObject.SetActive(false);
            goSecondButton.gameObject.SetActive(false);
        }
        else if(!isTie && isWin){
            hintText.text = hintString;
            scissorsButton.gameObject.SetActive(false);
            paperButton.gameObject.SetActive(false);
            rockButton.gameObject.SetActive(false);

            goFirstButton.gameObject.SetActive(true);
            goSecondButton.gameObject.SetActive(true);
        } else if(!isTie && !isWin){
            hintText.text = hintString;
            scissorsButton.gameObject.SetActive(false);
            paperButton.gameObject.SetActive(false);
            rockButton.gameObject.SetActive(false);

            goFirstButton.gameObject.SetActive(false);
            goSecondButton.gameObject.SetActive(false);
        }

        gameObject.GetComponent<Animator>().SetBool("IsShow", true);
    }

    public void Hide(){
        gameObject.GetComponent<Animator>().SetBool("IsShow", false);
        gameObject.SetActive(false);
    }
}
