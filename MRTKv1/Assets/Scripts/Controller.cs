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
    public int numberOfSteps;
    private int stepNum; //starts counting from 1

    //Called on startup
    void Start()
    {
        //Bind buttons to functions
        fwdButton.onClick.AddListener(moveToNextStep);
        backButton.onClick.AddListener(moveToPrevStep);

        //Let's start at the very beginning, a very good place to start
        stepNum = 1;
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
        if(stepNum > numberOfSteps)
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
            stepNum = numberOfSteps; //wrap around
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
