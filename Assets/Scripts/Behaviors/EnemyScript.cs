using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(CharacterController))]
public class EnemyScript : MonoBehaviour {
	
	private CharacterController mCharacter;
	private EnemyData mEnemy;
	private exSprite mSprite;
	public List<GameObject> mPath;
	private List<Skill> mSelectedSkills;
	
	private Vector3 mMoveVelocity;
	
	private float mHealth;
	private float mEnergy; 
	private float mXViewRange;
	private float mYViewRange; 
	
	private int mCurrentNode;
	private int mNextNode;
	private int mDirection;
	private int mSelectedSkill;
	
	private bool mIsDead; 
	private bool mIsHostile;
	private bool mIsWandering;
	private bool mIsIdle;
	private bool mIsAttacking;
	private bool mIsAirborne;
	private bool[] mIsSkillCooling = new bool[4];
	
	private const float kGravity = 			1700.0f;
	private const float kMinNodeDistance = 	100.0f;
	private const float kNodeWaitTime = 	0.2f;
	
	void Start () {
		mCharacter = 		(CharacterController)GetComponent<CharacterController>();
		mEnemy = 			new EnemyData(1, null);
		mSprite = 			(exSprite)GetComponent<exSprite>();
		mSelectedSkills = 	new List<Skill>();
		Skill icepick = 	new Skill(1,"Icepick", "Cold pick", 1, 10, 50, 500, "", 0, 0, false, 0.8f, 0, "Icepick");
		mSelectedSkills.Add(icepick);
		mSelectedSkill = 	0;
		mCurrentNode = 		0;
		
		if (mPath.Count > 1) {
			mNextNode = 1;
		} else {
			mNextNode = 0;
		}
		
		mMoveVelocity = 	new Vector3(0,0,0);
		mHealth = 			mEnemy.MaxHealth;
		mEnergy = 			mEnemy.MaxEnergy;
		mXViewRange = 		700.0f;
		mYViewRange = 		300.0f;
		mIsDead = 			false;
		mIsHostile = 		false;
		mIsWandering = 		false;
		mIsIdle = 			true;
		mIsAttacking = 		true;
		mIsAirborne = 		true;
		
		// Always set last
		gameObject.AddComponent("EnemyAI");
	}
	
	void Update () {
	
	}
	
	void FixedUpdate () {
		if (!mIsDead)
		{
			CheckGrounded ();
			ApplyGravity ();
			CheckHostile ();
			Move ();
		}
	}
	
	/*
	 * DEAD
	 */
	public bool IsDead ()
	{
		return mIsDead;
	}
	public void DeadAction ()
	{
		//do nothing	
	}
	
	/*
	 * DIE
	 */
	public bool IsDying ()
	{
		return mHealth <= 0;	
	}
	public void AnimateDie ()
	{
		// set die animation
		// start coroutine die
		StartCoroutine("CoroutineDieTime");
	}
	IEnumerator CoroutineDieTime ()
	{
		yield return new WaitForSeconds(1.0f); // Animation die time
		mIsDead = true;
		StopCoroutine("CoroutineDieTime");
	}
	public void DieAction ()
	{
		// some sort of dying action	
	}

	/*
	 * ALIVE
	 */
	public bool IsAlive ()
	{
		return !mIsDead;	
	}
	
	/*
	 * HOSTILE
	 */
	void CheckHostile ()
	{
		
		GameObject player = GameObject.FindGameObjectWithTag("Character");
		mIsHostile = 	((player.transform.position.x - mXViewRange <= transform.position.x && 
						transform.position.x <= player.transform.position.x + mXViewRange) &&
						(player.transform.position.y - mYViewRange <= transform.position.y &&
						transform.position.y <= player.transform.position.y + mYViewRange));
		
	}
	public bool IsHostile ()
	{
		return mIsHostile;	
	}
	
	/*
	 * ATTACK
	 */
	public bool IsSkillSelected ()
	{
		//TODO
		return true;
	}
	
	public bool IsAttackCooling ()
	{
		Debug.Log ("Check Attack cooling: " + mIsSkillCooling[mSelectedSkill]);
		return mIsSkillCooling[mSelectedSkill];	
	}
	
	public void AnimateAttack ()
	{
		//TODO
		StartCoroutine("CoroutineAttack");
	}
	IEnumerator CoroutineAttack ()
	{
		mIsSkillCooling[mSelectedSkill] = true;
		float delay = mSelectedSkills[mSelectedSkill].SkillDelay;
		yield return new WaitForSeconds(delay);
		mIsSkillCooling[mSelectedSkill] = false;
	}
	public void AttackAction ()
	{
		//TODO
	}
	
	/*
	 * SKILL
	 */
	public void SelectSkill ()
	{
		//TODO
		// Depending on the enemies stats, select the best possible skill
	}
	
	/*
	 * WANDER
	 */
	public bool IsWandering ()
	{
		return mIsWandering;	
	}
	public void SetWanderDirection ()
	{
		float dirCheck = transform.position.x - mPath[mNextNode].transform.position.x;
		
		Vector3 scale = gameObject.transform.localScale;
		scale.x = Mathf.Abs(scale.x);
		
		if ( dirCheck >= 0) {
			mDirection = -1;
			scale.x *= -1;
		}
		else {
			mDirection = 1;
			scale.x *= 1;
		}
		
		gameObject.transform.localScale = scale;
	}
	public void AnimateWander ()
	{
	}
	
	public void WanderAction ()
	{
		mIsWandering = true;
		if (Mathf.Abs(Vector3.Distance(transform.position, mPath[mNextNode].transform.position)) < kMinNodeDistance)
		{
			mIsWandering = false;
			if (++mNextNode >= mPath.Count) {
				mNextNode = 0;
				mMoveVelocity.x = 0;
			}
		}
		if (mIsWandering)
			mMoveVelocity.x = mDirection * mEnemy.MaxWanderSpeed;
		
		mCharacter.Move (mMoveVelocity * Time.deltaTime);
	}

	/*
	 * IDLE
	 */
	public bool IsIdle ()
	{
		return mIsIdle;	
	}
	public void AnimateIdle ()
	{	
	}
	IEnumerator CoroutineIdleTime ()
	{
		mIsIdle = true;
		yield return new WaitForSeconds(kNodeWaitTime);
		mIsIdle = false;
	}
	public void IdleInit ()
	{
		StartCoroutine("CoroutineIdleTime");	
	}
	public void IdleAction ()
	{
		mMoveVelocity.x = 0;
	}
	
	private void ApplyGravity ()
	{
		if (mIsAirborne)
		{
			mMoveVelocity.y -= kGravity * Time.deltaTime;
		}
	}
	
	private void CheckGrounded ()
	{
		RaycastHit hit;
		Vector3 down = transform.TransformDirection(-Vector3.up);
		float height = mCharacter.height;

		Vector3 pos = transform.position;

		pos.y -= (height / 2.0f) * transform.localScale.y - 5;
		pos.x -= mCharacter.radius / 5.0f;

		RaycastHit hit2;

		Vector3 pos2 = transform.position;

		pos2.y -= (height / 2.0f) * transform.localScale.y - 5;
		pos2.x += mCharacter.radius/ 5.0f;

        mIsAirborne = (	!Physics.Raycast(pos, down, out hit, mMoveVelocity.y * Time.deltaTime + 10) && 
						!Physics.Raycast(pos2, down, out hit2, mMoveVelocity.y * Time.deltaTime + 10));
		
		Vector3 debugVector = mMoveVelocity;
		debugVector.y = debugVector.y * Time.deltaTime + 10;
		
		Debug.DrawRay(pos, debugVector * Time.deltaTime, Color.red);
		Debug.DrawRay(pos2, debugVector * Time.deltaTime, Color.red);
	}
	
	public void OnControllerColliderHit (ControllerColliderHit hit)
	{
		int terrainLayer = LayerMask.NameToLayer("Terrain");
		
		if (hit.gameObject.layer == terrainLayer && mCharacter.collisionFlags == CollisionFlags.Below)
		{
			mMoveVelocity.y = 0;
			mIsAirborne = false;
		}
		// If player hits a wall, set x velocity to 0.
		if (hit.gameObject.layer == terrainLayer && mCharacter.collisionFlags == CollisionFlags.Sides)
		{
			mMoveVelocity.x = 0;
		}
		// If head hits the top of a terrain, bounce back down
		if (hit.gameObject.layer == terrainLayer && mCharacter.collisionFlags == CollisionFlags.CollidedAbove)
		{
			mMoveVelocity.y = 0;	
		}
	}
	
	void Move()
	{
		//mCharacter.Move (mMoveVelocity * Time.deltaTime);
	}
}
