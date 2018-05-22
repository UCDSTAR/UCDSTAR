using System;

public class TimerData : TelemetryData
{
    public TimeSpan timeVal;
    public TimeSpan warningTime;
    public TimeSpan criticalTime;

    public TimerData(string n, string t, string w, string c)
    {
        ttype = TelemetryType.TIMER;

        if(t == null)
        {
            name = n + " UNKNOWN";
            timeVal = new TimeSpan(0, 0, 0);
            severity = Severity.UNKNOWN;
        }
        else
        {
            name = n;
            timeVal = TimeSpan.Parse(t.Replace("-", ""));
            warningTime = TimeSpan.Parse(w);
            criticalTime = TimeSpan.Parse(c);
            severity = GetSeverity();
        }
    }

    public override Severity GetSeverity()
    {
        if (timeVal.CompareTo(criticalTime) < 0)
            return Severity.CRITICAL;
        else if (timeVal.CompareTo(warningTime) < 0)
            return Severity.WARNING;
        else
            return Severity.NOMINAL;
    }

    public override string GetDescription()
    {
        return name + ": " + timeVal;
    }

    public override string GetNameText()
    {
        return name + ": ";
    }

    public override string GetValueText()
    {
        return timeVal.ToString();
    }
}