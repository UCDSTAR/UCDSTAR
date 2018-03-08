using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoboxController : MonoBehaviour {

    //Clock
    public GameObject timeText;
    public GameObject detailsCanvas;
    public GameObject telemetryPanel; //this will be copied into the scene
    public Button toggleButton;
    private DateTime utcTime;

    //Dummy data
    TelemetryData pressureData = new TelemetryData(Severity.CRITICAL, "Pressure", 1, "psia");
    TelemetryData temperatureData = new TelemetryData(Severity.WARNING, "Temperature", 60, "F");
    TelemetryData fanData = new TelemetryData(Severity.NOMINAL, "Fan techometer", 900, "RPM");
    TelemetryData oxygenData = new TelemetryData(Severity.NOMINAL, "Oxygen level", 90, "%");

    // Use this for initialization
    void Start () {
        detailsCanvas.SetActive(false);
        toggleButton.onClick.AddListener(doToggle);
	}

    //Called when the toggle button is clicked
    void doToggle()
    {
        detailsCanvas.SetActive(!detailsCanvas.activeInHierarchy);
        if (detailsCanvas.activeInHierarchy)
        {
            createTelemetryPanel(temperatureData, 0);
            createTelemetryPanel(pressureData, 1);
            createTelemetryPanel(fanData, 2);
            createTelemetryPanel(oxygenData, 3);
        }
    }
	
	// Update is called once per frame
	void Update () {
        utcTime = DateTime.UtcNow;
        String timeStr = utcTime.ToString("t") + " UTC";
        timeText.GetComponent<TextMesh>().text = timeStr; 
    }

    void createTelemetryPanel(TelemetryData t, int index)
    {
        GameObject panelClone = Instantiate(telemetryPanel, detailsCanvas.GetComponent<Transform>(), false);
        panelClone.GetComponent<RectTransform>().sizeDelta = new Vector2((float)2.5, (float)0.75);
        panelClone.GetComponent<RectTransform>().localPosition = new Vector3((float)0, (float)(1.5-index), 0);
       
        //Set color based on severity
        switch (t.getSeverity())
        {
            case Severity.NOMINAL:
                panelClone.GetComponent<Image>().color = Color.green;
                break;
            case Severity.WARNING:
                panelClone.GetComponent<Image>().color = Color.yellow;
                break;
            case Severity.CRITICAL:
                panelClone.GetComponent<Image>().color = Color.red;
                break;
        }

        //Set text
        panelClone.GetComponentInChildren<TextMesh>().text = t.getDataText();
    }
}
