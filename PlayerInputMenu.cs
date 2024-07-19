using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerInputMenu : MonoBehaviour
{

    public static PlayerInputMenu instance;

    private void Awake()
    {
        instance = this;
    }

    #region Class Related Variables
    [Header("Class: Knight Menus")]
    public GameObject knightClassPanel;
    public GameObject knightMainActionMenu;
    public GameObject knightMeleeMenu;

    [Header("Class: Ranger Menus")]
    public GameObject rangerClassPanel;
    public GameObject rangerMainActionMenu;
    public GameObject rangerMeleeMenu;
    public GameObject rangerRangedMenu;

    [Header("Class: FireMage Menus")]
    public GameObject fireMageClassPanel;
    public GameObject fireMageMainActionsMenu;
    public GameObject fireMageMeleeMenu;
    public GameObject fireMageSpellsMenu;

    [Header("Class: Priest Menus")]
    public GameObject priestClassPanel;
    public GameObject priestMainActionsMenu;
    public GameObject priestMeleeMenu;
    public GameObject priestHealingMenu;

    #endregion

    #region UI Related Variables
    [Header("UI & HUD Elements")]
    [Tooltip("Assign the appropriate UI Elements")]
    public GameObject noTargetsFoundParent;
    public GameObject turnPointParent;
    public TextMeshProUGUI turnPointText;
    public TextMeshProUGUI noEnemyFoundText;
    public float errorDisplayTime;
    public float errorCounter;

    [Header("Bools for Various controls")]
    [Tooltip("Animation bool controls")]
    public bool selectedWalk, selectedDash;

    [Header("Fade Variables")] //FOR FADING OUT THE NO ENEMIES FOUND MESSAGE
    bool isFade;
    float startFade = 50;
    float startTextFade = 255;
    float endFade = 0;
    float speed = 40;

    #endregion

    public CharacterControl characterControl;

    private void Start()
    {
        
    }

    #region Class Panels

    public void ShowClassPanel()
    {

        characterControl = GameManager.instance.ActivePlayer;

        turnPointParent.SetActive(true);
        turnPointText.gameObject.SetActive(true); //Display the remaining turn points

        if (characterControl.isKnight)
        {
            knightClassPanel.SetActive(true);
            rangerClassPanel.SetActive(false);
            priestClassPanel.SetActive(false);
            fireMageClassPanel.SetActive(false);

            //Fill the Action Related Menus according to the class
            //Set the frame according to the class

        }

        if (characterControl.isRanger)
        {
            knightClassPanel.SetActive(false);
            rangerClassPanel.SetActive(true);
            priestClassPanel.SetActive(false);
            fireMageClassPanel.SetActive(false);

            //Fill the Action Related Menus according to the class
            //Set the frame according to the class
        }

        if (characterControl.isMage)
        {
            knightClassPanel.SetActive(false);
            rangerClassPanel.SetActive(false);
            priestClassPanel.SetActive(false);
            fireMageClassPanel.SetActive(true);

            //Fill the Action Related Menus according to the class
            //Set the frame according to the class
        }

        if (characterControl.isPriest)
        {
            knightClassPanel.SetActive(false);
            rangerClassPanel.SetActive(false);
            priestClassPanel.SetActive(true);
            fireMageClassPanel.SetActive(false);

            //Fill the Action Related Menus according to the class
            //Set the frame according to the class
        }

    }

    public void HideClassPanel()
    {

        characterControl = GameManager.instance.ActivePlayer;
        turnPointParent.SetActive(false);
        turnPointText.gameObject.SetActive(false); //Display the remaining turn points

        knightClassPanel.SetActive(false);
        rangerClassPanel.SetActive(false);
        priestClassPanel.SetActive(false);
        fireMageClassPanel.SetActive(false);

    }


    #endregion

    #region Knight Class
    #region Knight Class Menu Related
    public void ShowKnightMeleeMenu() //Show Knight Attack Menu
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        knightMainActionMenu.SetActive(false);
        knightMeleeMenu.SetActive(true);
    }

    public void HideKnightMeleeMenu() //Hide Knight Attack Menu
    {
        knightMeleeMenu.SetActive(false);
        knightMainActionMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }
    #endregion
    #region Knight Class Melee Related
    
    public void KnightCheckMelee()
    {
        GameManager.instance.ActivePlayer.GetMeleeTargets();

        if (GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.meleeTargets
                [GameManager.instance.ActivePlayer.currentMeleeTarget].transform.position;

            GameManager.instance.TargetDisplay.SetActive(true);

        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range"); //Show the message if no enemies within melee range
        }
    }

    public void KnightMeleeHit() //MELEE HIT FOR THE KNIGHT
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isAttacking", true);
        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range");
        }

        GameManager.instance.ActivePlayer.DisableMeleeAnimation();
        GameManager.instance.ActivePlayer.DoMelee();

        HideKnightMeleeMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }

    //IMPLEMENT STUN SKILL

    public void KnightStunHit() //KNIGHT STUN SKILL
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("toStun", true);
        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range");
        }

        GameManager.instance.ActivePlayer.DisableMeleeAnimation();
        GameManager.instance.ActivePlayer.DoMelee(true); //Doing the melee but it's not stunning the target

        HideKnightMeleeMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }



    #endregion


    #endregion

    #region Ranger Class
    #region Ranger Class Menu Related
    public void ShowRangeMeleeMenu()
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        rangerMainActionMenu.SetActive(false);
        rangerMeleeMenu.SetActive(true);
    }

    public void HideRangerMeleeMenu()
    {
        rangerMeleeMenu.SetActive(false);
        rangerMainActionMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }

    public void ShowRangerRangedMenu()
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        rangerMainActionMenu.SetActive(false);
        rangerRangedMenu.SetActive(true);
    }

    public void HideRangerRangedMenu()
    {
        rangerRangedMenu.SetActive(false);
        rangerMainActionMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }
    #endregion
    #region Ranger Melee Related
    public void RangerMeleeCheck()
    {
        GameManager.instance.TargetDisplay.SetActive(true);
        GameManager.instance.ActivePlayer.GetMeleeTargets();

        if (GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {

            GameManager.instance.TargetDisplay.SetActive(true);
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.meleeTargets
                [GameManager.instance.ActivePlayer.currentMeleeTarget].transform.position;

        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range"); //Show the message if no enemies within melee range
        }
    }

    public void RangerMeleeHit() //MELEE HIT FOR THE KNIGHT
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isAttacking", true);
        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range");
        }

        GameManager.instance.ActivePlayer.DisableMeleeAnimation();
        GameManager.instance.ActivePlayer.DoMelee();

        HideRangerMeleeMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }

    #endregion
    #region Ranger Ranged Attack Related

    public void RangerRangedCheck()
    {
        GameManager.instance.ActivePlayer.GetRangedTargets();

        if (GameManager.instance.ActivePlayer.rangedTargets.Count > 0)
        {
            CameraSystem.instance.SetMoveTarget(characterControl.rangedTargets[characterControl.currentRangedTarget].transform.position); //Move the camera to the target
            GameManager.instance.TargetDisplay.SetActive(true);
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.rangedTargets
                [GameManager.instance.ActivePlayer.currentRangedTarget].transform.position;

        }
        else
        {

            NoEnemiesInRangeMessage("No Enemies within Range"); //Show the message if no enemies within melee range
        }
    }

    public void RangerRangedAttack() //MELEE HIT FOR THE RANGER
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;
        
        if(GameManager.instance.ActivePlayer.rangedTargets.Count > 0)
        {
            Vector3 targetPosition = characterControl.rangedTargets[characterControl.currentRangedTarget].transform.position;

            GameManager.instance.ActivePlayer.animator.SetBool("isRangedAttacking", true);
            
            //GameObject missile = Instantiate(characterControl.Arrow, GameManager.instance.activePlayer.transform.position, transform.rotation.normalized);
            //missile.transform.Translate(targetPosition * 20f * Time.deltaTime);
            
            /*
            if (Vector3.Distance(missile.transform.position,
                characterControl.rangedTargets[characterControl.currentRangedTarget].transform.position) < 0.5f)
            {
                Destroy(missile);
            }
            */
        }

        GameManager.instance.ActivePlayer.DisableRangedAnimation();
        GameManager.instance.ActivePlayer.ShootTheTarget();

        HideRangerRangedMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));


    }


    #endregion



    #endregion

    #region Fire Mage Class
    #region Fire Mage Class Menu Related
    public void ShowFireMageMeleeMenu()
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        fireMageMeleeMenu.SetActive(true);
        fireMageMainActionsMenu.SetActive(false);
    }

    public void HideFireMageMeleeMenu()
    {
        fireMageMeleeMenu.SetActive(false);
        fireMageMainActionsMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }

    public void ShowFireMageSpellsMenu()
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        fireMageMainActionsMenu.SetActive(false);
        fireMageSpellsMenu.SetActive(true);
    }

    public void HideFireMageSpellsMenu()
    {
        fireMageSpellsMenu.SetActive(false);
        fireMageMainActionsMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }

    #endregion
    #region Fire Mage Melee Related

    public void FireMageMeleeCheck()
    {
        GameManager.instance.ActivePlayer.GetMeleeTargets();

        if (GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.meleeTargets
                [GameManager.instance.ActivePlayer.currentMeleeTarget].transform.position;
            GameManager.instance.TargetDisplay.SetActive(true);

        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range"); //Show the message if no enemies within melee range
        }
    }

    public void FireMageMeleeHit() //MELEE HIT FOR THE KNIGHT
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isAttacking", true);
        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies within Range"); //Show the message if no enemies within melee range
        }

        GameManager.instance.ActivePlayer.DisableMeleeAnimation();
        GameManager.instance.ActivePlayer.DoMelee();

        HideFireMageMeleeMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }


    #endregion
    #region Fire Mage Spells Related

    public void FireMageRangedCheck()
    {
        GameManager.instance.ActivePlayer.GetRangedTargets();

        if (GameManager.instance.ActivePlayer.rangedTargets.Count > 0)
        {
            CameraSystem.instance.SetMoveTarget(characterControl.rangedTargets[characterControl.currentRangedTarget].transform.position); //Move the camera to the target
            GameManager.instance.TargetDisplay.SetActive(true);
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.rangedTargets
                [GameManager.instance.ActivePlayer.currentRangedTarget].transform.position;

        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies within Range"); //Show the message if no enemies within melee range
        }
    }

    public void FireMageSpellAttack() //RANGED ATTACK FOR FIRE MAGE
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.rangedTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isSpellCasting", true);
        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies within Range"); //Show the message if no enemies within melee range
        }

        GameManager.instance.ActivePlayer.DisableRangedSpellAnimation();
        GameManager.instance.ActivePlayer.ShootTheTarget();

        HideFireMageSpellsMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }


    #endregion


    #endregion

    #region Priest Class
    #region Priest Class Menu Related
    public void ShowPriestMeleeMenu()
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        priestMeleeMenu.SetActive(true);
        priestMainActionsMenu.SetActive(false);
    }

    public void HidePriestMeleeMenu()
    {
        priestMeleeMenu.SetActive(false);
        priestMainActionsMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }

    public void ShowPriestHealingMenu()
    {
        selectedWalk = false;
        selectedDash = false;
        MoveGrid.instance.HideMovePoints();

        priestMainActionsMenu.SetActive(false);
        priestHealingMenu.SetActive(true);
    }

    public void HidePriestHealingMenu()
    {
        priestHealingMenu.SetActive(false);
        priestMainActionsMenu.SetActive(true);
        GameManager.instance.TargetDisplay.SetActive(false);
    }

    #endregion
    #region Priest Melee Related

    public void PriestMeleeCheck()
    {
        GameManager.instance.ActivePlayer.GetMeleeTargets();

        if (GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {

            GameManager.instance.TargetDisplay.SetActive(true);
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.meleeTargets
                [GameManager.instance.ActivePlayer.currentMeleeTarget].transform.position;

        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies in Melee Range"); //Show the message if no enemies within melee range
        }
    }

    public void PriestMeleeHit() //MELEE HIT FOR THE KNIGHT
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.meleeTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isAttacking", true);
        }
        else
        {
            NoEnemiesInRangeMessage("No Enemies within Range"); //Show the message if no enemies within melee range
        }

        GameManager.instance.ActivePlayer.DisableMeleeAnimation();
        GameManager.instance.ActivePlayer.DoMelee();

        HidePriestMeleeMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }

    #endregion
    #region Priest Heal Related

    public void PriestHealCheck()
    {
        GameManager.instance.ActivePlayer.GetTargetsToHeal();

        if (GameManager.instance.ActivePlayer.healingTargets.Count > 0 )
        {
            GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.healingTargets
                [GameManager.instance.ActivePlayer.currentHealingTarget].transform.position;

            GameManager.instance.TargetDisplay.SetActive(true);

        }
        else
        {
            NoEnemiesInRangeMessage("No Allies To Heal"); //Show the message if no enemies within melee range
        }
    }

    public void PriestHealSpell() //DO THE HEAL
    {
        GameManager.instance.TargetDisplay.SetActive(false);
        GameManager.instance.currentActionCost = 1;

        if(GameManager.instance.ActivePlayer.healingTargets.Count > 0)
        {
            GameManager.instance.ActivePlayer.animator.SetBool("isSpellCasting", true);
        }

        GameManager.instance.ActivePlayer.DisableHealingAnimation();
        GameManager.instance.ActivePlayer.DoHeal();

        HidePriestHealingMenu();
        HideClassPanel();

        StartCoroutine(WaitToEndActionCo(2f));

    }


    #endregion

    #endregion

    #region No Targets Found Messages Related

    public void NoEnemiesInRangeMessage(string messageToShow)
    {
        noEnemyFoundText.text = messageToShow;
        //noEnemyFoundText.gameObject.SetActive(true);
        noTargetsFoundParent.gameObject.SetActive(true);

        noTargetsFoundParent.GetComponent<Image>().color = new Color32(0, 0, 0, 50); //FADE OUT
        noEnemyFoundText.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255); //FADE OUT

        errorCounter = errorDisplayTime; //FADE OUT COUNTER

    }

    #endregion

    #region Select the Next Target

    public void NextMeleeTarget() //Switch to the next target in melee range
    {
        GameManager.instance.ActivePlayer.currentMeleeTarget++;

        if (GameManager.instance.ActivePlayer.currentMeleeTarget >= GameManager.instance.ActivePlayer.meleeTargets.Count)
        {
            GameManager.instance.ActivePlayer.currentMeleeTarget = 0;

        }

        CameraSystem.instance.SetMoveTarget(characterControl.meleeTargets[characterControl.currentMeleeTarget].transform.position); //Move the camera to the target

        GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.meleeTargets
               [GameManager.instance.ActivePlayer.currentMeleeTarget].transform.position;

    }

    public void NextRangedTarget() //Switch to next target in range
    {
        GameManager.instance.ActivePlayer.currentRangedTarget++;

        if (GameManager.instance.ActivePlayer.currentRangedTarget >= GameManager.instance.ActivePlayer.rangedTargets.Count)
        {
            GameManager.instance.ActivePlayer.currentRangedTarget = 0;
        }

        CameraSystem.instance.SetMoveTarget(characterControl.rangedTargets[characterControl.currentRangedTarget].transform.position); //Move the camera to the target

        GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.rangedTargets
            [GameManager.instance.ActivePlayer.currentRangedTarget].transform.position;
    }

    public void NextHealingTarget() //Switch to next healing target
    {

        GameManager.instance.ActivePlayer.currentHealingTarget++;

        if (GameManager.instance.ActivePlayer.currentHealingTarget >= GameManager.instance.ActivePlayer.healingTargets.Count)
        {
            GameManager.instance.ActivePlayer.currentHealingTarget = 0;
        }

        CameraSystem.instance.SetMoveTarget(characterControl.healingTargets[characterControl.currentHealingTarget].transform.position); //Move the camera to the target

        GameManager.instance.TargetDisplay.transform.position = GameManager.instance.ActivePlayer.healingTargets
            [GameManager.instance.ActivePlayer.currentHealingTarget].transform.position;
    }

    #endregion

    #region Grid Related Movement Range

    public void ShowMove() //Shows Walk Range as Grid
    {
        if (GameManager.instance.turnPointsRemaining >= 1)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.ActivePlayer.moveRange,
                GameManager.instance.ActivePlayer.transform.position);

            GameManager.instance.currentActionCost = 1;

            selectedWalk = true;
            selectedDash = false;

        }
    }

    public void ShowDash() //Shows Dash Range as Grid
    {
        if (GameManager.instance.turnPointsRemaining >= 2)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.ActivePlayer.dashRange,
                GameManager.instance.ActivePlayer.transform.position);
            GameManager.instance.currentActionCost = 2;

            selectedWalk = false;
            selectedDash = true;

        }
        else
        {
            NoEnemiesInRangeMessage("Not Enough Turn Points");
        }
    }

    #endregion

    #region SELF DAMAGE FOR DEBUG
    /*
    public void SelfDamage()
    {
        characterControl.animator.SetBool("isDamaged", true);
        GameManager.instance.activePlayer.TakeDamage(characterControl.meleeDamage);
        characterControl.UpdateHealthDisplay();

    }
    */
    #endregion

    public void UpdateTurnPointText(int turnPoints)
    {
        turnPointText.text = "Remaining Turn Points: " + turnPoints;
    }

    public void SkipTurn()
    {
        MoveGrid.instance.HideMovePoints(); //Hide the movepoints
        HideClassPanel(); //Hide the class panels
        GameManager.instance.EndTurn();
    }

    public IEnumerator WaitToEndActionCo(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendTurnPoints();

    }


    private void Update()
    {
        if(errorCounter > 0) //COUNTER FOR THE MESSAGE
        {
            errorCounter -= Time.deltaTime;

            if(errorCounter <= 0)
            {
                //noEnemyPanel.gameObject.SetActive(false);
                isFade = true;
                

            }

        }

        if (isFade) //FADE OUT
        {
            if(startFade > endFade)
            {
                startFade -= speed * Time.deltaTime;
                startTextFade -= speed * Time.deltaTime * 5;

                noTargetsFoundParent.GetComponent<Image>().color = new Color32(0, 0, 0, Convert.ToByte(startFade));
                noEnemyFoundText.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, Convert.ToByte(startTextFade));

            }
            else
            {
                isFade = false;
                startFade = 50;
                startTextFade = 255;
                noTargetsFoundParent.gameObject.SetActive(false);
            }
        }

    }

}
