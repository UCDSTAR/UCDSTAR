using System;

public class SwitchData : TelemetryData
{
    private string switchCondition;
    private bool trueMeansError;

    public SwitchData(string n, string c, bool t)
    {
        name = n;
        switchCondition = c;
        trueMeansError = t;
        severity = GetSeverity();
    }

    public override Severity GetSeverity()
    {
        if (trueMeansError)
        {
            if (switchCondition.Equals("false")) return Severity.NOMINAL;
            else return Severity.CRITICAL; //if condition not recognized, err on the side of caution
        }
        else
        {
            if (switchCondition.Equals("true")) return Severity.NOMINAL;
            else return Severity.CRITICAL;
        }
    }

    public override string GetDescription()
    {
        return name + ": " + switchCondition;
    }

    public override string GetNameText()
    {
        return name;
    }

    public override string GetValueText()
    {
        return switchCondition;
    }
}