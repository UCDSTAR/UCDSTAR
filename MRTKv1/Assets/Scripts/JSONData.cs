﻿using UnityEngine;

[System.Serializable]
public class JSONData
{
    //Numerical data
    public string heart_bpm;
    public string p_suit;
    public string p_sub;
    public string t_sub;
    public string v_fan;
    public string p_o2;
    public string rate_o2;
    public string cap_battery;
    public string p_h2o_g;
    public string p_h2o_l;
    public string p_sop;
    public string rate_sop;

    //Switch data
    public string sop_on;
    public string sspe;
    public string fan_error;
    public string vent_error;
    public string vehicle_power;
    public string h2o_off;
    public string o2_off;

    //Timer data
    public string t_battery;
    public string t_oxygen;
    public string t_water;

    public static JSONData CreateFromJSON(string jsonDataString)
    {
        return JsonUtility.FromJson<JSONData>(jsonDataString);
    }
}
