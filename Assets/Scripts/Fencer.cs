using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class Fencer : MonoBehaviour {

    //tunable parameters
    public string name;
    [SerializeField]
    private int attackSpeed;
    [SerializeField]
    private int parrySpeed;
    [SerializeField]
    private int[] stunTimes; //[MoveStun, AttackStun, ParryStun]
    [SerializeField]
    private int invincibleStart;
    [SerializeField]
    private int invincibleLength;
    [SerializeField]
    private float stepSize;
    [SerializeField]
    private int stepSpeed;
    [SerializeField]
    private float bounce;
    [SerializeField]
    private Color attackColor;
    [SerializeField]
    private Color parryColor;
    [SerializeField]
    private Color staggerColor;
    [SerializeField]
    private Color dieColor;
    [SerializeField]
    private OpponentAIController opponentAI; //player must attach OPPONENT's controller to broadcast.

    //attachments
    Rigidbody body;
    Animator animate;

    //game states
    private int canAct;
    private int canMove;
    [HideInInspector] public enum State{READY, ATTACKING, PARRYING, INVINCIBLE, STUNNED, DEAD};
    public State state; //mostly for external use
    [HideInInspector] public int[] stunCounter;

	// Use this for initialization
	void Start () {
        body = gameObject.GetComponent<Rigidbody>();
        animate = gameObject.GetComponent<Animator>();
        canMove = 0;
        canAct = 0;
        state = State.READY;
        stunCounter = new int[3];
        for (int i=0; i < stunCounter.Length; i++)
        {
            stunCounter[i] = 0;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(canMove > 0)
            canMove--;

        for(int i=0; i<stunCounter.Length; i++)
        {
            if (stunCounter[i] > 0)
            {
                stunCounter[i]--;
            }
        }

        if (canAct > 0)
            canAct--;
        else if (state != State.DEAD && stunCounter[1] == 0){
            state = State.READY;
        }
	}

    void OnTriggerEnter(Collider other){
        if (state == State.DEAD)
        {
            return;
        }

        Fencer o = other.gameObject.GetComponentInParent<Fencer>();

        if (other.gameObject.tag == "Sword" && state != State.PARRYING && state != State.INVINCIBLE)
        {
            Stagger();
        }
        else if (other.gameObject.tag == "Body" && state != State.STUNNED && o.state != State.INVINCIBLE)
        {
            if (o.state == State.INVINCIBLE){
                Stagger();
            }
            else{
                o.Die();
                StartCoroutine(Engine.singleton.Reset(name));
            }
        }
    }

    public void Attack() {
        if (canAct != 0){
            return;
        }
        if (state == State.DEAD) {
            return;
        }
        if (stunCounter[1] != 0) {
            return;
        }

        animate.SetTrigger("attack");
        canAct = attackSpeed;
        state = State.ATTACKING;
        if (opponentAI != null)
        {
            opponentAI.AlertAttacking();
        }
        StartCoroutine(ChangeColor(Color.white, attackColor, attackSpeed));
    }

    public IEnumerator Parry() {
        if (canAct != 0) {
            yield break;
        }
        if (state == State.DEAD) {
            yield break;
        }
        if (invincibleStart + invincibleLength >= parrySpeed) {
            yield break;
        }
        if (stunCounter[2] != 0) {
            yield break;
        }

        //start lag
        animate.SetTrigger("parry");
        canAct = parrySpeed;
        state = State.PARRYING;
        if (opponentAI != null)
        {
            opponentAI.AlertParrying();
        }
        StartCoroutine(ChangeColor(Color.white, parryColor, parrySpeed));
        yield return new WaitForSeconds(invincibleStart*Time.deltaTime);

        //invincibility kicks in
        state = State.INVINCIBLE;
        yield return new WaitForSeconds(invincibleLength * Time.deltaTime);

        //end lag
        state = State.PARRYING;
    }

    //take one discrete step along the z axis.
    public void Move(int zDir) {
        if (canMove != 0){
            return;
        }
        if (state == State.DEAD) {
            return;
        }
        if (stunCounter[0] != 0) {
            return;
        }

        body.velocity = new Vector3(0, bounce, stepSize*zDir);
        if (opponentAI != null)
        {
            opponentAI.AlertMoving(zDir);
        }
        canMove = stepSpeed;
    }

    public void Die() {
        if (state == State.DEAD) {
            return;
        }

        state = State.DEAD;
        if (opponentAI != null)
        {
            opponentAI.AlertDead();
        }
        body.constraints = 0;
        StartCoroutine(ChangeColor(dieColor, dieColor));
    }

    public IEnumerator ChangeColor(Color cPrev, Color cNext, int duration = 0){
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends){
            r.material.color = cNext;
        }

        yield return new WaitForSeconds(duration * Time.deltaTime);

        //turn back to original color
        if (state != State.DEAD)
        {
            foreach (Renderer r in rends)
            {
                if (state != State.STUNNED)
                    r.material.color = cPrev;
                else
                    r.material.color = staggerColor;
            }
        }
    }

    public void Stagger(){
        if (state == State.DEAD)
            return;

        animate.SetTrigger("stagger");
        Debug.Log("stunned!", gameObject);
        state = State.STUNNED;
        if (opponentAI != null)
        {
            opponentAI.AlertStunned();
        }
        for(int i=0; i<stunTimes.Length; i++)
        {
            stunCounter[i] = stunTimes[i];
        }
        StartCoroutine(ChangeColor(Color.white,staggerColor,stunTimes[1]));
    }
}
