using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start()
    {
        keywords.Add("Show alerts", () =>
        {
            this.BroadcastMessage("ShowAlerts_s");
        });

        keywords.Add("Hide alerts", () =>
        {
            this.BroadcastMessage("HideAlerts_s");
        });

        keywords.Add("Show next", () =>
        {
            this.BroadcastMessage("ShowNext_s");
        });

        keywords.Add("Show previous", () =>
        {
            this.BroadcastMessage("ShowPrevious_s");
        });

        keywords.Add("Show next procedure", () =>
        {
            this.BroadcastMessage("ShowNextProcedure_s");
        });

        keywords.Add("Show previous procedure", () =>
        {
            this.BroadcastMessage("ShowPreviousProcedure_s");
        });

        keywords.Add("Show time", () =>
        {
            this.BroadcastMessage("ShowTime_s");
        });

        keywords.Add("Hide time", () =>
        {
            this.BroadcastMessage("HideTime_s");
        });

        keywords.Add("Show image", () =>
        {
            this.BroadcastMessage("ShowImage_s");
        });

        keywords.Add("Hide image", () =>
        {
            this.BroadcastMessage("HideImage_s");
        });

        keywords.Add("Show task board", () =>
        {
            this.BroadcastMessage("ShowTaskboard_s");
        });

        keywords.Add("Hide task board", () =>
        {
            this.BroadcastMessage("HideTaskboard_s");
        });

        keywords.Add("Show help", () =>
        {
            this.BroadcastMessage("ShowHelp_s");
        });

        keywords.Add("Hide help", () =>
        {
            this.BroadcastMessage("HideHelp_s");
        });

        keywords.Add("Show placement", () =>
        {
            this.BroadcastMessage("ShowTapToPlace_s");
        });

        keywords.Add("Hide placement", () =>
        {
            this.BroadcastMessage("HideTapToPlace_s");
        });

        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}
