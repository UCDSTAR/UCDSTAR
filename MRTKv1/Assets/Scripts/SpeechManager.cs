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
            this.BroadcastMessage("ShowNotifications_s");
        });

        keywords.Add("Hide alerts", () =>
        {
            this.BroadcastMessage("HideNotifications_s");
        });

        keywords.Add("Show next", () =>
        {
            this.BroadcastMessage("NextInstruction_s");
        });

        keywords.Add("Show previous", () =>
        {
            this.BroadcastMessage("PreviousInstruction_s");
        });

        keywords.Add("Show next procedure", () =>
        {
            this.BroadcastMessage("NextProcedure_s");
        });

        keywords.Add("Show previous procedure", () =>
        {
            this.BroadcastMessage("PreviousProcedure_s");
        });

        keywords.Add("Show time", () =>
        {
            this.BroadcastMessage("ShowTimer_s");
        });

        keywords.Add("Hide time", () =>
        {
            this.BroadcastMessage("HideTimer_s");
        });

        keywords.Add("Show image", () =>
        {
            this.BroadcastMessage("ShowImage_s");
        });

        keywords.Add("Hide image", () =>
        {
            this.BroadcastMessage("HideImage_s");
        });

        keywords.Add("Show help", () =>
        {
            this.BroadcastMessage("ShowVoiceHelp_s");
        });

        keywords.Add("Hide help", () =>
        {
            this.BroadcastMessage("HideVoiceHelp_s");
        });

        keywords.Add("Show tap to place", () =>
        {
            this.BroadcastMessage("ShowTapToPlace_s");
        });

        keywords.Add("Hide tap to place", () =>
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
