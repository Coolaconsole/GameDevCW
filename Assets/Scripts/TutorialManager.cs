using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Unity.Burst.Intrinsics.X86.Avx;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public bool enable = true;
    [SerializeField] private bool promptsPauseGame = false;
    [SerializeField] private bool eventBasedPromptCompletion = false;

    private Dictionary<string, (string, Vector3, UnityEvent)> tutorialPrompts = new Dictionary<string, (string, Vector3, UnityEvent)>();
    private HashSet<string> completedEvents = new HashSet<string>();
    
    [Header("Tutorial UI")]
    List<(string key, string text, Vector3 pos)> promptQueue = new List<(string, string, Vector3)>();
    public GameObject promptObject;

    public float charsPerSecond = 25f;
    public float punctuationPause = 0.25f;

    private Coroutine typingRoutine;
    private TextMeshProUGUI tmp;
    private string currentText = "";
    private bool typingComplete = false;
    
    private string currentPromptKey = "";
    
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
        // Starting Prompts
        tutorialPrompts["welcome"] = ("Welcome, hero!\nYour objective is to protect your base at the top of the map.", new Vector3(0, 0, 0), null);
        tutorialPrompts["move"] = ("Move around with <b>WASD</b>.", new Vector3(-300, -50, 0), onPlayerMoved);
        tutorialPrompts["attack"] = ("Attack enemies with <b>SPACE</b> or <b>Left Click</b>.", new Vector3(-300, -50, 0), onPlayerAttack);
        tutorialPrompts["defend"] = ("You can place <b>towers</b> on the map to protect your base.\nBut we can't afford them yet.", new Vector3(-300, -50, 0), onMoneyPickup);
        
        Instance.QueuePrompt("welcome");
        Instance.QueuePrompt("move");
        Instance.QueuePrompt("attack");
        Instance.QueuePrompt("defend");

        // Wave 1 prompts
        tutorialPrompts["wave1"] = ("Wave 1 is starting!\n<color=red>Enemies</color> are coming from the <color=yellow>yellow path</color>.", new Vector3(-200, 100, 0), onEnemyDeath);
        tutorialPrompts["pickupMoney"] = ("Nice! The enemies have dropped gold for us to buy a <b>tower</b> Pick up the money!", new Vector3(-300, -50, 0), onMoneyPickup);
        tutorialPrompts["firstTower"] = ("Press <b>1</b> to select the first tower, and <b>SPACE</b> to place it.", new Vector3(-300, -50, 0), onTowerPlaced);
        // Wave 2 prompts
        tutorialPrompts["towerExplanation"] = ("Pressing <b>1-4</b> will display the tower's stats.\nTry to think what situations each would be useful in.", new Vector3(-300, -50, 0), null);
        // Wave 3 prompts
        tutorialPrompts["newPath"] = ("Aha, the enemies are making a new <color=yellow>path</color>!\nYou can move your tower by going up to it and pressing <b>E</b>.", new Vector3(0, -200, 0), onTowerPickup);
        tutorialPrompts["moveTower"] = ("Place back down with <b>Q</b>.", new Vector3(0, -200, 0), onTowerDrop);
        // Wave 4 prompts
        tutorialPrompts["newEnemy"] = ("Watch out! A new type of <color=red>enemy</color> has appeared!\nThese ones are <b>fast</b>, but they don't have much <b>health</b>.", new Vector3(-200, 100, 0), null);
        // Wave 5 prompts
        tutorialPrompts["pathColour"] = ("Take note of where enemies <color=red>die</color>.\nIf enough enemies die on the path, it might <b>split</b>.", new Vector3(-200, 100, 0), null);

        // Wave 7 prompts
        tutorialPrompts["checkIn"] = ("You're doing great so far!\nDon't forget you can move towers with <b>E</b> and <b>Q</b>", new Vector3(-200, 100, 0), null);

        // Wave 10 prompts
        tutorialPrompts["bossEnemy"] = ("A <color=red>boss enemy</color> is approaching!", new Vector3(-200, 100, 0), null);
        tutorialPrompts["bossPrep"] = ("Make sure you're prepared.\nDon't let it too close to your <b>base!</b>", new Vector3(-200, 100, 0), null);
        tutorialPrompts["flyingEnemy"] = ("Well done! Though it's not over just yet.\nIt seems new <color=red>flying enemies</color> can only be hit by certain towers.", new Vector3(-200, 100, 0), null);
    }

    private void Update()
    {
        if (promptObject.activeSelf && Time.timeScale != 0)
        {
            Time.timeScale = promptsPauseGame ? 0 : 1;
        }

        if (promptObject.activeSelf && (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)))
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

        (string key, string text, Vector3 pos) = promptQueue[0];
        promptQueue.RemoveAt(0);

        currentPromptKey = key;
        promptObject.SetActive(true);
        var rect = promptObject.GetComponent<RectTransform>();
        tmp = promptObject.GetComponentInChildren<TextMeshProUGUI>();
        rect.anchoredPosition3D = pos;

        StartTyping(text);
    }

    public void ClosePrompt()
    {
        if (promptObject.activeSelf)
        {
            promptObject.SetActive(false);
            currentPromptKey = "";
        }

        Time.timeScale = promptsPauseGame ? 1 : 0; //One line if statement
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
            
            var (text, pos, unityEvent) = tutorialPrompts[key];
            promptQueue.Add((key, text, pos));

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
