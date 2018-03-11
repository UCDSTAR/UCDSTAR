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
        fwdButton.onClick.AddListener(MoveToNextStep);
        backButton.onClick.AddListener(MoveToPrevStep);

        //Let's start at the very beginning, a very good place to start
        stepNum = 1;
        SetStepSprite(stepNum);
    }

    //Called every frame
    void Update()
    {
        //nothing here yet
    }

    void MoveToNextStep()
    {
        ++stepNum;
        if(stepNum > numberOfSteps)
        {
            stepNum = 1; //wrap around
        }
        SetStepSprite(stepNum);
    }

    void MoveToPrevStep()
    {
        --stepNum;
        if (stepNum < 1)
        {
            stepNum = numberOfSteps; //wrap around
        }
        SetStepSprite(stepNum);
    }

    //These next two are triggered by voice commands
    void NextInstruction_s()
    {
        MoveToNextStep();
    }

    void PreviousInstruction_s()
    {
        MoveToPrevStep();
    }

    void SetStepSprite(int stepNum)
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
