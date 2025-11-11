using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class TutorialManager : MonoBehaviour
{
    public bool enable = true;

    [SerializeField] private bool promptsPauseGame = false;
    public static TutorialManager Instance { get; private set; }

    private Dictionary<string, (string, Vector3)> tutorialPrompts = new Dictionary<string, (string, Vector3)>();
    List<(string, Vector3)> promptQueue = new List<(string, Vector3)>();
    public GameObject promptObject;

    public float charsPerSecond = 25f;
    public float punctuationPause = 0.25f;

    private Coroutine typingRoutine;
    private TextMeshProUGUI tmp;
    private string currentText = "";
    private bool typingComplete = false;


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
        tutorialPrompts["welcome1"] = ("O' hero!\n The court oracle has forseen that the castle is in grave danger!", new Vector3(0, 0, 0));
        tutorialPrompts["welcome2"] = ("O' brave hero!\n I would join you on the battlefield, but...\n then there would be no one to relay the oracles' prophecies...", new Vector3(0, 0, 0));
        tutorialPrompts["welcome3"] = ("Speak of the devil! Here comes another of her prophecies...", new Vector3(0, 0, 0));
        tutorialPrompts["attack"] = ("<color=lightblue> Move around with <b>WASD</b>\nAttack enemies with<b> SPACE</b> or <b> Left Click </b> </color> ", Vector3.zero);
        tutorialPrompts["defend"] = ("You have the kingdom's full support, including the deployment of <b>towers</b> on the map to protect your base.\nBut the royal reserves are thin and we can't afford them yet.", new Vector3(-300, -50, 0));
        Instance.QueuePrompt("welcome1");
        Instance.QueuePrompt("welcome2");
        Instance.QueuePrompt("welcome3");
        Instance.QueuePrompt("attack");
        Instance.QueuePrompt("defend");

        // Wave 1 prompts
        tutorialPrompts["wave1"] = ("Wave 1 is starting!\n<color=red>Enemies</color> are coming from the <color=yellow>yellow path</color>.\nVanquish these foes, and defend your kingdom!", new Vector3(-200, 100, 0));
        tutorialPrompts["firstTower"] = ("Nice! The enemies have dropped enough gold to buy a <b>tower</b>.\nPress <b>1</b> to select the first tower, and <b>SPACE</b> to place it.", new Vector3(-300, -50, 0));
        // Wave 2 prompts
        tutorialPrompts["towerExplanation"] = ("Pressing <b>1-4</b> will display the tower's stats.\nTry to think what situations each would be useful in.", new Vector3(-300, -50, 0));
        // Wave 3 prompts
        tutorialPrompts["newPath"] = ("Aha, the enemies are making a new <color=yellow>path</color>!\nYour tower might not be in the best place to deal with it.", new Vector3(0, -200, 0));
        tutorialPrompts["moveTower"] = ("Don't worry! You can pick up the tower you placed with <b>E</b>\nAnd you can place back down with <b>Q</b>.", new Vector3(0, -200, 0));
        // Wave 4 prompts
        tutorialPrompts["newEnemy"] = ("Watch out! A new type of <color=red>enemy</color> has appeared!\nThese ones are <b>fast</b>, but they don't have much <b>health</b>.", new Vector3(-200, 100, 0));
        // Wave 5 prompts
        tutorialPrompts["pathColour"] = ("Take note of where enemies <color=red>die</color>.\nIf enough enemies die on the path, it might <b>split</b>.", new Vector3(-200, 100, 0));

        // Wave 7 prompts
        tutorialPrompts["checkIn"] = ("You're doing great so far!\nDon't forget you can move towers with <b>E</b> and <b>Q</b>", new Vector3(-200, 100, 0));

        // Wave 10 prompts
        tutorialPrompts["bossEnemy"] = ("A <color=red>boss enemy</color> is approaching!", new Vector3(-200, 100, 0));
        tutorialPrompts["bossPrep"] = ("Make sure you're prepared.\nDon't let it too close to your <b>base!</b>", new Vector3(-200, 100, 0));
        tutorialPrompts["flyingEnemy"] = ("Well done! Though it's not over just yet.\nIt seems new <color=red>flying enemies</color> can only be hit by certain towers.", new Vector3(-200, 100, 0));
    }

    private void Update()
    {
        if (promptObject.activeSelf && Time.timeScale != 0)
        {
            Time.timeScale = promptsPauseGame ? 0 : 1;
        }

        if (promptObject.activeSelf && Input.GetKeyDown(KeyCode.R))
        {
            if (!typingComplete)
                CompleteInstantly();
            else
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
        Time.timeScale = promptsPauseGame ? 0 : 1;

        (string text, Vector3 pos) = promptQueue[0];
        promptQueue.RemoveAt(0);

        promptObject.SetActive(true);
        var rect = promptObject.GetComponent<RectTransform>();
        tmp = promptObject.GetComponentInChildren<TextMeshProUGUI>();
        rect.anchoredPosition3D = pos;

        StartTyping(text);
    }

    private void ClosePrompt()
    {
        if (promptObject.activeSelf)
        {
            promptObject.SetActive(false);
        }

        Time.timeScale = promptsPauseGame ? 1 : 0; //One line if statement
    }

    public void QueuePrompt(string key)
    {
        if (tutorialPrompts.ContainsKey(key))
        {
            promptQueue.Add(tutorialPrompts[key]);
            tutorialPrompts.Remove(key);  // so they arent shown again
        }
    }

    private void StartTyping(string fullText)
    {
        StopTyping();
        typingRoutine = StartCoroutine(TypeRoutine(fullText));
    }

    private void StopTyping()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);
        typingRoutine = null;
    }

    private IEnumerator TypeRoutine(string fullText)
    {
        tmp.text = "";
        currentText = fullText;
        typingComplete = false;

        float delay = 1f / Mathf.Max(1f, charsPerSecond);
        int i = 0;

        while (i < fullText.Length)
        {
            // if theres a markup tag, add the entire thing to prevent mess
            if (fullText[i] == '<')
            {
                int closingIndex = fullText.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    tmp.text += fullText.Substring(i, closingIndex - i + 1);
                    i = closingIndex + 1;
                    continue;
                }
            }

            tmp.text += fullText[i];
            char c = fullText[i];
            i++;

            if (c == '.' || c == ',' || c == '!' || c == '?')
                yield return new WaitForSeconds(punctuationPause);
            else
                yield return new WaitForSeconds(delay);
        }

        typingComplete = true;
        typingRoutine = null;
    }

    private void CompleteInstantly()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        tmp.text = currentText;
        typingComplete = true;
    }


}
