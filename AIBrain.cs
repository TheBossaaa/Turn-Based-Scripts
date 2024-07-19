using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [SerializeField] private float waitBeforeActing;
    [SerializeField] private float waitAfterActing;
    [Range(0f, 100f)]
    [SerializeField] private float ignoreShootChance = 20f;
    [SerializeField] private float ignoreHealChance = 20f;

    private CharacterControl charCon;

    private void Awake()
    {
        charCon = this.GetComponent<CharacterControl>();
    }

    public void ChooseAction()
    {
        StartCoroutine(ChooseCo());
    }

    public IEnumerator ChooseCo() //Make Choices Heres
    {
        Debug.Log(name + " is choosing an action");

        yield return new WaitForSeconds(waitBeforeActing);

        bool actionTaken = false;

        charCon.GetMeleeTargets(); //Getting the melee targets

        if(charCon.meleeTargets.Count > 0) //IF THERE ARE ENEMIES WITHIN MELEE RANGE
        {
            Debug.Log(name + "Chosen Melee");

            charCon.currentMeleeTarget = Random.Range(0, charCon.meleeTargets.Count);

            GameManager.instance.currentActionCost = 1;

            StartCoroutine(WaitToEndAction(waitAfterActing));

            charCon.animator.SetBool("isAttacking", true);
            charCon.DoMelee();
            yield return new WaitForSeconds(1.5f);
            charCon.animator.SetBool("isAttacking", false);
            actionTaken = true;
        }

        if(!charCon.isPlayer && charCon.rangedEnemy || !charCon.isPlayer && charCon.magicalEnemy) //IF THE ENEMY IS RANGED & MAGICAL
        {
            charCon.GetRangedTargets();
            if(actionTaken == false && charCon.rangedTargets.Count > 0)
            {
                if(Random.Range(0f, 100f) > ignoreShootChance)
                {
                    Debug.Log(name + "Chosen Ranged");

                    charCon.currentRangedTarget = Random.Range(0, charCon.rangedTargets.Count);

                    GameManager.instance.currentActionCost = 1;

                    StartCoroutine(WaitToEndAction(waitAfterActing));

                    if (charCon.rangedEnemy)
                    {
                        charCon.animator.SetBool("isRangedAttacking", true);
                        charCon.ShootTheTarget();
                        yield return new WaitForSeconds(1.5f);

                        charCon.animator.SetBool("isRangedAttacking", false);
                        actionTaken = true;
                    }

                    if (charCon.magicalEnemy)
                    {
                        charCon.animator.SetBool("isSpellCasting", true);
                        charCon.ShootTheTarget();
                        yield return new WaitForSeconds(1.5f);

                        charCon.animator.SetBool("isSpellCasting", false);
                        actionTaken = true;
                    }

                }
            }
        }

        if(!charCon.isPlayer && charCon.healerEnemy) //IF THE ENEMY IS A HEALER
        {
            charCon.GetTargetsToHeal();
            charCon.currentHealingTarget = Random.Range(0, charCon.healingTargets.Count);
            if (charCon.healingTargets.Count > 0 && charCon.healingTargets[charCon.currentHealingTarget].currentHealth 
                < charCon.healingTargets[charCon.currentHealingTarget].maxHealth)
            {

                if (actionTaken == false)
                {
                    if (Random.Range(0f, 100f) > ignoreHealChance)
                    {
                        Debug.Log(name + "Choose to heal");

                        GameManager.instance.ActivePlayer.transform.LookAt(charCon.healingTargets[charCon.currentHealingTarget].transform);

                        GameManager.instance.currentActionCost = 1;

                        StartCoroutine(WaitToEndAction(waitAfterActing));

                        charCon.animator.SetBool("isHealing", true);
                        charCon.healingTargets[charCon.currentHealingTarget].HealTarget(charCon.healingAmount);
                        yield return new WaitForSeconds(1.5f);

                        charCon.animator.SetBool("isHealing", false);
                        actionTaken = true;

                    }
                }
            }
           
        }

        if (!actionTaken) //IF NO ACTION TAKEN
        {
            var potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.moveRange, transform.position); //Get the move points in range

            if (potentialMovePoints.Count > 0)
            {
                int nearestPlayer = 0;
                for (int i = 1; i < GameManager.instance.PlayerTeam.Count; i++) //Find the nearest player character
                {
                    if (Vector3.Distance(transform.position, GameManager.instance.PlayerTeam[nearestPlayer].transform.position)
                       > Vector3.Distance(transform.position, GameManager.instance.PlayerTeam[i].transform.position))
                    {
                        nearestPlayer = i;
                    }
                }

                int selectedPoint = 0; //Select one
                float closestDistance = 1000f; //Never gonna need this far but just to safe keeping
                for (int i = 0; i < potentialMovePoints.Count; i++)
                {
                    if (Vector3.Distance(GameManager.instance.PlayerTeam[nearestPlayer].transform.position, potentialMovePoints[i]) < closestDistance)
                    {
                        closestDistance = Vector3.Distance(GameManager.instance.PlayerTeam[nearestPlayer].transform.position, potentialMovePoints[i]);
                        selectedPoint = i;
                    }
                }

                GameManager.instance.currentActionCost = 1; //ACTION COST FOR WALKING

                charCon.MoveToPoint(potentialMovePoints[selectedPoint]); //MOVE TO THAT POINT

                actionTaken = true; //ACTION TAKEN
            }

            if (!actionTaken)
            {
                //skip turn
                GameManager.instance.EndTurn();

                Debug.Log(name + "Skipped Turn");
            }
        }
    }


    IEnumerator WaitToEndAction(float timeToWait)
    {
        Debug.Log("Waiting to end action");
        yield return new WaitForSeconds(timeToWait);
        GameManager.instance.SpendTurnPoints();
    }

}
