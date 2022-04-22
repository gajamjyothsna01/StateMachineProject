using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameStatement : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public Transform player;
    State currentState;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = new Idle(this.gameObject,agent,animator,player);

    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
        
    }
}
public class State
{
    public enum STATE {IDLE, PATROL, ATTACK, CHASE, DEATH }
    public enum EVENTS { ENTER , UPDATE, EXIT}

    public STATE stateName;
    public EVENTS eventsState;
    public GameObject npc;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform playerPosition;
    public State nextState;
    float visualDistance = 10f, visualAngle=30f, shootingDistance=5f;

    public State(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition)
    {
        this.npc = _npc;
        this.playerPosition = _playerPosition;
        this.agent = _agent;
        this.animator = _animator;
        eventsState = EVENTS.ENTER;

    }

    public virtual void EnterMethod()
    {
        eventsState = EVENTS.UPDATE;
    }

    public virtual void UpdateMethod()
    {
        eventsState = EVENTS.UPDATE;
    }

    public virtual void ExitMethod()
    {
        eventsState = EVENTS.EXIT;
    }
    public State Process()
    {
        if(eventsState == EVENTS.ENTER)
        {
            EnterMethod();

        }
        if (eventsState == EVENTS.UPDATE)
        {
            UpdateMethod();

        }
        if (eventsState == EVENTS.EXIT)
        {
            ExitMethod();
            return nextState;

        }
        return this;
    }
    public bool CanSeePlayer()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        if(direction.magnitude < visualDistance && direction.magnitude < visualAngle)
        {
            return true;
        }
        return false;
    }

    public bool EnemyCanAttackPlayer()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        if(direction.magnitude <  shootingDistance)
        {
            return true;
        }
        return false;
    }
}
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition ): base(_npc, _agent, _animator,_playerPosition)
    {
        stateName = STATE.IDLE;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isIdle");
        base.EnterMethod(); 
    }
    public override void UpdateMethod()
    {
        if(CanSeePlayer())
        {
            nextState = new Chase(npc,agent,animator, playerPosition);
            eventsState = EVENTS.EXIT;
        }
        if(Random.Range(0,100) < 10f)
        {
            nextState = new Patrol(npc,agent, animator, playerPosition );
            eventsState = EVENTS.EXIT;
        }
      // base.UpdateMethod();
    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isIdle");

        base.ExitMethod();
    }
}
public class Patrol : State
{
    int currentIndex=- 1;
    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isWalking");
        //currentIndex = 0;
        base.EnterMethod();
    }
    public override void UpdateMethod()
    {
        if(agent.remainingDistance < 1)
        {
            if(currentIndex >= GameController.Instance.CheckPoints.Count)
            {
                currentIndex = 0;

            }
            else
            {
                currentIndex++;
            }
            agent.SetDestination(GameController.Instance.CheckPoints[currentIndex].transform.position);

        }
        //base.UpdateMethod();
    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isWalking");

        base.ExitMethod();
    }

}
public class Chase : State
{
    public Chase(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.CHASE;
        agent.speed = 5f;
        agent.isStopped = false;

    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isRunning");
        //currentIndex = 0;
        base.EnterMethod();
    }
    public override void UpdateMethod()
    {
        agent.SetDestination(playerPosition.position);
        if(agent.hasPath)
        {
            if(EnemyCanAttackPlayer())
            {
                nextState = new Attack(npc, agent, animator, playerPosition);
                eventsState = EVENTS.EXIT;
            }
            else if(!CanSeePlayer())
            {
                nextState = new Patrol(npc, agent, animator, playerPosition);
                eventsState= EVENTS.EXIT; 
            }

        }
        
    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isRunning");

        base.ExitMethod();
    }

}
public class Attack : State
{
    float roatationSpeed = 5;
    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName =STATE.ATTACK;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isShooting");
        agent.isStopped = true;
        //currentIndex = 0;
        base.EnterMethod();
    }
    public override void UpdateMethod()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        float angle = Vector3.Angle(direction,npc.transform.forward);
        direction.y = 0;
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), roatationSpeed * Time.deltaTime);
        if(!EnemyCanAttackPlayer())
        {
            nextState = new Idle(npc, agent, animator, playerPosition);
            eventsState = EVENTS.EXIT;
        }
        if(!CanSeePlayer())
        {
            nextState = new Death(npc, agent, animator, playerPosition);
            eventsState = EVENTS.EXIT;
        }

        if(EnemyCanAttackPlayer())
        {
            nextState = new Death(npc, agent, animator, playerPosition);
            eventsState = EVENTS.EXIT;

        }

    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isShooting");
        nextState = new Death(npc, agent, animator, playerPosition);

        base.ExitMethod();
    }

}
public class Death : State
{
    public Death(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.DEATH;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isSleeping");
       
        //currentIndex = 0;
        base.EnterMethod();
    }
    public override void UpdateMethod()
    {
        



    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isSleeping");
       // nextState = new Death(npc, agent, animator, playerPosition);

        base.ExitMethod();
    }
}