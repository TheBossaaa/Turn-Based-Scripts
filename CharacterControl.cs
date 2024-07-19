using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterControl : MonoBehaviour
{
    [Header("Define if the character is a player or AI")]
    public bool isPlayer;

    [Header("Class Related - Select the Player Class")]
    public bool isKnight;
    public bool isRanger;
    public bool isMage;
    public bool isPriest;

    [Header("Define the enemy difficulty")]
    public bool easyEnemy;
    public bool mediumEnemy;
    public bool hardEnemy;
    public bool isSideBoss;
    public bool isMainBoss;

    [Header("Define the enemy class")]
    public bool meleeEnemy;
    public bool rangedEnemy;
    public bool magicalEnemy;
    public bool healerEnemy;
    public bool necromancerEnemy; //DEPENDEND ON SPAWN MECHANIC

    public AIBrain aiBrain; //AI Brain

    [Header("Movement Related")]
    public float walkSpeed;
    public float dashSpeed;
    public Vector3 moveTarget;
    public NavMeshAgent navAgent;
    public float moveRange = 3.5f, dashRange = 6f;

    [Header("Combat Related")]
    public float meleeDistance = 1.5f;  //MELEE DISTANCE
    public float rangedDistance = 6f; //RANGED DISTANCE
    public float healingRange = 3f; //HEALING DISTANCE

    public float meleeDamage = 5f;  //MELEE DAMAGE
    public float rangedDamage = 3f;  //RANGED DAMAGE
    public float healingAmount = 3f;  //HEALING AMOUNT

    [Range(0f, 100f)]
    public float hitChancePercentage; //Represents the percentage of a hit chance
    [Range(0f, 100f)]
    public float randomValue; //Random percentage for hit or miss

    public List<CharacterControl> meleeTargets = new List<CharacterControl>();
    public List<CharacterControl> rangedTargets = new List<CharacterControl>();
    public List<CharacterControl> healingTargets = new List<CharacterControl>();

    public int currentMeleeTarget;
    public int currentRangedTarget;
    public int currentHealingTarget;

    public GameObject Arrow;
    public GameObject Fireball;

    public bool isStunned; //Bool for stun
    public ParticleSystem stunParticle; //stun particle

    [Header("Health Related")]
    [Tooltip("Set the appropriate health for the character")]
    public float maxHealth = 10f;
    [Tooltip("DO NOT EDIT")]
    public float currentHealth;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    public Canvas characterHealthCanvas;

    [Header("VFX Related")]
    public ParticleSystem healthParticle;

    [Header("Animation Related")]
    public Animator animator;

    bool isMoving;
    Transform target;

    private void Awake()
    {
        navAgent = this.GetComponent<NavMeshAgent>(); //This is working
        healthText = this.GetComponentInChildren<TextMeshProUGUI>();
        healthSlider = this.GetComponentInChildren<Slider>();
        healthParticle = this.GetComponentInChildren<ParticleSystem>();
        if (!isPlayer)
        {
            foreach(Transform child in transform)
            {
                if(child.tag == "stunParticle")
                {
                    stunParticle = child.GetComponent<ParticleSystem>();
                }
            }
        }
        characterHealthCanvas = this.GetComponentInChildren<Canvas>();

        foreach (var graphic in characterHealthCanvas.GetComponentsInChildren<Graphic>())
        {
            graphic.raycastTarget = false;
        }

        if (!isPlayer)
        {
            aiBrain = this.GetComponentInChildren<AIBrain>();
        }

        currentHealth = maxHealth;
    }

    void Start()
    {
        moveTarget = transform.position;
        navAgent.speed = walkSpeed;

        animator = this.GetComponent<Animator>();
        UpdateHealthDisplay();
    }

    void Update()
    {
        if (isMoving)
        {
            if (GameManager.instance.ActivePlayer == this)
            {
                CameraSystem.instance.SetMoveTarget(transform.position);

                if (Vector3.Distance(transform.position, moveTarget) < .2f)
                {
                    isMoving = false;
                    animator.SetBool("isMoving", false);
                    animator.SetBool("isDashing", false);
                    GameManager.instance.FinishedMovement();
                }
            }

            if (!isPlayer)
            {
                if (Vector3.Distance(transform.position, moveTarget) < .2f)
                {
                    isMoving = false;
                    animator.SetBool("isMoving", false);
                    animator.SetBool("isDashing", false);
                }
            }
        }
    }

    #region Movement Related
    public void MoveToPoint(Vector3 pointToMoveTo)
    {
        Debug.Log("Clicked");
        moveTarget = pointToMoveTo;

        navAgent.SetDestination(moveTarget);
        isMoving = true;

        if (PlayerInputMenu.instance.selectedWalk)
        {
            navAgent.speed = walkSpeed;
            animator.SetBool("isMoving", true);
        }

        if (PlayerInputMenu.instance.selectedDash)
        {
            navAgent.speed = dashSpeed;
            animator.SetBool("isDashing", true);
        }

        if (!isPlayer)
        {
            navAgent.speed = walkSpeed;
            animator.SetBool("isMoving", true);
        }
    }
    #endregion

    #region Targeting Related
    #region Get Melee Targets
    public void GetMeleeTargets() //TO FIND THE MELEE TARGETS AND LOCK ON THEM
    {
        meleeTargets.Clear();

        if (isPlayer)
        {
            foreach (CharacterControl cc in GameManager.instance.ActiveEnemies)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < meleeDistance)
                {
                    meleeTargets.Add(cc);
                }
            }
        }
        else
        {
            foreach (CharacterControl cc in GameManager.instance.PlayerTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < meleeDistance)
                {
                    meleeTargets.Add(cc);
                }
            }
        }

        if (currentMeleeTarget >= meleeTargets.Count) //Updates the targeted character so that the game won't crash in case changes
        {
            currentMeleeTarget = 0;
        }

    }

    #endregion
    #region Get Ranged Targets
    public void GetRangedTargets() //TO FIND THE RANGED TARGETS AND LOCK ON THEM
    {
        rangedTargets.Clear();

        if (isPlayer)
        {
            foreach (CharacterControl cc in GameManager.instance.ActiveEnemies)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < rangedDistance)
                {
                    rangedTargets.Add(cc);
                }
            }
        }
        else
        {
            foreach (CharacterControl cc in GameManager.instance.PlayerTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < rangedDistance)
                {
                    rangedTargets.Add(cc);
                }
            }
        }

        if (currentRangedTarget >= rangedTargets.Count) //Updates the targeted character so that the game won't crash in case changes
        {
            currentRangedTarget = 0;
        }
    }
    #endregion

    #region Get Targets To Heal
    public void GetTargetsToHeal()
    {
        healingTargets.Clear();

        CharacterControl activePlayer = GameManager.instance.ActivePlayer;

        // Get the team of the active player
        List<CharacterControl> activePlayerTeam = !activePlayer.isPlayer ? GameManager.instance.ActiveEnemies : GameManager.instance.PlayerTeam;

        // Iterate through the team members
        foreach (CharacterControl cc in activePlayerTeam)
        {
            if (cc.currentHealth < cc.maxHealth && Vector3.Distance(activePlayer.transform.position, cc.transform.position) < healingRange)
            {
                healingTargets.Add(cc);
            }
        }

        if (currentHealingTarget >= healingTargets.Count) //Updates the targeted character so that the game won't crash in case changes
        {
            currentHealingTarget = 0;
        }
    }
    #endregion

    #endregion

    #region Melee Related
    public void DoMelee(bool stun = false) //DO THE MELEE
    {
        randomValue = Random.Range(0f, 100f);

        if (randomValue <= hitChancePercentage) //If the random value is lower than the hit chance percentage
        {
            Debug.Log("Melee Attack is Successful!");
            GameManager.instance.ActivePlayer.transform.LookAt(meleeTargets[currentMeleeTarget].transform);
            if (stun)
            {
                meleeTargets[currentMeleeTarget].StunCharacter();
            }
            meleeTargets[currentMeleeTarget].TakeDamage(meleeDamage);
        }
        else
        {
            Debug.Log("Melee Attack Missed!");
            GameManager.instance.ActivePlayer.transform.LookAt(meleeTargets[currentMeleeTarget].transform);
            //Show miss indicator
        }
    }

    public void DisableMeleeAnimation() //DISABLE MELEE ANIMATION
    {
        StartCoroutine(MeleeDisableCoroutine());
    }

    public IEnumerator MeleeDisableCoroutine() //COROUTINE FOR MELEE ANIMATION DISABLE
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("isAttacking", false);

        if (isKnight)
        {
            animator.SetBool("toStun", false);
        }
    }

    public void StunCharacter() //STUN THE CHARACTER
    {
        isStunned = true;
        //When the character is stunned, skip it's turn only for once.
        //Play the particle effect until it's the character's turn again.
        //Stop the particle effect when the character is out of stun
        stunParticle.Play();
    }

    #endregion

    #region Ranged Related
    public void ShootTheTarget() //DO A RANGED ATTACK
    {
        randomValue = Random.Range(0f, 100f);

        if (randomValue <= hitChancePercentage)
        {
            Debug.Log("Ranged Attack Successful!");
            GameManager.instance.ActivePlayer.transform.LookAt(rangedTargets[currentRangedTarget].transform);
            CameraSystem.instance.SetMoveTarget(GameManager.instance.ActivePlayer.transform.position);
            StartCoroutine(RangedCameraMovement(1f));
            InstantiateProjectile();
            rangedTargets[currentRangedTarget].TakeDamage(rangedDamage);

        }
        else
        {
            Debug.Log("Ranged Attack Miss");
            CameraSystem.instance.SetMoveTarget(GameManager.instance.ActivePlayer.transform.position);
            GameManager.instance.ActivePlayer.transform.LookAt(rangedTargets[currentRangedTarget].transform);
        }

    }

    public void InstantiateProjectile()
    {
        if(isMage || magicalEnemy)
        {
            //Instantiate fireball and move it towards the target
            Instantiate(Fireball, transform.position, Quaternion.identity);
        }

        if(isRanger || rangedEnemy)
        {
            Instantiate(Arrow, transform.position + new Vector3(0, 0.8f, 0), Quaternion.identity);
        }
    }

    public void DisableRangedAnimation() //DISABLE THE RANGED ATTACK ANIMATION
    {
        StartCoroutine(RangedDisableCoroutine());
    }

    public void DisableRangedSpellAnimation() //DISABLE THE RANGED SPELL ANIMATION
    {
        StartCoroutine(RangedSpellDisableCoroutine());
    }

    public IEnumerator RangedDisableCoroutine() //DISABLE RANGED ANIMATION COROUTINE
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("isRangedAttacking", false);
    }

    public IEnumerator RangedSpellDisableCoroutine() //DISABLE RANGED SPELL ANIMATION COROUTINE
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("isSpellCasting", false);
    }
    #endregion

    #region Healing Related
    public void HealTarget(float amountToHeal) //HEAL DA TARGET
    {
        currentHealth += amountToHeal;

        healthParticle.Play();

        if (currentHealth >= maxHealth) //If the healing more than the max health
        {
            currentHealth = maxHealth; //Set the current health to maximum if its equal or higher than max health
        }

        UpdateHealthDisplay();

    }
    public void DoHeal() //DO THE  HEAL
    {
        GameManager.instance.ActivePlayer.transform.LookAt(healingTargets[currentHealingTarget].transform);

        if (healingTargets[currentHealingTarget].currentHealth < healingTargets[currentHealingTarget].maxHealth)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isHealing", true);
            healingTargets[currentHealingTarget].HealTarget(healingAmount);
        }
        else
        {
            PlayerInputMenu.instance.NoEnemiesInRangeMessage("Health is Full"); //Show the message if no enemies within melee range
        }
    }

    public void DisableHealingAnimation() //DISABLE THE HEALING ANIMATION METHOD
    {
        StartCoroutine(DisableHealingAnimationCoRoutine());
    }

    public IEnumerator DisableHealingAnimationCoRoutine() //DISABLE THE ANIMATION BOOL COROUTINE
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("isHealing", false);
    }

    #endregion

    #region Damage & Health
    #region Take Damage
    public void TakeDamage(float damageToTake)
    {
        currentHealth -= damageToTake; //Decrease health

        animator.SetBool("isDamaged", true);
        UpdateHealthDisplay();

        if (currentHealth <= 0)
        {
            currentHealth = 0; //Set the health to 0

            navAgent.enabled = false; //Disable Nav Mesh Agent
            characterHealthCanvas.enabled = false; //Disable the canvas
            isStunned = false;
            if (stunParticle != null)
            {
                stunParticle.Stop();
            }

            //Play death animation
            animator.SetInteger("isDead", Random.Range(1, 3));
            GameManager.instance.CleanupProcess();

            if (GameManager.instance.ActivePlayer == this && currentHealth <= 0) //If the pplayer dies in their own turn, end turn
            {
                GameManager.instance.EndTurn();
            }
        }
    }
    #endregion
    #region Control Coroutines & Updates
    public void UpdateHealthDisplay() //UPDATES THE HEALTH DISPLAY
    {
        healthText.text = $"HP: {currentHealth}/{maxHealth}";
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        StartCoroutine(CancelTakenDmgAnim()); //SETS THE DAMAGE TAKEN ANIMATION TO FALSE
    }

    public IEnumerator CancelTakenDmgAnim() //CONTROLS THE DAMAGE TAKEN ANIMATION FOR THE ANIMATOR
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("isDamaged", false);
    }

    public IEnumerator WaitCoroutine(float waitSeconds) //Use this if you just want to wait for some time before something
    {
        yield return new WaitForSeconds(waitSeconds);
    }
    #endregion
    #endregion

    #region Camera Control
    public IEnumerator RangedCameraMovement(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);
        if (LevelManager.Instance?.PlayerCanControl ?? true)
        {
            CameraSystem.instance.SetMoveTarget(rangedTargets[currentRangedTarget].transform.position);
        }
    }
    #endregion

}
