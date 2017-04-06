using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Fencer))]
public class OpponentAIController : MonoBehaviour {
    [SerializeField]
    private Fencer player;
    [SerializeField]
    private int reactTime;
    [SerializeField]
    private int lagTolerance;
    [SerializeField]
    private int attackRange;
    [SerializeField]
    private int feedbackFreq;
    private Fencer opponent;

	// Use this for initialization
	void Start () {
        opponent = gameObject.GetComponent<Fencer>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //Acting code
        //opponent.Attack();
	}

    /**
     * Acting!
     */
    public IEnumerator InputAttack(){
        yield return new WaitForSeconds(reactTime*Time.deltaTime);
        for (int c = 0; c < lagTolerance; c++)
        {
            opponent.Attack();
            yield return null;
        }
        StartCoroutine(Evaluate());
    }

    public IEnumerator InputParry(){
        yield return new WaitForSeconds(reactTime*Time.deltaTime);
        for (int c = 0; c < lagTolerance; c++)
        {
            StartCoroutine(opponent.Parry());
            yield return null;
        }
        StartCoroutine(Evaluate());
    }

    public IEnumerator InputMove(int zDir){
        yield return new WaitForSeconds(reactTime*Time.deltaTime);
        for (int c = 0; c < lagTolerance; c++)
        {
            opponent.Move(zDir);
            yield return null;
        }
        StartCoroutine(Evaluate());
    }

    public IEnumerator Evaluate(){
        yield return new WaitForSeconds(feedbackFreq*Time.deltaTime);
        if (InRange(attackRange))
        {
            if (player.state == Fencer.State.STUNNED)
            {
                StartCoroutine(InputAttack());
                yield break;
            }
            int d = RollDice();
            if (d < 80)
            {
                StartCoroutine(InputAttack());
                yield break;
            }
            else if (d < 90)
            {
                StartCoroutine(InputParry());
            }
            //attack, preempt parry
        }
        else
        {
            //advance or hold ground
            if (player.state == Fencer.State.STUNNED)
            {
                StartCoroutine(InputMove(-1));
                yield break;
            }
            int d = RollDice();
            if (d < 50)
            {
                StartCoroutine(InputMove(-1)); //parallel
            }
        }
    }

    /**
     * Sensors!
     */
    public void AlertAttacking(){
        if(InRange(attackRange)){
            StartCoroutine(InputParry());
        }
    }

    public void AlertMoving(int zDir){
        Debug.Log(-1*zDir);
        if (InRange(attackRange))
        {
            int d = RollDice();
            if (d < 50)
            {
                StartCoroutine(InputMove(zDir)); //parallel
            }

            d = RollDice();
            if (d < 80)
            {
                StartCoroutine(InputAttack());
                return;
            }
            else if (d < 90)
            {
                StartCoroutine(InputParry());
            }
            //attack, preempt parry, or back off
        }
        else
        {
            //advance or hold ground
            int d = RollDice();
            if (d < 50)
            {
                StartCoroutine(InputMove(-1)); //parallel
            }
        }
    }

    public void AlertParrying(){
    }

    public void AlertStunned(){
        if (InRange(attackRange))
        {
            StartCoroutine(InputAttack());
        }
        else
        {
            StartCoroutine(InputMove(-1));
        }
    }

    public void AlertDead(){
    }

    public bool InRange(int dist){
        return Physics.Raycast(transform.position, new Vector3(0,0,-1), dist, 1<<LayerMask.NameToLayer("Body"));
    }

    public int RollDice(){
        return Random.Range(0,100);
    }
}
