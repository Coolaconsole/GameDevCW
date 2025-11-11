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
    public List<(string, Vector3)> promptQueue = new List<(string, Vector3)>();
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

    public bool HasUnclosedPrompts()
    {
        return (promptQueue.Count > 0) || promptObject.activeSelf;
    }

    private void Start()
    {
        tutorialPrompts["welcome1"] = ("O hero!\nThe court oracle has foreseen grave danger for the castle!", new Vector3(0, 0, 0));
        tutorialPrompts["welcome2"] = ("Brave hero!\nI’d join you in battle, but someone must relay the oracle’s prophecies!", new Vector3(0, 0, 0));
        tutorialPrompts["welcome3"] = ("Speak of the devil — another prophecy arrives...", new Vector3(0, 0, 0));
        tutorialPrompts["attack"] = ("<color=lightblue> Move with <b>WASD</b>\nAttack using<b> SPACE</b> or <b> Left Click </b> </color> ", Vector3.zero);
        tutorialPrompts["defend"] = ("The kingdom grants you the power to deploy <b>towers</b>.\nBut the royal reserves are low — we can’t afford them yet.", new Vector3(-300, -50, 0));
        Instance.QueuePrompt("welcome1");
        Instance.QueuePrompt("welcome2");
        Instance.QueuePrompt("welcome3");
        Instance.QueuePrompt("attack");
        Instance.QueuePrompt("defend");

        // Wave 1 prompts
        tutorialPrompts["wave1"] = ("Wave 1 is starting!\n<color=red>Enemies</color> are coming from the <color=yellow>yellow path</color>.\nVanquish these foes, and defend your kingdom!", new Vector3(-200, 100, 0));
        tutorialPrompts["firstTower"] = ("Fortune smiles upon us!\nThe fallen foes have yielded enough gold for a <b>tower</b>.\nPress <b>1</b> to select it, and <b>SPACE</b> to place it.", new Vector3(-300, -50, 0));
        // Wave 2 prompts
        tutorialPrompts["towerExplanation"] = ("Each tower has its strengths!\n<b>1-4</b> will reveal the tower's stats.\nI will leave their strategic deployment to you, O wise hero!", new Vector3(-300, -50, 0));
        // Wave 3 prompts
        tutorialPrompts["newPath"] = ("By the heavens!\nOur adversaries have carved a new <color=yellow>path</color> into our realm!\nYour tower may not stand in the most... strategic spot..", new Vector3(0, -200, 0));
        tutorialPrompts["moveTower"] = ("Fret not, dear hero!\nYou can pick up the tower you placed with <b>E</b>\nAnd you can place back down with <b>Q</b>.", new Vector3(0, -200, 0));
        // Wave 4 prompts
        tutorialPrompts["newEnemy"] = ("Another dire omen!\nThe oracle has revealed a new breed of <color=red>enemy</color> approaching!\nThese ones are <b>fast</b> on their feet, but they don't have much <b>health</b>.", new Vector3(-200, 100, 0));
        // Wave 5 prompts
        tutorialPrompts["pathColour"] = ("Heed the oracle’s counsel: watch where the <color=red>fallen</color> lie.\nThe enemies become even more fearsome in tiles stained with their fallen's <color=red>blood</color>!\nToo many slain in one place, and the path itself may <b>split</b>!", new Vector3(-200, 100, 0));

        // Wave 7 prompts
        tutorialPrompts["checkIn"] = ("I commend your valor, O persistent hero!\nDon't forget you can move towers with <b>E</b> and <b>Q</b>", new Vector3(-200, 100, 0));

        // Wave 10 prompts
        tutorialPrompts["bossEnemy"] = ("The castle trembles - a <color=red>boss enemy</color> is approaching!", new Vector3(-200, 100, 0));
        tutorialPrompts["bossPrep"] = ("Make haste with preparation!\nDon't let get close to the <b>castle!</b>, lest the kingdom fall!", new Vector3(-200, 100, 0));
        tutorialPrompts["flyingEnemy"] = ("Spledid, O valiant hero!\nYet the orcale warns of new peril - \nnew <color=red>flying enemies</color> can only be struck by towers with sufficient range. Take care...", new Vector3(-200, 100, 0));
        tutorialPrompts["kamikaze"] = ("Terrible fortune!\nAnother adversary has been forclosed to us by the oracle.\nThis mysterious foe has explosive power! Catch it before it tears a hole in our defences!..", new Vector3(-200, 100, 0));
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
        promptObject.SetActive(true);

        (string text, Vector3 pos) = promptQueue[0];
        promptQueue.RemoveAt(0);

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
