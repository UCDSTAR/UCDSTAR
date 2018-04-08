using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class Main : MonoBehaviour
{

    void Awake()
    {

        List<Dictionary<string, object>> data = CSVReader.Read("Disabling Alarm Procedure");

        for (var i = 0; i < data.Count; i++)
        {
            print("Step " + data[i]["Step"] + " " +
                   "Text " + data[i]["Text"] + " " +
                   "Caution " + data[i]["Caution"] + " " +
                   "Warning " + data[i]["Warning"] + " " +
                   "Figure " + data[i]["Figure"]);
        }

    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}