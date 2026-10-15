using StarterAssets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class _GameController : MonoBehaviour
{
    //for singleton classes we want to make sure that this class will be "protected"
    //what that means is that the "data" or variable of this class can be fetched publicly
    //BUT those variables can ONLY by set privated (aka within this class)
    public static _GameController instance { get; private set; }
    public updateCoins updateUI;
    public CoinsUI coinInventory;
    public int currentCoins;

    public GameObject ThirdPersonRig;
    public Transform mainStartingTransform;
    public Vector3 loadPos;
    public Quaternion loadRot;
    public List<string> coinsCollected;

    [Header("Rat Quest")]
    public RatQuestConfig ratQuestConfig;
    public RatQuestState ratQuestState = RatQuestState.NotStarted;
    public List<string> ratsCaught = new List<string>();
    public bool ratRewardClaimed;

    public event Action RatQuestChanged;

    public int RatsCaughtCount => ratsCaught == null ? 0 : ratsCaught.Count;
    public int RatTargetCount => ratQuestConfig == null ? 12 : ratQuestConfig.targetRatCount;
    public bool IsDialogueActive { get; private set; }
    public bool CanCaptureRats => ratQuestState == RatQuestState.Active && !IsDialogueActive;

    public void Awake()
    {
        //this block of logic ensures that there are only ever going to be ONE instance of this class in our project
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            coinsCollected ??= new List<string>();
            ratsCaught ??= new List<string>();
            //this is a build-in `Action` within the SceneManager that requires a listener method
            //that takes two arguments: Scene & LoadSceneMode
            //This `Action` is called whenever a new Scene is loaded
            SceneManager.sceneLoaded += OnSceneLoad;
            DontDestroyOnLoad(this);
        }

    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }
    }


    //this is the receiver method of the sceneLoaded Action
    //it checks for what scene has been loaded using the scene's build index number
    //that number is set in the Build Profile(s) for the project
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 0)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                loadCoins();
                return;
            }

            //the below logic checks to see if data has been loaded into loadPos
            //if loadPos is `blank` then the game spawns the player at the mainStartingTransform
            //if loadPos has data then the game spawns the player at the loadPos & loadRot spot
            if (loadPos == Vector3.zero)
            {
                if (mainStartingTransform == null)
                {
                    GameObject sceneStart = GameObject.Find("mainStart");
                    mainStartingTransform = sceneStart == null ? null : sceneStart.transform;
                }

                //Calls the Instantation method using dynamic arguments for the position and rotation
                if (mainStartingTransform != null)
                {
                    SpawnThirdPersonPrefab(mainStartingTransform.position, mainStartingTransform.rotation);
                }
            }
            else
            {
                SpawnThirdPersonPrefab(loadPos, loadRot);
            }

            loadCoins();
        }
    }

    //a PUBLIC method we can call from outside this class to instantiate our ThirdPersonRig prefab
    //it takes two arguments, the position and rotation of where we want to spawn the ThirdPersonRig prefab
    public void SpawnThirdPersonPrefab(Vector3 pos, Quaternion rot)
    {
        if (ThirdPersonRig != null)
        {
            Instantiate(ThirdPersonRig, pos, rot);
        }
    }

    //this method is called by the trigger of our loading zone(s) when we exit this scene
    //so that when we reload the `3D_demo` scene, we use the loadPos and loadRot data
    public void loadLocationData(Vector3 pos, Quaternion rot)
    {
        loadPos = pos;
        loadRot = rot;
    }

    //this method takes the unique coinID of each prefab instance when it is collected by the player
    //it then invokes the UI to add a coin to the existing count (CURRENTLY NOT WORKING)
    public void coinGot(string coinID)
    {
        if (coinsCollected.Contains(coinID))
        {
            return;
        }

        coinsCollected.Add(coinID);
        currentCoins++;
        updateUI?.Invoke(currentCoins);
        if (coinInventory != null)
        {
            coinInventory.addCoins(currentCoins);
        }
    }

    public void loadCoins()
    {
        _CoinCollect[] allCoinsInScene = FindObjectsByType<_CoinCollect>(FindObjectsSortMode.None);
        if(coinsCollected != null)
        {
            //make our comparison
            foreach(_CoinCollect coin in allCoinsInScene)
            {
                if (coinsCollected.Contains(coin.coinID))
                {
                    Destroy(coin.gameObject);
                }
            }
        }
    }

    //this is a `setter` method for my `coinInvetory` variable
    public void loadCoinUI(CoinsUI ui)
    {
        coinInventory = ui;
        if (coinInventory != null)
        {
            coinInventory.addCoins(currentCoins);
        }
    }

    //this is a method that is called by the StartDialog event in the DialogRunner class
    //we will toggle true/false in the event system to call whether we want to move or not
    public void setPlayerMovement(bool b)
    {
        IsDialogueActive = b;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        ThirdPersonController tpc = player == null ? null : player.GetComponent<ThirdPersonController>();

        if (tpc == null)
        {
            return;
        }

        if (b)
        {
            tpc.enabled = false;
        }
        else
        {
            tpc.enabled = true;
        }
    }

    public void movePlayerOnNPC(Transform newPos)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && newPos != null)
        {
            player.transform.SetPositionAndRotation(newPos.position, newPos.rotation);
        }
    }

    public bool AcceptRatQuest()
    {
        if (ratQuestState != RatQuestState.NotStarted)
        {
            return false;
        }

        ratQuestState = RatQuestState.Active;
        RatQuestChanged?.Invoke();
        return true;
    }

    public bool IsRatCaught(string ratId)
    {
        return !string.IsNullOrEmpty(ratId) && ratsCaught != null && ratsCaught.Contains(ratId);
    }

    public bool TryCatchRat(string ratId)
    {
        if (!CanCaptureRats || string.IsNullOrEmpty(ratId) || IsRatCaught(ratId))
        {
            return false;
        }

        ratsCaught.Add(ratId);
        if (RatsCaughtCount >= RatTargetCount)
        {
            ratQuestState = RatQuestState.ReadyToTurnIn;
        }

        RatQuestChanged?.Invoke();
        return true;
    }

    public bool CanTurnInRatQuest()
    {
        return ratQuestState == RatQuestState.ReadyToTurnIn && RatsCaughtCount >= RatTargetCount;
    }

    public bool TryRewardRatQuest()
    {
        if (!CanTurnInRatQuest() || ratRewardClaimed)
        {
            return false;
        }

        ratRewardClaimed = true;
        ratQuestState = RatQuestState.Complete;
        currentCoins += ratQuestConfig == null ? 100 : ratQuestConfig.rewardCoins;
        updateUI?.Invoke(currentCoins);
        if (coinInventory != null)
        {
            coinInventory.addCoins(currentCoins);
        }

        RatQuestChanged?.Invoke();
        return true;
    }
}

public enum RatQuestState
{
    NotStarted,
    Active,
    ReadyToTurnIn,
    Complete
}

[System.Serializable]
public class updateCoins : UnityEvent<int>
{

}
