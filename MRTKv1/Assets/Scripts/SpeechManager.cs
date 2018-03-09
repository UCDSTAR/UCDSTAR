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
        keywords.Add("Show notifications", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("showNotifications_s");
        });

        keywords.Add("Hide notifications", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("hideNotifications_s");
        });

        keywords.Add("Next instruction", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("nextInstruction_s");
        });

        keywords.Add("Previous instruction", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("previousInstruction_s");
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