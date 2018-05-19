using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Button voiceHelpButton;
    public Image enlargedImage;
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
        if (ProcedureController.isImageExpanded && ProcedureController.currentImageType == ProcedureController.ImageType.HELP)
            HideVoiceHelp();
        else
            ShowVoiceHelp();
    }

    void ShowVoiceHelp()
    {
        //Set enlarged image to taskboard image and show
        enlargedImage.sprite = Resources.Load<Sprite>("Images/commands");
        enlargedImage.preserveAspect = true;
        enlargedImage.gameObject.SetActive(true);

        //Update button icon
        voiceHelpButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/mic-white");

        //Update state
        ProcedureController.isImageExpanded = true;
        ProcedureController.currentImageType = ProcedureController.ImageType.HELP;
    }

    void HideVoiceHelp()
    {
        //Hide image
        enlargedImage.gameObject.SetActive(false);

        //Update button icon
        voiceHelpButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/mic-black");

        //Update state
        ProcedureController.isImageExpanded = false;
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
