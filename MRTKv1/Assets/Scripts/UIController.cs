using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Button voiceHelpButton;
    public Image voiceHelpImg;
    public GameObject dragPanel;
    public GameObject ui;

    // Use this for initialization
    void Start () {
        //Set up toolbar buttons
        voiceHelpButton.onClick.AddListener(ToggleVoiceHelp);
        ShowTapToPlace_s();
    }

    void ShowHelp_s()
    {
        ShowVoiceHelp();
    }

    void HideHelp_s()
    {
        HideVoiceHelp();
    }

    void ToggleVoiceHelp()
    {
        if (voiceHelpImg.IsActive())
            HideVoiceHelp();
        else
            ShowVoiceHelp();
    }

    void ShowVoiceHelp()
    {
        voiceHelpImg.gameObject.SetActive(true);
        voiceHelpButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/mic-white");
    }

    void HideVoiceHelp()
    {
        voiceHelpImg.gameObject.SetActive(false);
        voiceHelpButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/mic-black");
    }

    void ShowTapToPlace_s()
    {
        //Enable panel and collider box
        dragPanel.SetActive(true);
        //ui.GetComponent(TapToPlace).enabled = true;
    }

    void HideTapToPlace_s()
    {
        //Disable panel and collider box
        //ui.GetComponent<TapToPlace>().enabled = false;
        dragPanel.SetActive(false);
    }
}
