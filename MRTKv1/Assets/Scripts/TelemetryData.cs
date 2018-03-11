using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public enum Severity { NOMINAL, WARNING, CRITICAL};

public class TelemetryData
{
    private Severity severity;
    private String dataName;
    private float dataValue;
    private String dataUnits;

    public TelemetryData(Severity s, String name, float val, String units)
    {
        severity = s;
        dataName = name;
        dataValue = val;
        dataUnits = units;
    }

    public Severity GetSeverity()
    {
        return severity;
    }

    public String GetDataText()
    {
        return dataName + ":\n" + dataValue + " " + dataUnits;
    }
}

