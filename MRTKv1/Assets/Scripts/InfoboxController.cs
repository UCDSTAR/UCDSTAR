using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoboxController : MonoBehaviour {

    //Clock
    public GameObject timeText;
    public GameObject detailsCanvas;
    private DateTime utcTime;
    private bool detailsActive;

    //Dummy data
    TelemetryData pressureData = new TelemetryData(Severity.CRITICAL, "Pressure", 1, "psia");
    TelemetryData temperatureData = new TelemetryData(Severity.WARNING, "Temperature", 60, "F");
    TelemetryData fanData = new TelemetryData(Severity.NOMINAL, "Fan techometer", 900, "RPM");

    // Use this for initialization
    void Start () {
        detailsCanvas.SetActive(false);
        detailsActive = false;
	}
	
	// Update is called once per frame
	void Update () {
        utcTime = DateTime.UtcNow;
        String timeStr = utcTime.ToString("t") + " UTC";
        timeText.GetComponent<TextMesh>().text = timeStr;

        if (!detailsActive && detailsCanvas.activeSelf)
        {
            detailsActive = true;
            createTelemetryButton(temperatureData, 0);
            createTelemetryButton(pressureData, 1);
        }
    }

    void createTelemetryButton(TelemetryData t, int index)
    {
        //Create button object
        GameObject button = new GameObject();
        button.transform.parent = detailsCanvas.GetComponent<Transform>();
        button.AddComponent<RectTransform>();
        button.AddComponent<Button>();
        button.AddComponent<Image>();
        button.transform.localPosition = new Vector3(0, (float)(0.5+index), 0);
        button.GetComponent<RectTransform>().sizeDelta = new Vector2((float)2.5, (float)0.75);

        //Set color based on severity
        switch (t.getSeverity())
        {
            case Severity.NOMINAL:
                button.GetComponent<Image>().color = Color.green;
                break;
            case Severity.WARNING:
                button.GetComponent<Image>().color = Color.yellow;
                break;
            case Severity.CRITICAL:
                button.GetComponent<Image>().color = Color.red;
                break;
        }

        //Create text object
        GameObject dataText = new GameObject();
        dataText.transform.parent = button.GetComponent<Transform>();
        dataText.AddComponent<RectTransform>();
        dataText.AddComponent<Text>();
        dataText.transform.localPosition = new Vector3((float)1.5, 2, 0);
        dataText.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 30);
        dataText.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        dataText.GetComponent<Text>().text = "Button";
    }
}
