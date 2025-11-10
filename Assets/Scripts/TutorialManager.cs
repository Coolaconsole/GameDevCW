using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public bool enable = true;
    public static TutorialManager Instance { get; private set; }

    private Dictionary<string, (string, Vector3)> tutorialPrompts = new Dictionary<string, (string, Vector3)>();
    List<(string, Vector3)> promptQueue = new List<(string, Vector3)>();
    public GameObject promptObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        tutorialPrompts["attack"] = ("Test prompt", new Vector3(0, 0, 0));
        tutorialPrompts["defend"] = ("Test prompt", new Vector3(0, -200, 0));

        Instance.QueuePrompt("attack");
        Instance.QueuePrompt("defend");
    }

    private void Update()
    {
        if (promptObject.activeSelf && Time.timeScale != 0)
        {
            Time.timeScale = 0;
        }

        if (promptObject.activeSelf && Input.GetKeyDown(KeyCode.P))
        {
            ClosePrompt();
            return;
        }

        if (enable && !promptObject.activeSelf && promptQueue.Count > 0)
        {
            ShowNextPrompt();
        }
    }

    private void ShowNextPrompt()
    {
        Time.timeScale = 0;

        (string text, Vector3 pos) = promptQueue[0];
        promptQueue.RemoveAt(0);

        promptObject.SetActive(true);
        var rect = promptObject.GetComponent<RectTransform>();
        var tmp = promptObject.GetComponentInChildren<TextMeshProUGUI>();

        tmp.text = text;
        rect.anchoredPosition3D = pos; 
    }

    private void ClosePrompt()
    {
        if (promptObject.activeSelf)
        {
            promptObject.SetActive(false);
        }

        Time.timeScale = 1;
    }

    public void QueuePrompt(string key)
    {
        if (tutorialPrompts.ContainsKey(key))
        {
            promptQueue.Add(tutorialPrompts[key]);
            tutorialPrompts.Remove(key);  // so they arent shown again
        }
    }


}
