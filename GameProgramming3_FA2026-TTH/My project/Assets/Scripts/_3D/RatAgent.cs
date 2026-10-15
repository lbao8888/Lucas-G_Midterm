using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class RatAgent : MonoBehaviour
{
    [SerializeField] private string ratId;
    [SerializeField] private RatQuestConfig config;

    private NavMeshAgent agent;
    private Transform player;
    private float nextWanderTime;

    public string RatId => ratId;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Hide rats before the first rendered frame. They are loaded again when
        // Backyard is entered after the quest has been accepted.
        if (_GameController.instance == null ||
            _GameController.instance.ratQuestState == RatQuestState.NotStarted ||
            _GameController.instance.ratQuestState == RatQuestState.Complete ||
            _GameController.instance.IsRatCaught(ratId))
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (_GameController.instance == null || _GameController.instance.IsRatCaught(ratId) ||
            _GameController.instance.ratQuestState == RatQuestState.NotStarted ||
            _GameController.instance.ratQuestState == RatQuestState.Complete)
        {
            gameObject.SetActive(false);
            return;
        }

        player = FindPlayer();
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        ChooseWanderDestination();
    }

    private void Update()
    {
        if (_GameController.instance == null || _GameController.instance.IsRatCaught(ratId))
        {
            gameObject.SetActive(false);
            return;
        }

        if (player == null)
        {
            player = FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (_GameController.instance.CanCaptureRats && distance <= CaptureDistance() &&
            Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (_GameController.instance.TryCatchRat(ratId))
            {
                gameObject.SetActive(false);
            }

            return;
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        if (distance <= FleeDistance())
        {
            FleeFromPlayer();
        }
        else if (Time.time >= nextWanderTime || (!agent.pathPending && agent.remainingDistance <= 0.5f))
        {
            ChooseWanderDestination();
        }
    }

    public void Configure(string id, RatQuestConfig questConfig)
    {
        ratId = id;
        config = questConfig;
    }

    private Transform FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject == null ? null : playerObject.transform;
    }

    private void ChooseWanderDestination()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        Vector2 circle = Random.insideUnitCircle * WanderRadius();
        Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, WanderRadius(), NavMesh.AllAreas))
        {
            agent.speed = config == null ? 2.2f : config.wanderSpeed;
            agent.SetDestination(hit.position);
        }

        nextWanderTime = Time.time + Random.Range(2f, 5f);
    }

    private void FleeFromPlayer()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Random.onUnitSphere;
            direction.y = 0f;
        }

        Vector3 candidate = transform.position + direction * WanderRadius();
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, WanderRadius(), NavMesh.AllAreas))
        {
            agent.speed = config == null ? 4.5f : config.fleeSpeed;
            agent.SetDestination(hit.position);
        }
    }

    private float CaptureDistance() => config == null ? 2.25f : config.captureDistance;
    private float FleeDistance() => config == null ? 7f : config.fleeDistance;
    private float WanderRadius() => config == null ? 10f : config.wanderRadius;
}
