using System.Collections;
using System.Collections.Generic;

public class ProcedureContainer
{
    private object[] steps;

    public ProcedureContainer(int numSteps)
    {
        steps = new object[numSteps];
    }
}