using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class NPC_Interact : MonoBehaviour
{
    //TODO: Get storyData to `set` the phase of our Yarn Spinner Scripts
    //TODO: Set storyData from Yarn Spinner Scripts
    

    public TextMeshProUGUI promtText;
    public string interactText;
    public bool canInteract;
    public DialogueRunner npcDialogue;
    public NPC_Cam_Rig npc_cam_rig;
    public npcData storyData;

    public void Start()
    {
        //Setting this variable looks a bit `static` for our system design tastes
        //BUT we can practice a bit of deliberate design because we know we're creating
        //a systematized NPC rig & interaction for multiple instances
        npc_cam_rig = transform.GetComponentInChildren<NPC_Cam_Rig>();
        if (npcDialogue != null)
        {
            npcDialogue.onDialogueStart.AddListener(HandleDialogueStarted);
            npcDialogue.onDialogueComplete.AddListener(HandleDialogueCompleted);
        }
    }

    private void OnDestroy()
    {
        if (npcDialogue != null)
        {
            npcDialogue.onDialogueStart.RemoveListener(HandleDialogueStarted);
            npcDialogue.onDialogueComplete.RemoveListener(HandleDialogueCompleted);
        }

        HandleDialogueCompleted();
    }

    private void HandleDialogueStarted()
    {
        _GameController.instance?.setPlayerMovement(true);
    }

    private void HandleDialogueCompleted()
    {
        _GameController.instance?.setPlayerMovement(false);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            setText(interactText);
            canInteract = true;
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            setText("");
            canInteract = false;
        }
    }

    public void setText(string txt)
    {
        if (promtText != null)
        {
            promtText.text = txt;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract && npcDialogue != null && !npcDialogue.IsDialogueRunning)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("Start NPC Dialogue");
                setText("");
                if (npc_cam_rig != null)
                {
                    npc_cam_rig.movePlayer();
                }
                npcDialogue.StartDialogue(storyData.startingNode);
                canInteract = false;
            }
        }
    }

    //This yarn command / method `sets` the $dialogPhase variable in my YarnSpinner script.
    //We use this in our `loadPhase` node to route the YarnSpinner script to it's correct story path.
    [YarnCommand("setPhase")]
    public void setStoryPhase()
    {
        InMemoryVariableStorage vStore = GameObject.FindAnyObjectByType<InMemoryVariableStorage>();
        vStore.SetValue("$dialogPhase", storyData.currentPhase.ToString());
    }

    //This yarn command / method `gets` a string from YarnSpinner and "converts" that to the phase we're on in our npcData scriableObject;
    //It also contains a nice lil' error reporting in case we pass a string that does not correspond to any dialogPhase.
    [YarnCommand("getPhase")]
    public void getStoryPhase(string phase)
    {
        if(phase == "comeback")
        {
            storyData.currentPhase = npcData.dialogPhase.comeback;
        }
        else if(phase == "quest_completed")
        {
            storyData.currentPhase = npcData.dialogPhase.quest_completed;
        }
        else
        {
            Debug.LogError("The string you passed in the Yarn Script does not match any current dialogPhase in the npcData scriptableObject");
        }
    }

    public void OnApplicationQuit()
    {
        storyData.dataReset();
    }

    [YarnCommand("acceptRatQuest")]
    public void AcceptRatQuest()
    {
        _GameController.instance?.AcceptRatQuest();
    }

    [YarnCommand("rewardRatQuest")]
    public void RewardRatQuest()
    {
        _GameController.instance?.TryRewardRatQuest();
    }

    [YarnFunction("ratQuestIsActive")]
    public static bool RatQuestIsActive()
    {
        return _GameController.instance != null && _GameController.instance.ratQuestState == RatQuestState.Active;
    }

    [YarnFunction("ratQuestCanTurnIn")]
    public static bool RatQuestCanTurnIn()
    {
        return _GameController.instance != null && _GameController.instance.CanTurnInRatQuest();
    }

    [YarnFunction("ratQuestIsComplete")]
    public static bool RatQuestIsComplete()
    {
        return _GameController.instance != null && _GameController.instance.ratQuestState == RatQuestState.Complete;
    }

    [YarnFunction("ratsCaught")]
    public static int RatsCaught()
    {
        return _GameController.instance == null ? 0 : _GameController.instance.RatsCaughtCount;
    }

    [YarnFunction("ratTarget")]
    public static int RatTarget()
    {
        return _GameController.instance == null ? 12 : _GameController.instance.RatTargetCount;
    }
}
