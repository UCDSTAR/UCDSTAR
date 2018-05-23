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
    public GameObject telemetryNotification; //this will be copied into the scene
    public GameObject telemetryTextName; //this will be copied into the scene
    public GameObject telemetryTextNumber; //this will be copied into the scene
    public Image oxygenImage;    
    public Image oxygenArrow;
    public GameObject oxygenText;
    public Image pressureImage;
    public Image pressureArrow;
    public GameObject pressureText;
    public Image temperatureImage;
    public GameObject temperatureText;
    public Image batteryImage;
    public GameObject notifyImage;
    public GameObject timerPanel;
    public GameObject notificationCountText;

    //Telemetry data
    private const int MAX_NOTIFICATIONS = 5;
    private const double REFRESH_RATE = 10; //in seconds
    private const int TEMPERATURE_INDEX = 2;
    private const int PRESSURE_INDEX = 9;
    private const int OXYGEN_INDEX = 7;
    private const int BATTERY_INDEX = 0;
    private const int NUM_NUMERICAL = 12;
    private const int NUM_SWITCH = 7;
    private const int NUM_TIMER = 3;
    private List<TelemetryData> notificationsList = new List<TelemetryData>(NUM_NUMERICAL + NUM_SWITCH +NUM_TIMER);
    private List<NumericalData> numericalTextList = new List<NumericalData>(NUM_NUMERICAL);
    private List<SwitchData> switchTextList = new List<SwitchData>(NUM_SWITCH);
    private List<TimerData> timerTextList = new List<TimerData>(NUM_TIMER);
    private string numericalDataURL = "https://apollo-program.herokuapp.com/api/suit/recent";
    private string switchDataURL = "https://apollo-program.herokuapp.com/api/suitswitch/recent";
    private Boolean numericalServerConnErr;
    private Boolean switchServerConnErr;

    // Use this for initialization
    void Start()
    {
        InitializeLists();

        //Hide notifications panel on startup
        notificationsPanel.SetActive(true);

        //Start polling server for data
        StartCoroutine(GetTelemetryData());
    }

    void InitializeLists()
    {
        for (int i = 0; i < notificationsList.Capacity; ++i)
        {
            notificationsList.Add(null);
        }
        for (int i = 0; i < numericalTextList.Capacity; ++i)
        {
            numericalTextList.Add(null);
        }
        for (int i = 0; i < switchTextList.Capacity; ++i)
        {
            switchTextList.Add(null);
        }
        for (int i = 0; i < timerTextList.Capacity; ++i)
        {
            timerTextList.Add(null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        String timeStr = DateTime.UtcNow.ToString("t") + " UTC";
        currentTimeText.GetComponentInChildren<Text>().text = timeStr;
    }

    //Called when voice command is triggered
    void ShowAlerts_s()
    {
        //Show panel
        notificationsPanel.SetActive(true);
    }

    //Called when voice command is triggered
    void HideAlerts_s()
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
                    numericalServerConnErr = false;
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
                    switchServerConnErr = false;
                    switchStr = www2.downloadHandler.text;
                    switchStr = switchStr.Trim();
                }
            }

            //Parse and update notifications if valid connections to servers exist
            //Note that BOTH connections must be working for any updates to occur
            //TODO: if one server goes down, figure out how to only update notifications from the other server
            if (!switchServerConnErr && !numericalServerConnErr)
            {
                jsonStr = numericalStr.Substring(1, numericalStr.Length - 3) + "," + switchStr.Substring(2, switchStr.Length - 3);
                JSONData jsonData = JSONData.CreateFromJSON(jsonStr);
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
                if (notificationsList[index] == null) break; //hit end of list early
                if (notificationsList[index].severity == Severity.NOMINAL) break; //only show errors and warnings
                CreateTelemetryNotification(notificationsList[index], index);
            }

            //Update notification icon
            Severity notifySeverity = notificationsList[0].severity; //there should always be something in index 0, even if server is down
            if (switchServerConnErr || numericalServerConnErr)
                notifySeverity = Severity.CRITICAL;
            String notifyIconPath = String.Format("Icons/notify-{0}", notifySeverity.ToString());
            Sprite notifyIcon = Resources.Load<Sprite>(notifyIconPath);
            notifyImage.GetComponent<Image>().sprite = notifyIcon;

            //Update notification count
            int numNotifications = (NUM_NUMERICAL + NUM_SWITCH + NUM_TIMER) - notificationsList.FindAll(x => x.severity == Severity.NOMINAL).Count;
            if (numNotifications == 0)
                notificationCountText.SetActive(false);
            else
            {
                notificationCountText.SetActive(true);
                notificationCountText.GetComponentInChildren<Text>().text = "" + numNotifications;
            }
            /*
            if (notifySeverity == Severity.CRITICAL)
            {
                notifyImage.GetComponent<AudioSource>().Play();
            }
            else
            {
                notifyImage.GetComponent<AudioSource>().Stop();
            }
            */
            //Create telemetry text for right panel
            CreateTelemetryTextHeaders();
            for (int j = 0; j < numericalTextList.Count; ++j)
            {
                CreateTelemetryText(numericalTextList[j], j, TelemetryType.NUMERICAL);
            }
            for(int k = 0; k < switchTextList.Count; ++k)
            {
                CreateTelemetryText(switchTextList[k], k, TelemetryType.SWITCH);
            }
            for (int m = 0; m < timerTextList.Count; ++m)
            {
                CreateTelemetryText(timerTextList[m], m, TelemetryType.TIMER);
            }

            //Get pressure, oxygen, temperature, and battery data
            NumericalData sop_pressure = numericalTextList[PRESSURE_INDEX];
            NumericalData oxygen_pressure = numericalTextList[OXYGEN_INDEX];
            NumericalData temperature = numericalTextList[TEMPERATURE_INDEX];
            TimerData battery = timerTextList[BATTERY_INDEX];

            //Update the pressure, oxygen, temperature, and battery icons and text
            if (sop_pressure != null)
            {
                String pressureIconPath = String.Format("Icons/suit-{0}", sop_pressure.severity.ToString());
                Sprite pressureIcon = Resources.Load<Sprite>(pressureIconPath);
                pressureImage.sprite = pressureIcon;
                pressureText.GetComponentInChildren<Text>().text = sop_pressure.value + " " + sop_pressure.units;
            }
            if (oxygen_pressure != null)
            {
                String oxygenIconPath = String.Format("Icons/oxygen-{0}", oxygen_pressure.severity.ToString());
                Sprite oxygenIcon = Resources.Load<Sprite>(oxygenIconPath);
                oxygenImage.sprite = oxygenIcon;
                oxygenText.GetComponentInChildren<Text>().text = oxygen_pressure.value + " " + oxygen_pressure.units;
            }
            if (temperature != null)
            {
                String temperatureIconPath = String.Format("Icons/temperature-{0}", temperature.severity.ToString());
                Sprite temperatureIcon = Resources.Load<Sprite>(temperatureIconPath);
                temperatureImage.sprite = temperatureIcon;
                temperatureText.GetComponentInChildren<Text>().text = temperature.value + " " + temperature.units;
            }
            if (battery != null)
            {
                String batteryIconPath = String.Format("Icons/battery-{0}", battery.severity.ToString());
                Sprite batteryIcon = Resources.Load<Sprite>(batteryIconPath);
                batteryImage.sprite = batteryIcon;
            }

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
        panelClone.GetComponent<RectTransform>().localPosition = new Vector3(0, (float)(2.4 - 0.95 * index), 0);

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

    //Create the "Numerical Data" and "Switch Data" headers on the right-side panel
    void CreateTelemetryTextHeaders()
    {
        //Numerical data
        GameObject numericalTextNameClone = Instantiate(telemetryTextName, textPanel.GetComponent<Transform>(), false);
        numericalTextNameClone.GetComponent<RectTransform>().localPosition = new Vector3((float)(0.75 - 0.25), (float)(3.5 - 0.4 * -1) + 1.5f, 0);
        numericalTextNameClone.GetComponentInChildren<Text>().text = "<b>Numerical data</b>";

        //Switch data
        GameObject switchTextNameClone = Instantiate(telemetryTextName, textPanel.GetComponent<Transform>(), false);
        switchTextNameClone.GetComponent<RectTransform>().localPosition = new Vector3((float)(0.75 - 0.25), (float)(3.5 - 0.4 * -1) - 4.0f, 0);
        switchTextNameClone.GetComponentInChildren<Text>().text = "<b>Switch data</b>";

        //Timer data
        GameObject timerTextNameClone = Instantiate(telemetryTextName, textPanel.GetComponent<Transform>(), false);
        timerTextNameClone.GetComponent<RectTransform>().localPosition = new Vector3((float)(0.75 - 0.25), (float)(3.5 - 0.4 * -1) - 7.5f, 0);
        timerTextNameClone.GetComponentInChildren<Text>().text = "<b>Timers</b>";
    }

    //Create telemetry data text for the right panel
    //Index 0 is at the top of the panel
    void CreateTelemetryText(TelemetryData t, int index, TelemetryType ttype)
    {
        if (t == null)
            return;

        float offset;
        if (ttype == TelemetryType.NUMERICAL)
            offset = -1.5f;
        else if (ttype == TelemetryType.SWITCH)
            offset = 4.0f;
        else
            offset = 7.5f;

        GameObject textNameClone = Instantiate(telemetryTextName, textPanel.GetComponent<Transform>(), false);
        GameObject textNumberClone = Instantiate(telemetryTextNumber, textPanel.GetComponent<Transform>(), false);

        textNameClone.GetComponent<RectTransform>().localPosition = new Vector3((float)0.75, (float)(3.5 - 0.4 * index) - offset, 0);
        textNumberClone.GetComponent<RectTransform>().localPosition = new Vector3((float)-0.75, (float)(3.5 - 0.4 * index) - offset, 0);

        //Set color based on severity
        switch (t.severity)
        {
            case Severity.NOMINAL:
                textNameClone.GetComponentInChildren<Text>().color = Constants.WHITE;
                textNumberClone.GetComponentInChildren<Text>().color = Constants.WHITE;
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
        NumericalData Heart_bpm = new NumericalData("Heart rate", jsonData.heart_bpm, "bpm", 0, 300);
        NumericalData P_suit = new NumericalData("Suit pressure", jsonData.p_suit, "psia", 2, 4);
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

        //Timer data
        TimerData T_battery = new TimerData("Battery life", jsonData.t_battery, "01:00:00", "00:30:00");
        TimerData T_oxygen = new TimerData("Oxygen remaining", jsonData.t_oxygen, "01:00:00", "00:30:00");
        TimerData T_water = new TimerData("Water remaining", jsonData.t_water, "01:00:00", "00:30:00");

        //These next two lists are already in alphabetical order to save us a sort
        //*****BE VERY CAREFUL WHEN EDITING THESE NEXT TWO LISTS*****
        //*****There are constants at the top of the script which depend on the objects' positions*****
        numericalTextList[0] = Cap_battery;
        numericalTextList[1] = P_sub;
        numericalTextList[2] = T_sub;
        numericalTextList[3] = V_fan;
        numericalTextList[4] = P_h2o_g;
        numericalTextList[5] = P_h2o_l;
        numericalTextList[6] = Heart_bpm;
        numericalTextList[7] = Rate_o2;
        numericalTextList[8] = P_o2;
        numericalTextList[9] = Rate_sop;
        numericalTextList[10] = P_sop;
        numericalTextList[11] = P_suit;

        switchTextList[0] = Fan_error;
        switchTextList[1] = H2o_off;
        switchTextList[2] = O2_off;
        switchTextList[3] = Sspe;
        switchTextList[4] = Vehicle_power;
        switchTextList[5] = Sop_on;
        switchTextList[6] = Vent_error;

        timerTextList[0] = T_battery;
        timerTextList[1] = T_oxygen;
        timerTextList[2] = T_water;

        //We'll need to sort this list
        //Feel free to edit this list
        notificationsList[0] = P_sub;
        notificationsList[1] = T_sub;
        notificationsList[2] = V_fan;
        notificationsList[3] = P_o2;
        notificationsList[4] = Rate_o2;
        notificationsList[5] = Cap_battery;
        notificationsList[6] = P_h2o_g;
        notificationsList[7] = P_h2o_l;
        notificationsList[8] = P_sop;
        notificationsList[9] = Rate_sop;
        notificationsList[10] = Sop_on;
        notificationsList[11] = Sspe;
        notificationsList[12] = Fan_error;
        notificationsList[13] = Vent_error;
        notificationsList[14] = Vehicle_power;
        notificationsList[15] = H2o_off;
        notificationsList[16] = O2_off;
        notificationsList[17] = P_suit;
        notificationsList[18] = Heart_bpm;
        notificationsList[19] = T_battery;
        notificationsList[20] = T_oxygen;
        notificationsList[21] = T_water;

        //Sort by severity and then alphabetically
        notificationsList.Sort((x, y) =>
        {
            int retval = x.severity.CompareTo(y.severity);
            if (retval == 0) retval = x.name.CompareTo(y.name);
            return retval;
        });
    }

    //Show/hide the EVA timer
    void ShowTime_s()
    {
        timerPanel.SetActive(true);
    }

    void HideTime_s()
    {
        timerPanel.SetActive(false);
    }
}
