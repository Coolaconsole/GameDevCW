using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static Unity.Burst.Intrinsics.X86.Avx;
using Vector3 = UnityEngine.Vector3;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public bool enable = true;
    [SerializeField] private bool allPromptsPauseGame = false;
    [SerializeField] private bool eventBasedPromptCompletion = false;
    //(string = tutorial text, Vector3 = position on screen, UnityEvent = Event tied to tutorial, bool = if prompt pauses the game)
    private Dictionary<string, (string, Vector3, UnityEvent, bool)> tutorialPrompts = new Dictionary<string, (string, Vector3, UnityEvent, bool)>();
    private HashSet<string> completedEvents = new HashSet<string>();
    
    [Header("Tutorial UI")]
    List<(string key, string text, Vector3 pos, bool pausesGame)> promptQueue = new List<(string, string, Vector3, bool)>();
    public GameObject promptObject;

    public float charsPerSecond = 25f;
    public float punctuationPause = 0.25f;

    private Coroutine typingRoutine;
    private TextMeshProUGUI tmp;
    private string currentText = "";
    private bool typingComplete = false;
    
    private string currentPromptKey = "";
    private bool currentPromptPausesGame = false;
    
    [Header("Others")]
    public static TutorialManager Instance { get; private set; }

    //Events that can be called from other scripts
    [HideInInspector] public UnityEvent onTowerPlaced = new UnityEvent();
    [HideInInspector] public UnityEvent onTowerPickup = new UnityEvent();
    [HideInInspector] public UnityEvent onTowerDrop = new UnityEvent();
    [HideInInspector] public UnityEvent onTowerSelected = new UnityEvent();
    [HideInInspector] public UnityEvent onPlayerMoved = new UnityEvent();
    [HideInInspector] public UnityEvent onPlayerAttack = new UnityEvent();
    [HideInInspector] public UnityEvent onMoneyPickup = new UnityEvent();
    [HideInInspector] public UnityEvent onEnemyDeath = new UnityEvent();
    [HideInInspector] public UnityEvent onTowerDeselected = new UnityEvent();

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
        // Starting Prompts
        tutorialPrompts["welcome"] = ("O hero! The court oracle has foreseen <color=red>grave danger</color> for the castle!", new Vector3(0, 0, 0), null, true);
        tutorialPrompts["move"] = ("Brave hero! I’d join you in battle, but someone must relay the oracle’s <b>prophecies</b>!", new Vector3(0, 0, 0), null, false);
        tutorialPrompts["attack"] = ("<color=lightblue>Move with <b>WASD</b> and attack using <b>SPACE</b> or <b>Left Click</b>.</color>", Vector3.zero, null, false);
        tutorialPrompts["defend"] = ("The kingdom grants you the power to deploy <b>towers</b>. But the royal reserves are low — we can’t afford them yet.", new Vector3(-300, -50, 0), onMoneyPickup, true);

        Instance.QueuePrompt("welcome");
        Instance.QueuePrompt("move");
        Instance.QueuePrompt("attack");
        Instance.QueuePrompt("defend");

        // Wave 1 prompts
        tutorialPrompts["wave1"] = ("O hero, the oracle foretells our first trial! <color=red>Enemies</color> march forth along the <color=yellow>yellow path</color>. Stand firm and defend the realm!", new Vector3(-200, 100, 0), onEnemyDeath, true);
        tutorialPrompts["firstTower"] = ("Fortune smiles upon us! The fallen foes have yielded enough gold for a <b>tower</b>. Press <b>1</b> to select it, and <b>SPACE</b> to place it.", new Vector3(-300, -50, 0), onTowerPlaced, false);

        // Wave 2 prompts
        tutorialPrompts["towerExplanation"] = ("Each tower has its strengths! Press <b>1–4</b> to reveal their stats. I will leave their strategic deployment to you, O wise hero!", new Vector3(-300, -50, 0), null, false);

        // Wave 3 prompts
        tutorialPrompts["newPath"] = ("By the heavens! Our adversaries have carved a new <color=yellow>path</color> into our realm! Your tower may not stand in the most... strategic spot.", new Vector3(0, -200, 0), null, true);
        tutorialPrompts["moveTower"] = ("Fret not, dear hero! You can pick up the tower you placed with <b>E</b> and place it back down with <b>Q</b>.", new Vector3(0, -200, 0), onTowerDrop, false);

        // Wave 4 prompts
        tutorialPrompts["newEnemy"] = ("Another dire omen! The oracle has revealed a new breed of <color=red>enemy</color> approaching! These ones are <b>fast</b> on their feet, but they don’t have much <b>health</b>.", new Vector3(-200, 100, 0), null, true);

        // Wave 5 prompts
        tutorialPrompts["pathColour"] = ("Heed the oracle’s counsel: watch where the <color=red>fallen</color> lie. The enemies grow stronger in tiles stained with their <color=red>blood</color>! Too many slain in one place, and the path itself may <b>split</b>!", new Vector3(-200, 100, 0), null, true);

        // Wave 7 prompts
        tutorialPrompts["checkIn"] = ("I commend your valor, O persistent hero! Don’t forget you can move towers with <b>E</b> and <b>Q</b>.", new Vector3(-200, 100, 0), null, false);

        // Wave 10 prompts
        tutorialPrompts["bossEnemy"] = ("The castle trembles — a <color=red>boss enemy</color> is approaching!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["bossPrep"] = ("Make haste with preparation! Don’t let it get close to the <b>castle</b>, lest the kingdom fall!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["flyingEnemy"] = ("Splendid, O valiant hero! Yet the oracle warns of new peril — <color=red>flying enemies</color> can only be struck by towers with sufficient range. Take care!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["kamikaze"] = ("Terrible fortune! Another adversary has been revealed by the oracle. This mysterious foe bears <b>explosive power</b>! Catch it before it tears a hole in our <b>defenses</b>!", new Vector3(-200, 100, 0), null, true);


    }

    private void Update()
    {
        if (promptObject.activeSelf && Time.timeScale != 0)
        {
            //Whether all prompts pause game or just this specific one pause game
            Time.timeScale = allPromptsPauseGame || currentPromptPausesGame ? 0f : 1f;
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
        (string key, string text, Vector3 pos, bool pausesGame) = promptQueue[0];
        promptQueue.RemoveAt(0);

        currentPromptKey = key;
        currentPromptPausesGame = pausesGame;
        
        promptObject.SetActive(true);
        var rect = promptObject.GetComponent<RectTransform>();
        tmp = promptObject.GetComponentInChildren<TextMeshProUGUI>();
        rect.anchoredPosition3D = pos;

        //If the prompt is specified to pause the game or everyone does 
        if (pausesGame || allPromptsPauseGame)
        {
            //Cannot have typing animation so just show text
            StartTyping(text);
            CompleteInstantly();
            
            Time.timeScale = 0;
        }
        else //Start Typing Animation
        {
            Time.timeScale = 1f;
            StartTyping(text);
        }
    }

    public void ClosePrompt()
    {
        if (promptObject.activeSelf)
        {
            promptObject.SetActive(false);
            currentPromptKey = "";
        }

        Time.timeScale = 1f;
    }

    public void QueuePrompt(string key)
    {
        if (tutorialPrompts.ContainsKey(key))
        {
            //Checks for event completion
            if (completedEvents.Contains(key))
            {
                tutorialPrompts.Remove(key);
                return;
            }
            
            var (text, pos, unityEvent, pause) = tutorialPrompts[key];
            promptQueue.Add((key, text, pos, pause));

            //If there is an event subscribe to it, if it goes off complete tutorial prompt
            if (unityEvent != null)
            {
                unityEvent.AddListener(() => { OnEventTriggerd(key); });
            }
            
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

    public void OnEventTriggerd(string eventKey)
    {
        //Turns this on to allow event triggering
        if (!eventBasedPromptCompletion) { return; }
        
        completedEvents.Add(eventKey);

        if (currentPromptKey == eventKey)
        {
            ClosePrompt();
        }
        
        //Removes it from the queue if it hasn't come up yet
        promptQueue.RemoveAll(p => p.key == eventKey);
    }


}
