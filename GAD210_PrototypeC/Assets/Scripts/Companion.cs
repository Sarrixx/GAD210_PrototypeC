using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Companion : MonoBehaviour
{
    public class AIState
    {
        protected readonly Companion instance;

        public AIState(Companion companion)
        {
            instance = companion;
        }

        public virtual void OnStateEnter() { }

        public virtual void OnStateUpdate() { }

        public virtual void OnStateExit() { }
    }

    [System.Serializable]
    public class IdleState : AIState
    {
        public IdleState(Companion companion) : base(companion)
        {
        }

        public override void OnStateEnter()
        {
            if(instance.agent != null)
            {
                instance.agent.isStopped = true;
            }
        }

        public override void OnStateUpdate()
        {
            if (instance.followTarget != null)
            {
                Vector3 position2D = new Vector3(instance.transform.position.x, 0, instance.transform.position.z);
                if (instance.TargetDistance > instance.followDistance)
                {
                    Debug.Log("Transition to follow: " + instance.TargetDistance);
                    if (instance.agent != null)
                    {
                        instance.SetState(instance.followState);
                    }
                }
                if ((instance.followTarget.transform.position - instance.transform.position).z > 1.5f)
                {
                    instance.agent.baseOffset -= Time.deltaTime * 0.5f;
                }
                else if ((instance.followTarget.transform.position - instance.transform.position).z < 1.25f)
                {
                    instance.agent.baseOffset += Time.deltaTime * 0.5f;
                }
                //if (instance.followTarget.transform.position.z - instance.transform.position.z > instance.followDistance)
                //{
                //    instance.agent.baseOffset -= Time.deltaTime * 1.5f;
                //}
                if (instance.FollowTargetWithinViewAngle == false)
                {
                    Vector3 followTargetPosition2D = new Vector3(instance.followTarget.transform.position.x, 0, instance.followTarget.position.z);
                    Vector3 normalisedTargetDir = (followTargetPosition2D - position2D).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(normalisedTargetDir);
                    float step = instance.rotationSpeed * Time.fixedDeltaTime;
                    instance.transform.rotation = Quaternion.RotateTowards(instance.transform.rotation, lookRotation, step);
                }
            }
        }

        public override void OnStateExit()
        {
            if(instance.agent != null)
            {
                instance.agent.isStopped = false;
            }
        }
    }

    [System.Serializable]
    public class FollowState : AIState
    {
        public FollowState(Companion companion) : base(companion)
        {
        }

        public override void OnStateEnter()
        {
            if(instance.agent != null)
            {
                if (instance.followTarget != null)
                {
                    instance.agent.isStopped = false;
                    instance.agent.SetDestination(instance.followTarget.position);
                }
                else
                {
                    instance.SetState(instance.idleState);
                }
            }
        }

        public override void OnStateUpdate()
        {
            if (instance.followTarget != null)
            {
                if (instance.TargetDistance > instance.followDistance * 0.9f)
                {
                    if (instance.agent.destination != instance.followTarget.position)
                    {
                        instance.agent.SetDestination(instance.followTarget.position);
                    }
                }
                else
                {
                    instance.SetState(instance.idleState);
                    Debug.Log("Transition to idle: " + instance.TargetDistance);
                }
                if ((instance.followTarget.transform.position - instance.transform.position).z > 1.5f)
                {
                    instance.agent.baseOffset += Time.deltaTime * 0.5f;
                }
                else if ((instance.followTarget.transform.position - instance.transform.position).z < 1.25f)
                {
                    instance.agent.baseOffset -= Time.deltaTime * 0.5f;
                }
            }
        }

        public override void OnStateExit()
        {
        }
    }

    [SerializeField] private float followDistance = 1.0f;
    [SerializeField] private Transform followTarget;
    [Tooltip("Defines the angle for the camera's line of sight.")]
    [SerializeField][Range(5f, 180f)] private float lineOfSightAngle = 45f;
    [SerializeField] private float rotationSpeed = 5;

    private IdleState idleState;
    private FollowState followState;

    private NavMeshAgent agent;
    private AIState currentState;

    /// <summary>
    /// Returns true if the target is within the view frustrum of the camera.
    /// </summary>
    public bool FollowTargetWithinViewAngle
    {
        get
        {
            if (followTarget != null)
            {
                Vector3 targetDir = followTarget.position - transform.position;
                float targetAngle = Vector3.Angle(targetDir, transform.forward);

                if (targetAngle >= -lineOfSightAngle && targetAngle <= lineOfSightAngle)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
    public float TargetDistance
    {
        get
        {
            if (followTarget != null)
            {
                return Vector3.Distance(transform.position, followTarget.transform.position);
            }
            else
            {
                return 0.0f;
            }
        }
    }

    private void Awake()
    {
        TryGetComponent(out agent);
        idleState = new IdleState(this);
        followState = new FollowState(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetState(idleState);
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState != null)
        {
            currentState.OnStateUpdate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    public void SetState(AIState state)
    {
        if(currentState != null)
        {
            currentState.OnStateExit();
        }
        currentState = state;
        currentState.OnStateEnter();
    }
}