using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

    //Steps
    public Button fwdButton;
    public Button backButton;
    public Image stepImage;
    private int stepNum;
    private int maxStepNum;

    //Called on startup
    void Start()
    {
        //Bind buttons to functions
        fwdButton.onClick.AddListener(moveToNextStep);
        backButton.onClick.AddListener(moveToPrevStep);

        //Let's start at the very beginning, a very good place to start
        stepNum = 1;
        maxStepNum = 6;
        setStepSprite(stepNum);
    }

    //Called every frame
    void Update()
    {
        //nothing here yet
    }

    void moveToNextStep()
    {
        ++stepNum;
        if(stepNum > maxStepNum)
        {
            stepNum = 1; //wrap around
        }
        setStepSprite(stepNum);
    }

    void moveToPrevStep()
    {
        --stepNum;
        if (stepNum < 1)
        {
            stepNum = maxStepNum; //wrap around
        }
        setStepSprite(stepNum);
    }

    void setStepSprite(int stepNum)
    {
        String path = String.Format("ProcedureSteps/Steps-0{0}-static", stepNum);
        Sprite newSprite = Resources.Load<Sprite>(path);
        if(newSprite)
        {
            stepImage.sprite = newSprite;
        }
        else
        {
            Debug.LogError(path, this);
        }
    }
}
