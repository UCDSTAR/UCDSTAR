using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TelemetryController : MonoBehaviour
{
    //These are set in the editor
    public GameObject timeText;
    public GameObject detailsPanel;
    public GameObject telemetryNotification; //this will be copied into the scene

    //Clock data
    private DateTime utcTime;

    //Telemetry data
    private const int MAX_NOTIFICATIONS = 4;
    private const float REFRESH_RATE = 10.0f;
    private TelemetryData[] notifications;

    // Use this for initialization
    void Start()
    {
        detailsPanel.SetActive(false);
        StartCoroutine(GetTelemetryData());
    }

    //Called when voice command is triggered
    void ShowNotifications_s()
    {
        detailsPanel.SetActive(true);

        //Clear and add notifications
    }

    //Called when voice command is triggered
    void HideNotifications_s()
    {
        detailsPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        utcTime = DateTime.UtcNow;
        String timeStr = utcTime.ToString("t") + " UTC";
        timeText.GetComponentInChildren<Text>().text = timeStr;
    }

    //Create a telemetry notification object and add it to the details panel at the given index
    //Index 0 is at the top of the panel
    void CreateTelemetryNotification(TelemetryData t, int index)
    {
        GameObject panelClone = Instantiate(telemetryNotification, detailsPanel.GetComponent<Transform>(), false);
        panelClone.GetComponent<RectTransform>().localPosition = new Vector3(0, (float)(1.425 - 0.95 * index), 0);

        //Set color based on severity
        switch (t.GetSeverity())
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
        //panelClone.GetComponentInChildren<Text>().text = t.GetDataText();
    }

    //Repeatedly read telemetry data from server using an HTTP GET request
    IEnumerator GetTelemetryData()
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://hrvip.ucdavis.edu/share/UCDSUITS/api/telemetry/recent.json"))
            {
                yield return www.Send();

                if (www.isError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // Show results as text
                    Debug.Log(www.downloadHandler.text);
                    JSONData jsonData = JSONData.CreateFromJSON(www.downloadHandler.text);
                }
            }

            //Wait 10 seconds before pulling data again
            yield return new WaitForSecondsRealtime(REFRESH_RATE);
        }
    }

    void GenerateNotificationsArray(JSONData jsonData)
    {
        TelemetryData P_sub = new TelemetryData("Sub pressure", jsonData.p_sub, "psia", 2, 4);
    }
}
