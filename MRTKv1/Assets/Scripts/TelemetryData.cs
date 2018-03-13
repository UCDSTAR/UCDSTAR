using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public enum Severity { CRITICAL=0, WARNING=1, NOMINAL=2}; //should be ordered most to least severe

public abstract class TelemetryData
{
    public string name;
    public Severity severity;

    public abstract Severity GetSeverity();
    public abstract string GetDataText();
    public abstract string GetNameText();
    public abstract string GetValueText();
}

