using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
 
//NOTE: some parts of this program assume there are at least 4 steps!!!
//Steps are 0-indexed (i.e. step #1 is considered to be step 0)

public class ProcedureController : MonoBehaviour
{
    public string csvName;
    public GameObject stepAsset; //this is cloned
    public GameObject currentStepPanel; //this is not cloned, it's updated as needed
    public Canvas stepCanvas;
    public GameObject zoomImage;

    private List<Dictionary<string, string>> data;
    private int numSteps;
    private int currentStep;
    private GameObject[] stepContainer;
    private const int SHOW_NUM_STEPS = 4;

    //Acts as constructor for object
    void Awake()
    {
        //Parse csv file
        data = CSVReader.Read(csvName);
        numSteps = data.Count;        
    }

    // Use this for initialization
    void Start()
    {
        ProcedureInit();
    }

    //Initializes the procedure display with the first few steps
    void ProcedureInit()
    {
        stepContainer = new GameObject[SHOW_NUM_STEPS];
        currentStep = 0; //first step

        //Init container with first few steps
        for(int i = 0; i < SHOW_NUM_STEPS; ++i)
        {
            stepContainer[i] = GenerateStep(data[i]["Step"], data[i]["Text"], data[i]["Caution"], data[i]["Warning"], data[i]["Figure"]);
            SetStepPos(stepContainer[i], i);
        }

        //Initialize currentStepPanel
        GameObject progressBar = currentStepPanel.transform.Find("ProgressBar").gameObject;
        progressBar.GetComponent<Slider>().minValue = 1;
        progressBar.GetComponent<Slider>().maxValue = numSteps;

        //Set first instruction as active
        SetStepActive(stepContainer[0], true);
    }

    //Triggered by voice command
    void NextInstruction_s()
    {
        MoveToNextStep();
    }

    //Triggered by voice command
    void PreviousInstruction_s()
    {
        MoveToPrevStep();
    }

    void MoveToNextStep()
    {
        if (currentStep == numSteps - 1) return;

        //Special cases where we don't shift all instructions up
        if (currentStep == 0)
        {
            SetStepActive(stepContainer[0], false);
            SetStepActive(stepContainer[1], true);
        }
        else if(currentStep == numSteps - 3)
        {
            SetStepActive(stepContainer[1], false);
            SetStepActive(stepContainer[2], true);
        }
        else if(currentStep == numSteps - 2)
        {
            SetStepActive(stepContainer[2], false);
            SetStepActive(stepContainer[3], true);
        }
        else //General case where we shift all instructions up
        {
            //There's probably a better way to do this but I'm just trying to get it to work right now
            Destroy(stepContainer[0]);
            stepContainer[0] = stepContainer[1];
            SetStepPos(stepContainer[0], 0);
            stepContainer[1] = stepContainer[2];
            SetStepPos(stepContainer[1], 1);
            stepContainer[2] = stepContainer[3];
            SetStepPos(stepContainer[2], 2);
            stepContainer[3] = GenerateStep(data[currentStep + 3]["Step"], data[currentStep + 3]["Text"], data[currentStep + 3]["Caution"], data[currentStep + 3]["Warning"], data[currentStep + 3]["Figure"]);
            SetStepPos(stepContainer[3], 3);

            SetStepActive(stepContainer[0], false);
            SetStepActive(stepContainer[1], true);
        }

        ++currentStep;
    }

    void MoveToPrevStep()
    {
        if (currentStep == 0) return;

        //Special cases where we don't shift all instructions down
        if (currentStep == 1)
        {
            SetStepActive(stepContainer[1], false);
            SetStepActive(stepContainer[0], true);
        }
        else if(currentStep == numSteps - 2)
        {
            SetStepActive(stepContainer[2], false);
            SetStepActive(stepContainer[1], true);
        }
        else if(currentStep == numSteps - 1)
        {
            SetStepActive(stepContainer[3], false);
            SetStepActive(stepContainer[2], true);
        }
        else //General case where we shift all instructions down
        {
            Destroy(stepContainer[3]);
            stepContainer[3] = stepContainer[2];
            SetStepPos(stepContainer[3], 3);
            stepContainer[2] = stepContainer[1];
            SetStepPos(stepContainer[2], 2);
            stepContainer[1] = stepContainer[0];
            SetStepPos(stepContainer[1], 1);
            stepContainer[0] = GenerateStep(data[currentStep - 2]["Step"], data[currentStep - 2]["Text"], data[currentStep - 2]["Caution"], data[currentStep - 2]["Warning"], data[currentStep - 2]["Figure"]);
            SetStepPos(stepContainer[0], 0);

            SetStepActive(stepContainer[2], false);
            SetStepActive(stepContainer[1], true);
        }

        --currentStep;
    }

    //Create a step asset with the given information
    //TODO: add images
    private GameObject GenerateStep(string step, string text, string caution, string warning, string figure)
    {
        int stepval = int.Parse(step);
        bool hasFigure = bool.Parse(figure);

        //We assume an instruction can't have both a warning and a caution string
        //If neither is provided, warningCautionStr will just be ""
        bool isWarning = true;
        string warningCautionStr = warning;
        if (!string.IsNullOrEmpty(caution))
        {
            warningCautionStr = caution;
            isWarning = false;
        }

        GameObject stepClone = Instantiate(stepAsset, stepCanvas.GetComponent<Transform>(), false);

        GameObject warningCautionText = stepClone.transform.Find("WarningCautionText").gameObject;
        Text txt = warningCautionText.GetComponentInChildren<Text>();
        txt.text = warningCautionStr;
        if (isWarning) txt.color = Constants.RED;
        else txt.color = Constants.YELLOW;

        GameObject instructionText = stepClone.transform.Find("InstructionText").gameObject;
        instructionText.GetComponentInChildren<Text>().text = text;

        GameObject stepNumberText = stepClone.transform.Find("StepNumberText").gameObject;
        stepNumberText.GetComponentInChildren<Text>().text = step;

        GameObject progressBar = stepClone.transform.Find("ProgressBar").gameObject;
        Slider bar = progressBar.GetComponent<Slider>();
        bar.minValue = 1;
        bar.maxValue = numSteps;
        bar.value = stepval;

        GameObject imageButton = stepClone.transform.Find("ImageButton").gameObject;
        if (hasFigure)
        {
            string imgpath = string.Format("GeneratorImages/2.{0}", stepval.ToString("D2")); //pad with zeros until length 2
            Sprite img = Resources.Load<Sprite>(imgpath);
            if (!img) Debug.Log("Error loading " + imgpath);
            else imageButton.GetComponentInChildren<Image>().sprite = img;

            imageButton.GetComponent<Button>().onClick.AddListener(SetZoomImage);
        }
        else
        {
            imageButton.SetActive(false);
        }

        return stepClone;
    }

    //Color a procedure step white if it's the active step, else color it gray
    //Also enable or disable the progress bar
    //We'll also update the currentStepPanel if we're active
    private void SetStepActive(GameObject step, bool isActive)
    {
        if(isActive)
        {
            step.GetComponent<Image>().color = Color.white;
            step.transform.Find("ProgressBar").gameObject.SetActive(true);
            SetCurrentStepPanel(step);
        }
        else
        {
            step.GetComponent<Image>().color = Color.gray;
            step.transform.Find("ProgressBar").gameObject.SetActive(false);
        }
    }

    //Set the given step's position in the step display
    //0 <= step < SHOW_NUM_STEPS
    private void SetStepPos(GameObject step, int pos)
    {
        step.GetComponent<RectTransform>().localPosition = new Vector3(0, -2*pos + 3, 0);
    }

    //Copies data from the given step into currentStepPanel
    private void SetCurrentStepPanel(GameObject step)
    {
        Text currentWCText = currentStepPanel.transform.Find("WarningCautionText").gameObject.GetComponentInChildren<Text>();
        Text stepWCText = step.transform.Find("WarningCautionText").gameObject.GetComponentInChildren<Text>();
        currentWCText.text = stepWCText.text;
        currentWCText.color = stepWCText.color;

        Text currentIText = currentStepPanel.transform.Find("InstructionText").gameObject.GetComponentInChildren<Text>();
        Text stepIText = step.transform.Find("InstructionText").gameObject.GetComponentInChildren<Text>();
        currentIText.text = stepIText.text;

        Text currentSText = currentStepPanel.transform.Find("StepNumberText").gameObject.GetComponentInChildren<Text>();
        Text stepSText = step.transform.Find("StepNumberText").gameObject.GetComponentInChildren<Text>();
        currentSText.text = stepSText.text;

        Slider currentSlider = currentStepPanel.transform.Find("ProgressBar").gameObject.GetComponent<Slider>();
        Slider stepSlider = step.transform.Find("ProgressBar").gameObject.GetComponent<Slider>();
        currentSlider.value = stepSlider.value;
    }

    private void SetZoomImage()
    {
        if(!zoomImage.activeInHierarchy)
        {
            Image img = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Image>();
            zoomImage.GetComponent<Image>().sprite = img.sprite;
            zoomImage.SetActive(true);
            currentStepPanel.SetActive(false);
        }
        else
        {
            zoomImage.SetActive(false);
            currentStepPanel.SetActive(true);
        }
    }
}