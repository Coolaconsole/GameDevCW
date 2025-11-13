using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
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
        DontDestroyOnLoad(gameObject);
    }

    public bool HasUnclosedPrompts()
    {
        return (promptQueue.Count > 0) || promptObject.activeSelf;
    }

    private void Start()
    {
        // Starting Prompts
        tutorialPrompts["welcome"] = ("O hero!\nThe court oracle has foreseen <color=red>grave danger</color> for the castle!", new Vector3(0, 0, 0), null, true);
        tutorialPrompts["move"] = ("Brave hero!\nI'd join you in battle, but someone must relay the oracle's <color=lightblue>prophecies</color>!", new Vector3(0, 0, 0), null, false);
        tutorialPrompts["welcome2"] = ("Speak of the devil!\nHere comes another of her <color=lightblue>prophecies</color>...", new Vector3(0, 0, 0), null, false);
        tutorialPrompts["attack"] = ("<color=lightblue>Move with <b>WASD</b></color> and <color=lightblue>attack using <b>Left Click</b>.</color>", Vector3.zero, null, false);
        tutorialPrompts["defend"] = ("The kingdom grants you the power to deploy <color=lightblue>towers</color>.\nBut the royal reserves are low <color=red>we can't afford them yet!</color>", new Vector3(-300, 0, 0), onMoneyPickup, true);

        Instance.QueuePrompt("welcome");
        Instance.QueuePrompt("move");
        Instance.QueuePrompt("welcome2");
        Instance.QueuePrompt("attack");
        Instance.QueuePrompt("defend");

        tutorialPrompts["lowHealth"] = ("Watch out, hero!\n The <color=red>castle is under enemy fire!</color> Watch out for enemies that hide <color=red> behind </color> the castle in its shadow!", new Vector3(0, 0, 0), null, true);

        // Wave 1 prompts
        tutorialPrompts["wave1"] = ("O hero, the oracle foretells our first trial!\n<color=red>Enemies</color> march forth along the <color=yellow>yellow path</color>. Stand firm and defend the realm!", new Vector3(0, 0, 0), onEnemyDeath, true);
        tutorialPrompts["firstTower"] = ("Fortune smiles upon us!\nThe fallen foes have yielded <color=lightblue>enough gold for a tower</color>.\nPress  <color=lightblue>Key Number 1</color> to select it,\nand  <color=lightblue>Left Click</color> to place it.", new Vector3(-300, 0, 0), onTowerPlaced, false);

        // Wave 2 prompts
        tutorialPrompts["towerExplanation"] = ("Each tower has its strengths!\nPress <color=lightblue>Keys 1 - 4</color> to reveal their stats. I will leave their strategic deployment to you, O wise hero!\nRemeber to <color=lightblue> deselect </color> the tower by pressing <color=lightblue> 1 </color> again!", new Vector3(-300, 0, 0), null, false);

        // Wave 3 prompts
        tutorialPrompts["newPath"] = ("By the heavens!\nOur adversaries are carving <color=yellow> a new path</color> into our realm!\nYour tower may not stand in the most... strategic spot.", new Vector3(0, -100, 0), null, true);
        tutorialPrompts["moveTower"] = ("Fret not, dear hero!\nYou can <color=lightblue>pick up the tower</color> you placed with <color=lightblue>E</color>\nand place it back down with <color=lightblue>the same button</color>.", new Vector3(-300, 0, 0), onTowerDrop, true);

        // Wave 5 prompts
        tutorialPrompts["newEnemy"] = ("Another dire omen!\nThe oracle has revealed a <color=red>new breed of enemy</color> approaching!\nThese ones are <color=red>fast</color> on their feet, but they don't have much <color=red>health</color>.", new Vector3(-200, 100, 0), null, true);

        // Wave 4 prompts
        tutorialPrompts["pathColour"] = ("Hear the oracle's counsel!\nThe <color=red>enemies grow stronger</color> in tiles <color=red>stained red with their blood</color>!\nToo many slain in one place, and the path itself may <b>split</b>!\nAttack the enemies from different angles to prevent catastrophe...", new Vector3(-200, 100, 0), null, false);

        // Wave 6 promppt
        tutorialPrompts["healing"] = ("O weary hero\nKnow that you do not fight alone!\nThe kingdom sends it best craftsmen to <color=lightblue> repair your standing structures between each wave of foes.</color>", new Vector3(0, 0, 0), null, false);
        tutorialPrompts["pathSplit"] = ("Hark, hero!\nA tile on the map has <color=red>absorbed enough of our enemies blood</color> and is <color=red>about to split</color>! Consider <color=lightblue> relocating your towers</color>, lest you fight this war on more fronts...", new Vector3(-200, 100, 0), null, true);
        
        // Wave 7 prompts
        tutorialPrompts["toughEnemy"] = ("Noble hero, it appears our enemies have raised <color=red>tougher undead foes</color> to seige our castle.\nSuch insolence cannot be tolerated!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["anotherPath"] = ("Lo, as predicted, our foes are carving <color=yellow>another path</color> through our land. Stay vigilant, hero!", new Vector3(-200, 100, 0), null, false);
        tutorialPrompts["checkIn"] = ("I commend your valor, O persistent hero!\nDon't forget you can  <color=lightblue>move towers</color> with  <color=lightblue>E</color>.", new Vector3(-300, 0, 0), null, false);

        // Wave 10 prompts
        tutorialPrompts["bossEnemy"] = ("The castle trembles a <color=red>boss enemy</color> is approaching!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["bossPrep"] = ("Make haste with preparation!\nDon't let it get close to the <b>castle</b>, lest the kingdom fall!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["flyingEnemy"] = ("Splendid, O valiant hero!\nYet the oracle warns of new peril <color=red>flying enemies</color> can only be struck by towers with <color=red>sufficient range</color>. Take care!", new Vector3(-200, 100, 0), null, true);
        tutorialPrompts["kamikaze"] = ("Terrible fortune!\n<color=red>Another adversary</color> has been revealed by the oracle.\nThis one packs some <color=red>explosive power</color>!\nListen for its <color=red>cackle</color> and catch it before it tears a hole in our <b>defenses</b>!", new Vector3(-200, 100, 0), null, true);
        
        tutorialPrompts["bossLevels"] = ("The tremors grow in numbers, the oracle forsees a <color=red>boss wave</color> every <color=yellow>five waves</color>!\nOur foes sure seem to be determined yet.", new Vector3(-200, 100, 0), null, false);

        // Wave 25 prompts
        tutorialPrompts["success"] = ("Hero...\nKnow that your name will be etched into the cannals of history.\nNo matter the outcome of this fight, know that you have <color=red>earnt your title</color>!", new Vector3(0, 0, 0), null, true);
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
            {
                AudioManager.Instance.PlaySFX("click", 0.1f, 1f, 1f);
                ClosePrompt();
            }
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
        if (promptQueue.Count == 0)
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
