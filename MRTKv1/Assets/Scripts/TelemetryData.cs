public enum Severity { UNKNOWN = 0, CRITICAL = 1, WARNING = 2, NOMINAL = 3 }; //should be ordered most to least severe

public abstract class TelemetryData
{
    public string name;
    public Severity severity;

    public abstract Severity GetSeverity();
    public abstract string GetDescription();
    public abstract string GetNameText();
    public abstract string GetValueText();
}
