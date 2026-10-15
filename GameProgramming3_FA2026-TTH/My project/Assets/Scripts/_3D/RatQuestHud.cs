using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RatQuestHud : MonoBehaviour
{
    private TextMeshProUGUI label;
    private _GameController controller;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        BindController();
        Refresh();
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.RatQuestChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (controller == null)
        {
            BindController();
            Refresh();
        }
    }

    private void BindController()
    {
        if (_GameController.instance == null || controller == _GameController.instance)
        {
            return;
        }

        if (controller != null)
        {
            controller.RatQuestChanged -= Refresh;
        }

        controller = _GameController.instance;
        controller.RatQuestChanged += Refresh;
    }

    private void Refresh()
    {
        if (label == null || controller == null)
        {
            return;
        }

        switch (controller.ratQuestState)
        {
            case RatQuestState.NotStarted:
                label.text = "Go inside and check on the homeowner.";
                break;
            case RatQuestState.Active:
                label.text = $"Rats caught: {controller.RatsCaughtCount} / {controller.RatTargetCount}\nGet close and press E to catch.";
                break;
            case RatQuestState.ReadyToTurnIn:
                label.text = $"Rats caught: {controller.RatsCaughtCount} / {controller.RatTargetCount}\nAll caught! Return to the homeowner.";
                break;
            default:
                label.text = "Rat job complete. Reward received.";
                break;
        }
    }
}
