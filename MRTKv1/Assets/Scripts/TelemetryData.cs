using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public enum Severity { NOMINAL, WARNING, CRITICAL};

public class TelemetryData
{
    private string name;
    private float value;
    private string units;

    private float minValue;
    private float maxValue;
    private bool isSwitch;

    private const float WARNING_PERCENTAGE = 0.1f;
    private float warningAmt;

    //Constructor for numerical data
    public TelemetryData(string n, string v, string u, float min, float max)
    {
        name = n;
        value = Convert.ToSingle(v);
        units = u;
        minValue = min;
        maxValue = max;
        warningAmt = (maxValue - minValue) * WARNING_PERCENTAGE;
        isSwitch = false;
    }

    //Constructor for switch data
    public TelemetryData(string n, string v)
    {
        name = n;
        value = Convert.ToSingle(v);
        isSwitch = true;
    }

    public Severity GetSeverity()
    {
        if (isSwitch)
        {
            if (value == 1) return Severity.CRITICAL;
            else return Severity.NOMINAL;
        }
        else
        {
            if(value < minValue || value > maxValue)
            {
                return Severity.CRITICAL;
            }
            else if(value < (minValue + warningAmt) || value > (maxValue - warningAmt))
            {
                return Severity.WARNING;
            }
            else
            {
                return Severity.NOMINAL;
            }
        }
    }
}

