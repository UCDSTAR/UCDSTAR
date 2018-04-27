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
    public GameObject notificationsPanel;
    public GameObject textPanel;
    public GameObject evaTimeText;
    public GameObject telemetryNotification; //this will be copied into the scene
    public GameObject telemetryTextName; //this will be copied into the scene
    public GameObject telemetryTextNumber; //this will be copied into the scene
    public GameObject pressureArrow;
    public GameObject oxygenArrow;
    public GameObject pressureImage;
    public GameObject oxygenImage;
    public GameObject temperatureImage;
    public GameObject batteryImage;
    public GameObject notifyImage;
    public GameObject timerPanel;

    //Telemetry data
    private const int MAX_NOTIFICATIONS = 4;
    private const double REFRESH_RATE = 2; //in seconds
    private const int TEMPERATURE_INDEX = 2;
    private const int PRESSURE_INDEX = 15;
    private const int OXYGEN_INDEX = 10;
    private const int BATTERY_INDEX = 0;
    private List<TelemetryData> notificationsList;
    private List<TelemetryData> textList;
    private string numericalDataURL = "https://hrvip.ucdavis.edu/share/UCDSUITS/api/telemetry/recent.json";
    private string switchDataURL = "https://hrvip.ucdavis.edu/share/UCDSUITS/api/switch/recent.json";
    private Boolean numericalServerConnErr;
    private Boolean switchServerConnErr;

    // Use this for initialization
    void Start()
    {
        //Hide notifications panel on startup
        notificationsPanel.SetActive(true);

        //Start polling server for data
        StartCoroutine(GetTelemetryData());
    }

    // Update is called once per frame
    void Update()
    {
        String timeStr = DateTime.UtcNow.ToString("t") + " UTC";
        currentTimeText.GetComponentInChildren<Text>().text = timeStr;
    }

    //Called when voice command is triggered
    void ShowNotifications_s()
    {
        //Show panel
        notificationsPanel.SetActive(true);
    }

    //Called when voice command is triggered
    void HideNotifications_s()
    {
        notificationsPanel.SetActive(false);
    }

    //Repeatedly read telemetry data from server using an HTTP GET request and update notification data
    IEnumerator GetTelemetryData()
    {
        while (true)
        {
            string numericalStr = "blank:blank", switchStr = "blank:blank", jsonStr = "blank:blank";

            //Get numerical data
            numericalServerConnErr = false;
            switchServerConnErr = false;

            using (UnityWebRequest www1 = UnityWebRequest.Get(numericalDataURL))
            {
                yield return www1.Send();

                if (www1.isError)
                {
                    numericalServerConnErr = true;
                    Debug.Log(www1.error);
                }
                else
                {
                    numericalStr = www1.downloadHandler.text;
                    numericalStr = numericalStr.Trim();
                }
            }

            //Get switch data
            using (UnityWebRequest www2 = UnityWebRequest.Get(switchDataURL))
            {
                yield return www2.Send();

                if (www2.isError)
                {
                    switchServerConnErr = true;
                    Debug.Log(www2.error);
                }
                else
                {
                    switchStr = www2.downloadHandler.text;
                    switchStr = switchStr.Trim();
                }
            }

            //Parse and update notifications if valid connections to servers exist
            //Note that BOTH connections must be working for any updates to occur
            //TODO: if one server goes down, figure out how to only update notifications from the other server
            if (!switchServerConnErr && !numericalServerConnErr)
            {
                jsonStr = numericalStr.Substring(0, numericalStr.Length - 1) + "," + switchStr.Substring(1);
                JSONData jsonData = JSONData.CreateFromJSON(jsonStr);
                if (jsonData.t_eva != null)
                    evaTimeText.GetComponentInChildren<Text>().text = jsonData.t_eva; //update eva time separately
                UpdateNotificationsArray(jsonData);
            }

            //Clear notifications
            foreach (Transform child in notificationsPanel.transform)
            {
                Destroy(child.gameObject);
            }

            //Clear right panel text
            foreach (Transform child in textPanel.transform)
            {
                Destroy(child.gameObject);
            }

            //Create new notifications
            //Special notifications for server communication failure appear first (if any)
            SwitchData switchConnNotification, numConnNotification;
            int index = 0;
            if (switchServerConnErr)
            {
                switchConnNotification = new SwitchData("Switch server connection", "false", false);
                CreateTelemetryNotification(switchConnNotification, index);
                ++index;
            }
            if (numericalServerConnErr)
            {
                numConnNotification = new SwitchData("Telemetry server connection", "false", false);
                CreateTelemetryNotification(numConnNotification, index);
                ++index;
            }
            for (; index < MAX_NOTIFICATIONS; ++index)
            {
                if (notificationsList[index].severity == Severity.NOMINAL) break; //only show errors and warnings
                CreateTelemetryNotification(notificationsList[index], index);
            }

            //Update notification icon
            Severity notifySeverity = notificationsList[0].severity;
            if (switchServerConnErr || numericalServerConnErr) notifySeverity = Severity.CRITICAL;
            String notifyIconPath = String.Format("Icons/notify-{0}", notifySeverity.ToString());
            Sprite notifyIcon = Resources.Load<Sprite>(notifyIconPath);
            notifyImage.GetComponent<Image>().sprite = notifyIcon;

            //Create telemetry text for right panel
            for (int j = 0; j < textList.Count; ++j)
            {
                CreateTelemetryText(textList[j], j);
            }

            //Get pressure, oxygen, temperature, and battery data
            NumericalData sop_pressure = (NumericalData)textList[PRESSURE_INDEX];
            NumericalData oxygen_pressure = (NumericalData)textList[OXYGEN_INDEX];
            NumericalData temperature = (NumericalData)textList[TEMPERATURE_INDEX];
            NumericalData battery = (NumericalData)textList[BATTERY_INDEX];

            //Update the pressure, oxygen, temperature, and battery icons
            String pressureIconPath = String.Format("Icons/dial-{0}", sop_pressure.severity.ToString());
            String oxygenIconPath = String.Format("Icons/dial-{0}", oxygen_pressure.severity.ToString());
            String temperatureIconPath = String.Format("Icons/temperature-{0}", temperature.severity.ToString());
            String batteryIconPath = String.Format("Icons/battery-{0}", battery.severity.ToString());
            Sprite pressureIcon = Resources.Load<Sprite>(pressureIconPath);
            Sprite oxygenIcon = Resources.Load<Sprite>(oxygenIconPath);
            Sprite temperatureIcon = Resources.Load<Sprite>(temperatureIconPath);
            Sprite batteryIcon = Resources.Load<Sprite>(batteryIconPath);
            pressureImage.GetComponent<Image>().sprite = pressureIcon;
            oxygenImage.GetComponent<Image>().sprite = oxygenIcon;
            temperatureImage.GetComponent<Image>().sprite = temperatureIcon;
            batteryImage.GetComponent<Image>().sprite = batteryIcon;


            //Update pressure and oxygen arrows if they have values
            //If null, the arrows won't change their rotation
            if (sop_pressure != null)
            {
                double pressureAngle = ValueToDegrees(sop_pressure);
                pressureArrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (float)pressureAngle);
            }
            if (oxygen_pressure != null)
            {
                double oxygenAngle = ValueToDegrees(oxygen_pressure);
                oxygenArrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (float)oxygenAngle);
            }

            //Wait before pulling data again
            yield return new WaitForSecondsRealtime((float)REFRESH_RATE);
        }
    }

    //Converts a data value into a degree value between 225 deg (-10%) and -45 deg (110%)
    double ValueToDegrees(NumericalData data)
    {
        double degrees = (100 * (data.value - data.minValue) / (data.maxValue - data.minValue)) * -2.25 + 202.5;
        if (degrees > 225) return 225;
        else if (degrees < -45) return -45;
        else return degrees;
    }

    //Create a telemetry notification object and add it to the details panel at the given index
    //Index 0 is at the top of the panel
    void CreateTelemetryNotification(TelemetryData t, int index)
    {
        GameObject panelClone = Instantiate(telemetryNotification, notificationsPanel.GetComponent<Transform>(), false);
        panelClone.GetComponent<RectTransform>().localPosition = new Vector3(0, (float)(1.425 - 0.95 * index), 0);

        //Set color based on severity
        switch (t.severity)
        {
            case Severity.NOMINAL:
                panelClone.GetComponent<Image>().color = Constants.GREEN;
                break;
            case Severity.WARNING:
                panelClone.GetComponent<Image>().color = Constants.YELLOW;
                break;
            case Severity.CRITICAL:
                panelClone.GetComponent<Image>().color = Constants.RED;
                break;
            case Severity.UNKNOWN:
                panelClone.GetComponent<Image>().color = Constants.RED;
                break;
        }

        //Set text
        panelClone.GetComponentInChildren<Text>().text = t.GetDescription();
    }

    //Create telemetry data text for the right panel
    //Index 0 is at the top of the panel
    void CreateTelemetryText(TelemetryData t, int index)
    {
        GameObject textNameClone = Instantiate(telemetryTextName, textPanel.GetComponent<Transform>(), false);
        GameObject textNumberClone = Instantiate(telemetryTextNumber, textPanel.GetComponent<Transform>(), false);

        textNameClone.GetComponent<RectTransform>().localPosition = new Vector3((float)0.75, (float)(3.5 - 0.4 * index), 0);
        textNumberClone.GetComponent<RectTransform>().localPosition = new Vector3((float)-0.75, (float)(3.5 - 0.4 * index), 0);

        //Set color based on severity
        switch (t.severity)
        {
            case Severity.NOMINAL:
                textNameClone.GetComponentInChildren<Text>().color = Color.white;
                textNumberClone.GetComponentInChildren<Text>().color = Color.white;
                break;
            case Severity.WARNING:
                textNameClone.GetComponentInChildren<Text>().color = Constants.YELLOW;
                textNumberClone.GetComponentInChildren<Text>().color = Constants.YELLOW;
                break;
            case Severity.CRITICAL:
                textNameClone.GetComponentInChildren<Text>().color = Constants.RED;
                textNumberClone.GetComponentInChildren<Text>().color = Constants.RED;
                break;
            case Severity.UNKNOWN:
                textNameClone.GetComponentInChildren<Text>().color = Constants.RED;
                textNumberClone.GetComponentInChildren<Text>().color = Constants.RED;
                break;
        }

        //Set text
        textNameClone.GetComponentInChildren<Text>().text = t.GetNameText();
        textNumberClone.GetComponentInChildren<Text>().text = t.GetValueText();
    }

    //Converts json data into TelemetryData objects, sorts by severity and name, and updates notification array
    void UpdateNotificationsArray(JSONData jsonData)
    {
        //Numerical data
        NumericalData P_sub = new NumericalData("External pressure", jsonData.p_sub, "psia", 2, 4);
        NumericalData T_sub = new NumericalData("External temperature", jsonData.t_sub, "F", -150, 250);
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
        SwitchData Sspe = new SwitchData("Pressure emergency", jsonData.sspe, true);
        SwitchData Fan_error = new SwitchData("Fan error", jsonData.fan_error, true);
        SwitchData Vent_error = new SwitchData("Vent error", jsonData.vent_error, true);
        SwitchData Vehicle_power = new SwitchData("Receiving power", jsonData.vehicle_power, false);
        SwitchData H2o_off = new SwitchData("H2O offline", jsonData.h2o_off, true);
        SwitchData O2_off = new SwitchData("O2 offline", jsonData.o2_off, true);

        //This is already in alphabetical order to save us a sort
        //*****Do NOT edit this list*****
        //There are constants at the top of the script which depend on the objects' positions
        textList = new List<TelemetryData>
        {
            Cap_battery,
            P_sub,
            T_sub,
            Fan_error,
            V_fan,
            P_h2o_g,
            P_h2o_l,
            H2o_off,
            Rate_o2,
            O2_off,
            P_o2,
            Sspe,
            Vehicle_power,
            Sop_on,
            Rate_sop,
            P_sop,
            Vent_error
        };

        //We'll need to sort this list
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

    //Show/hide the EVA timer
    void ToggleTimer_s()
    {
        timerPanel.SetActive(!timerPanel.activeInHierarchy);
    }
}
