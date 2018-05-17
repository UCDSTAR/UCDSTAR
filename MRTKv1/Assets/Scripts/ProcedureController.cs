using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//NOTE: some parts of this program assume there are at least 3 steps!!!
//Steps are 0-indexed (i.e. step #1 is considered to be step 0)

public class ProcedureController : MonoBehaviour
{
    public string csvName;
    public GameObject stepAsset; //this is cloned
    public GameObject currentStepPanel; //this is not cloned, it's updated as needed
    public Canvas stepCanvas;
    public Image enlargedImage;
    public Button prevButton;
    public Button nextButton;
    public GameObject titleText;

    private List<Dictionary<string, string>> data;
    private List<int> numStepsList;
    private List<int> procedureOffsetsList;
    private List<string> procedureNamesList;
    private int numSteps;
    private int offset;
    private int currentStep;
    private int currentProcedure;
    private GameObject[] stepContainer;
    private const int SHOW_NUM_STEPS = 3;
    private bool isImageExpanded;

    //Acts as constructor for object
    void Awake()
    {
        //Parse csv file
        data = CSVReader.Read(csvName);

        //Init vars
        numStepsList = new List<int>();
        procedureOffsetsList = new List<int>();
        procedureNamesList = new List<string>();
        stepContainer = new GameObject[SHOW_NUM_STEPS];

        //Parse
        ParseCSVData();
    }

    //Parse csv data to extract procedures
    void ParseCSVData()
    {
        int index = -1;
        int row;
        for(row = 0; row < data.Count; ++row)
        {
            if(data[row]["Step"].StartsWith("Name:")) //indicates new procedure
            {
                ++index;
                procedureOffsetsList.Add(row + 1); //skip NAME row
                procedureNamesList.Add(data[row]["Step"].Substring(5));
                numStepsList.Add(0);
            }
            else
            {
                numStepsList[index]++;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        ProcedureInit(0);

        //Setup touch navigation
        currentStepPanel.transform.Find("LeftArrowButton").gameObject.GetComponent<Button>().onClick.AddListener(MoveToPrevStep);
        currentStepPanel.transform.Find("RightArrowButton").gameObject.GetComponent<Button>().onClick.AddListener(MoveToNextStep);
        //SetPrevNextStepButtons();
        prevButton.onClick.AddListener(MoveToPrevStep);
        nextButton.onClick.AddListener(MoveToNextStep);
    }

    //Initializes the procedure display with the first few steps
    void ProcedureInit(int procedureIndex)
    {
        currentProcedure = procedureIndex;
        isImageExpanded = false;
        offset = procedureOffsetsList[procedureIndex];
        numSteps = numStepsList[procedureIndex];
        
        currentStep = 0; //first step

        //Clear out container
        if (stepContainer[0] != null) Destroy(stepContainer[0]);
        if (stepContainer[1] != null) Destroy(stepContainer[1]);
        if (stepContainer[2] != null) Destroy(stepContainer[2]);

        //Update title text
        titleText.GetComponentInChildren<Text>().text = procedureNamesList[procedureIndex];

        //Init container with first few steps
        for (int i = 0; i < SHOW_NUM_STEPS; ++i)
        {
            stepContainer[i] = GenerateStep(data[i + offset]["Step"], data[i + offset]["Text"], data[i + offset]["Caution"], data[i + offset]["Warning"], data[i + offset]["Figure"]);
            DrawStepAtPos(stepContainer[i], i);
        }

        //Initialize currentStepPanel
        GameObject progressBar = currentStepPanel.transform.Find("ProgressBar").gameObject;
        progressBar.GetComponent<Slider>().minValue = 1;
        progressBar.GetComponent<Slider>().maxValue = numSteps;

        //Set first instruction as active
        SetStepActive(stepContainer[0], true);
    }

    //Enable clicking on previous or next step to navigate
    /*
    void SetPrevNextStepButtons()
    {
        //Disable all step buttons
        stepContainer[0].GetComponent<Button>().onClick.RemoveAllListeners();
        stepContainer[1].GetComponent<Button>().onClick.RemoveAllListeners();
        stepContainer[2].GetComponent<Button>().onClick.RemoveAllListeners();

        //Add the listeners we want
        int currentIndex = GetCurrentContainerIndex();
        if (currentIndex - 1 >= 0)
            stepContainer[currentIndex - 1].GetComponent<Button>().onClick.AddListener(MoveToPrevStep);
        if (currentIndex + 1 < SHOW_NUM_STEPS)
            stepContainer[currentIndex + 1].GetComponent<Button>().onClick.AddListener(MoveToNextStep);
    }
    */

    //Triggered by voice command
    void ShowNext_s()
    {
        MoveToNextStep();
    }

    //Triggered by voice command
    void ShowPrevious_s()
    {
        MoveToPrevStep();
    }

    void MoveToNextStep()
    {
        if (isImageExpanded)
            return;

        if (currentStep == numSteps - 1) //if on last step, don't move
            return;

        //Special cases where we don't shift all instructions up
        if (currentStep == 0) //on first step
        {
            SetStepActive(stepContainer[0], false);
            SetStepActive(stepContainer[1], true);
        }
        else if (currentStep == numSteps - 2) //on second to last step
        {
            SetStepActive(stepContainer[1], false);
            SetStepActive(stepContainer[2], true);
        }
        else //General case where we shift all instructions up
        {
            //There's probably a better way to do this but I'm just trying to get it to work right now
            Destroy(stepContainer[0]);
            stepContainer[0] = stepContainer[1];
            DrawStepAtPos(stepContainer[0], 0);
            stepContainer[1] = stepContainer[2];
            DrawStepAtPos(stepContainer[1], 1);
            stepContainer[2] = GenerateStep(data[currentStep + offset + 2]["Step"], data[currentStep + offset + 2]["Text"], data[currentStep + offset + 2]["Caution"], data[currentStep + offset + 2]["Warning"], data[currentStep + offset + 2]["Figure"]);
            DrawStepAtPos(stepContainer[2], 2);

            SetStepActive(stepContainer[0], false);
            SetStepActive(stepContainer[1], true);
        }

        ++currentStep;

        //SetPrevNextStepButtons();
    }

    void MoveToPrevStep()
    {
        if (isImageExpanded)
            return;

        if (currentStep == 0) //if on first step, don't move
            return;

        //Special cases where we don't shift all instructions down
        if (currentStep == 1) //on second step
        {
            SetStepActive(stepContainer[1], false);
            SetStepActive(stepContainer[0], true);
        }
        else if (currentStep == numSteps - 1) //on last step
        {
            SetStepActive(stepContainer[2], false);
            SetStepActive(stepContainer[1], true);
        }
        else //General case where we shift all instructions down
        {
            Destroy(stepContainer[2]);
            stepContainer[2] = stepContainer[1];
            DrawStepAtPos(stepContainer[2], 2);
            stepContainer[1] = stepContainer[0];
            DrawStepAtPos(stepContainer[1], 1);
            stepContainer[0] = GenerateStep(data[currentStep + offset - 2]["Step"], data[currentStep + offset - 2]["Text"], data[currentStep + offset - 2]["Caution"], data[currentStep + offset - 2]["Warning"], data[currentStep + offset - 2]["Figure"]);
            DrawStepAtPos(stepContainer[0], 0);

            SetStepActive(stepContainer[2], false);
            SetStepActive(stepContainer[1], true);
        }

        --currentStep;

        //SetPrevNextStepButtons();
    }

    void ShowNextProcedure_s()
    {
        MoveToNextProcedure();
    }

    void ShowPreviousProcedure_s()
    {
        MoveToPrevProcedure();
    }

    void MoveToNextProcedure()
    {
        if (isImageExpanded)
            return;

        int nextIndex = currentProcedure + 1;
        if(nextIndex >= procedureNamesList.Count) //wraparound
        {
            nextIndex = 0;
        }
        ProcedureInit(nextIndex);
    }

    void MoveToPrevProcedure()
    {
        if (isImageExpanded)
            return;

        int prevIndex = currentProcedure - 1;
        if (prevIndex < 0) //wraparound
        {
            prevIndex = procedureNamesList.Count - 1;
        }
        ProcedureInit(prevIndex);
    }

    void ToggleImage()
    {
        if (isImageExpanded) //hide image
        {
            HideImage();
        }
        else //enlarge image
        {
            ShowImage();
        }

        //Don't forget to toggle!
        isImageExpanded = !isImageExpanded;
    }

    //Triggered by voice command
    void ShowImage_s()
    {
        //Check if we have an image to hide
        int currentIndex = GetCurrentContainerIndex();
        GameObject imageButton = stepContainer[currentIndex].transform.Find("ImageButton").gameObject;
        if (imageButton.activeInHierarchy)
            ShowImage();
    }

    //Triggered by voice command
    void HideImage_s()
    {
        //Check if we have an image to hide
        int currentIndex = GetCurrentContainerIndex();
        GameObject imageButton = stepContainer[currentIndex].transform.Find("ImageButton").gameObject;
        if (imageButton.activeInHierarchy)
            HideImage();
    }

    void ShowImage()
    {
        //Hide all steps
        stepContainer[0].SetActive(false);
        stepContainer[1].SetActive(false);
        stepContainer[2].SetActive(false);

        //Display current step at top position
        int currentIndex = GetCurrentContainerIndex();
        DrawStepAtPos(stepContainer[currentIndex], 0);
        stepContainer[currentIndex].SetActive(true);

        //Get current step's image
        Sprite currentSprite = stepContainer[currentIndex].transform.Find("ImageButton").gameObject.GetComponentInChildren<Image>().sprite;

        //Set enlarged image to current image and show
        enlargedImage.sprite = currentSprite;
        enlargedImage.preserveAspect = true;
        enlargedImage.gameObject.SetActive(true);

        //Set current step to show back button in place of image
        Sprite backButton = Resources.Load<Sprite>("Icons/arrowLeft");
        stepContainer[currentIndex].transform.Find("ImageButton").gameObject.GetComponentInChildren<Image>().sprite = backButton;
    }

    void HideImage()
    {
        //Hide image
        enlargedImage.gameObject.SetActive(false);

        //Move current step back into place
        int currentIndex = GetCurrentContainerIndex();
        DrawStepAtPos(stepContainer[currentIndex], currentIndex);

        //Reset current step's image
        stepContainer[currentIndex].transform.Find("ImageButton").gameObject.GetComponentInChildren<Image>().sprite = enlargedImage.sprite;

        //Re-enable all steps
        stepContainer[0].SetActive(true);
        stepContainer[1].SetActive(true);
        stepContainer[2].SetActive(true);
    }

    //Create a step asset with the given information
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
            if (!img)
                Debug.Log("Error loading " + imgpath);
            else
                imageButton.GetComponent<Image>().sprite = img;
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
    private void SetStepActive(GameObject step, bool setActive)
    {
        if (setActive)
        {
            step.GetComponent<Image>().color = Constants.ACTIVE_STEP;
            step.transform.Find("ProgressBar").gameObject.SetActive(true);
            SetCurrentStepPanel(step);
            GameObject imageButton = step.transform.Find("ImageButton").gameObject;
            if(imageButton.activeInHierarchy)
            {
                imageButton.GetComponent<Button>().onClick.AddListener(ToggleImage);
            }
        }
        else
        {
            step.GetComponent<Image>().color = Constants.INACTIVE_STEP;
            step.transform.Find("ProgressBar").gameObject.SetActive(false);
            GameObject imageButton = step.transform.Find("ImageButton").gameObject;
            if (imageButton.activeInHierarchy)
            {
                imageButton.GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }
    }

    //Draws the given step at the given position in the step display
    //0 <= step < SHOW_NUM_STEPS
    //This function DOES NOT alter the step's position in the stepContainer
    private void DrawStepAtPos(GameObject step, int pos)
    {
        step.GetComponent<RectTransform>().localPosition = new Vector3(0, -2.667f * pos + 2.667f, 0);
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

        Slider currentSlider = currentStepPanel.transform.Find("ProgressBar").gameObject.GetComponent<Slider>();
        Slider stepSlider = step.transform.Find("ProgressBar").gameObject.GetComponent<Slider>();
        currentSlider.value = stepSlider.value;
    }

    private int GetCurrentContainerIndex()
    {
        if (currentStep == 0)
            return 0;
        else if (currentStep == numSteps - 1)
            return 2;
        else
            return 1;
    }
}
