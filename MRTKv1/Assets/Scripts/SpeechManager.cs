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
        keywords.Add("Alerts", () =>
        {
            this.BroadcastMessage("ShowNotifications_s");
        });

        keywords.Add("Hide alerts", () =>
        {
            this.BroadcastMessage("HideNotifications_s");
        });

        keywords.Add("Next", () =>
        {
            this.BroadcastMessage("NextInstruction_s");
        });

        keywords.Add("Back", () =>
        {
            this.BroadcastMessage("PreviousInstruction_s");
        });

        keywords.Add("Time", () =>
        {
            this.BroadcastMessage("ToggleTimer_s");
        });

        keywords.Add("Show image", () =>
        {
            this.BroadcastMessage("ShowImage_s");
        });

        keywords.Add("Hide image", () =>
        {
            this.BroadcastMessage("HideImage_s");
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
