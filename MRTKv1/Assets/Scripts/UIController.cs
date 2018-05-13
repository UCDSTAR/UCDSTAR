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
    }

    void ShowVoiceHelp_s()
    {
        ShowVoiceHelp();
    }

    void HideVoiceHelp_s()
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
    }

    void HideVoiceHelp()
    {
        voiceHelpImg.gameObject.SetActive(false);
    }

    void ShowTapToPlace_s()
    {
        //Enable panel and collider box
        dragPanel.SetActive(true);
        ui.GetComponent<BoxCollider>().enabled = true;
    }

    void HideTapToPlace_s()
    {
        //Disable panel and collider box
        ui.GetComponent<BoxCollider>().enabled = false;
        dragPanel.SetActive(false);
    }
}
