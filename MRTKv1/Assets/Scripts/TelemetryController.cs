using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TelemetryController : MonoBehaviour
{
    //These are set in the editor
    public GameObject currentTimeText;
    public GameObject detailsPanel;
    public GameObject evaTimeText;
    public GameObject telemetryNotification; //this will be copied into the scene

    //Clock data
    private DateTime utcTime;

    //Telemetry data
    private const int MAX_NOTIFICATIONS = 4;
    private const double REFRESH_RATE = 10;
    private List<TelemetryData> notificationsList;

    // Use this for initialization
    void Start()
    {
        //Hide notifications panel on startup
        detailsPanel.SetActive(true);

        //Start polling server for data
        StartCoroutine(GetTelemetryData());
    }

    //Called when voice command is triggered
    void ShowNotifications_s()
    {
        //Show panel
        detailsPanel.SetActive(true);
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
        currentTimeText.GetComponentInChildren<Text>().text = timeStr;
    }

    //Create a telemetry notification object and add it to the details panel at the given index
    //Index 0 is at the top of the panel
    void CreateTelemetryNotification(TelemetryData t, int index)
    {
        GameObject panelClone = Instantiate(telemetryNotification, detailsPanel.GetComponent<Transform>(), false);
        panelClone.GetComponent<RectTransform>().localPosition = new Vector3(0, (float)(1.425 - 0.95 * index), 0);

        //Set color based on severity
        switch (t.severity)
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
        panelClone.GetComponentInChildren<Text>().text = t.GetDataText();
    }

    //Repeatedly read telemetry data from server using an HTTP GET request and update notification data
    IEnumerator GetTelemetryData()
    {
        while (true)
        {
            string numericalStr="", switchStr="", jsonStr="";

            //Get numerical data
            using (UnityWebRequest www1 = UnityWebRequest.Get("https://hrvip.ucdavis.edu/share/UCDSUITS/api/telemetry/recent.json"))
            {
                yield return www1.Send();

                if (www1.isError)
                {
                    Debug.Log(www1.error);
                }
                else
                {
                    numericalStr = www1.downloadHandler.text;
                }
            }

            //Concatenate with switch data
            using (UnityWebRequest www2 = UnityWebRequest.Get("https://hrvip.ucdavis.edu/share/UCDSUITS/api/switch/recent.json"))
            {
                yield return www2.Send();

                if (www2.isError)
                {
                    Debug.Log(www2.error);
                }
                else
                {
                    switchStr = www2.downloadHandler.text;
                }
            }

            //Parse and update notifications
            jsonStr = numericalStr.Substring(0, numericalStr.Length - 3) + "," + switchStr.Substring(1);
            JSONData jsonData = JSONData.CreateFromJSON(jsonStr);
            evaTimeText.GetComponentInChildren<Text>().text = jsonData.t_eva; //update eva time separately
            UpdateNotificationsArray(jsonData);

            //Clear notifications
            foreach (Transform child in detailsPanel.transform)
            {
                Destroy(child.gameObject);
            }

            //Create new notifications
            for (int i = 0; i < MAX_NOTIFICATIONS; ++i)
            {
                CreateTelemetryNotification(notificationsList[i], i);
            }

            //Wait 10 seconds before pulling data again
            yield return new WaitForSecondsRealtime((float)REFRESH_RATE);
        }
    }

    //Converts json data into TelemetryData objects, sorts by severity and name, and updates notification array
    void UpdateNotificationsArray(JSONData jsonData)
    {
        //Numerical data
        NumericalData P_sub = new NumericalData("External pressure", jsonData.p_sub, "psia", 2, 4);
        NumericalData T_sub = new NumericalData("External temperature", jsonData.t_sub, "F", 0, 100);
        NumericalData V_fan = new NumericalData("Fan speed", jsonData.v_fan, "RPM", 10000, 40000);
        NumericalData P_o2 = new NumericalData("O2 pressure", jsonData.p_o2, "psia", 750, 950);
        NumericalData Rate_o2 = new NumericalData("O2 flow rate", jsonData.rate_o2, "psi/min", 0.5, 1);
        NumericalData Cap_battery = new NumericalData("Battery capacity", jsonData.cap_battery, "amp-hr", 0, 30);
        NumericalData P_h2o_g = new NumericalData("H2O gas pressure", jsonData.p_h2o_g, "psia", 14, 16);
        NumericalData P_h2o_l = new NumericalData("H2O liquid pressure", jsonData.p_h2o_l, "psia", 14, 16);
        NumericalData P_sop = new NumericalData("SOP pressure", jsonData.p_sop, "psia", 750, 950);
        NumericalData Rate_sop = new NumericalData("SOP flow rate", jsonData.rate_sop, "psia/min", 0.5, 1);

        //Switch data
        SwitchData Sop_on = new SwitchData("SOP active", jsonData.sop_on, false);
        SwitchData Sspe = new SwitchData("Spacesuit pressure emergency", jsonData.sspe, true);
        SwitchData Fan_error = new SwitchData("Fan error", jsonData.fan_error, true);
        SwitchData Vent_error = new SwitchData("Vent error", jsonData.vent_error, true);
        SwitchData Vehicle_power = new SwitchData("Receiving power through craft", jsonData.vehicle_power, false);
        SwitchData H2o_off = new SwitchData("H2O offline", jsonData.h2o_off, true);
        SwitchData O2_off = new SwitchData("O2 offline", jsonData.o2_off, true);

        notificationsList = new List<TelemetryData>
        {
            P_sub,
            T_sub,
            V_fan,
            P_o2,
            Rate_o2,
            Cap_battery,
            P_h2o_g,
            P_h2o_l,
            P_sop,
            Rate_sop,
            Sop_on,
            Sspe,
            Fan_error,
            Vent_error,
            Vehicle_power,
            H2o_off,
            O2_off
        };

        //Sort by severity and then alphabetically
        notificationsList.Sort((x, y) =>
        {
            int retval = x.severity.CompareTo(y.severity);
            if (retval == 0) retval = x.name.CompareTo(y.name);
            return retval;
        });
    }
}
