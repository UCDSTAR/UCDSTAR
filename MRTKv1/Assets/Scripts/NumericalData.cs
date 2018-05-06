using System;

public class NumericalData : TelemetryData
{
    public double value;
    public string units;
    public double minValue;
    public double maxValue;

    private const double WARNING_PERCENTAGE = 0.1;
    private double warningAmt;

    public NumericalData(string n, string v, string u, double min, double max)
    {
        ttype = TelemetryType.NUMERICAL;

        if (v == null) //value missing from JSON
        {
            name = n + " UNKNOWN";
            value = -1;
            severity = Severity.UNKNOWN;
        }
        else
        {
            name = n;
            value = Convert.ToDouble(v);
            units = u;
            minValue = min;
            maxValue = max;
            warningAmt = (maxValue - minValue) * WARNING_PERCENTAGE;
            severity = GetSeverity();
        }
    }

    public override Severity GetSeverity()
    {
        if (value < minValue || value > maxValue)
        {
            return Severity.CRITICAL;
        }
        else if (value < (minValue + warningAmt) || value > (maxValue - warningAmt))
        {
            return Severity.WARNING;
        }
        else
        {
            return Severity.NOMINAL;
        }
    }

    public override string GetDescription()
    {
        return name + ": " + value + " " + units;
    }

    public override string GetNameText()
    {
        return name + ": ";
    }

    public override string GetValueText()
    {
        return value + " " + units;
    }
}
