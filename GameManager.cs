using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; //Singleton

    [SerializeField] private bool setupGameOnStart;
    [SerializeField] private bool loadAllEnemiesOnStart;
    [SerializeField] private GameObject targetDisplay;
    [Tooltip("Set the total turn points that the characters will take in a turn")]
    [SerializeField] private int totalTurnPoints = 2; //Total points for the characters to play

    [Tooltip("This is to track the total remaining turn points")]
    public int turnPointsRemaining; //Tracking the remaining pointss
    public int currentActionCost = 1;

    private CharacterControl activePlayer;
    private int currentChar; //Current Character
    private List<CharacterControl> playerTeam = new List<CharacterControl>(); //Controls player team
    private List<CharacterControl> enemyTeam = new List<CharacterControl>(); //Controls enemy team
    private List<CharacterControl> activeEnemies = new List<CharacterControl>(); //Controls enemy team

    public IEnumerable<CharacterControl> AllCharacters => playerTeam.Concat(activeEnemies);
    public List<CharacterControl> PlayerTeam => playerTeam;
    public List<CharacterControl> ActiveEnemies => activeEnemies;
    public GameObject TargetDisplay => targetDisplay;
    public CharacterControl ActivePlayer => activePlayer;

    private void Awake()
    {
        instance = this; //Singleton
    }

    private void Start()
    {
        LevelGenerator.Instance?.OnLevelGenerationCompleted.AddListener(OnLevelGenerationCompleted);

        if (setupGameOnStart)
        {
            SetupGame();
        }
    }

    private void OnLevelGenerationCompleted()
    {
        if(!setupGameOnStart)
        {
            StartCoroutine(SetupGameDelayed());
        }
    }

    private IEnumerator SetupGameDelayed()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        SetupGame();
    }
   
    public void SetupGame()
    {
        List<CharacterControl> tempList = new List<CharacterControl>();

        tempList.AddRange(FindObjectsOfType<CharacterControl>());

        //IMPLEMENT COIN TOSS METHOD (50% CHANCE TO DECIDE WHICH TEAM TO START)
        foreach (CharacterControl cc in tempList) //Adding teams into the list
        {
            if (cc.isPlayer)
            {
                playerTeam.Add(cc);
            }
            else
            {
                enemyTeam.Add(cc);
            }
        }

        playerTeam = playerTeam.OrderBy(x => Random.Range(int.MinValue, int.MaxValue)).ToList();
        enemyTeam = enemyTeam.OrderBy(x => Random.Range(int.MinValue, int.MaxValue)).ToList();
        if(loadAllEnemiesOnStart)
        {
            activeEnemies = new List<CharacterControl>(enemyTeam);
        }

        activePlayer = AllCharacters.First();
        if (LevelManager.Instance?.PlayerCanControl ?? true)
        {
            CameraSystem.instance.SetMoveTarget(ActivePlayer.transform.position);
        }

        currentChar = -1; //Skipping the first character whether its an enemy
        EndTurn();
    }

    public void AddNewEnemies(List<CharacterControl> enemies)
    {
        List<CharacterControl> tempList = new List<CharacterControl>(enemies);
        int iterations = tempList.Count + 50;
        while (tempList.Count > 0 && iterations > 0)
        {
            int randomPick = Random.Range(0, tempList.Count);

            enemyTeam.Add(tempList[randomPick]);
            activeEnemies.Add(tempList[randomPick]);

            tempList.RemoveAt(randomPick);

            iterations--;
        }
    }

    public void FinishedMovement()
    {
        if (ActivePlayer.isPlayer)
        {
            var roomPos = new Vector2Int(Mathf.RoundToInt(ActivePlayer.transform.position.x / LevelGenerator.TileSize), Mathf.RoundToInt(ActivePlayer.transform.position.z / LevelGenerator.TileSize));
            var room = LevelGenerator.Instance?.GetRoomAt(roomPos);

            if(room != null)
            {
                foreach(var enemy in enemyTeam)
                {
                    if (activeEnemies.Contains(enemy)) continue;

                    var enemyRoomPos = new Vector2Int(
                        Mathf.RoundToInt(enemy.transform.position.x / LevelGenerator.TileSize), 
                        Mathf.RoundToInt(enemy.transform.position.z / LevelGenerator.TileSize));

                    if(LevelGenerator.Instance?.GetRoomAt(enemyRoomPos) == room)
                    {
                        activeEnemies.Add(enemy);
                    }
                }
            }
        }

        SpendTurnPoints();
    }

    public void SpendTurnPoints()
    {
        if (activeEnemies.Count > 0)
        {
            turnPointsRemaining -= currentActionCost; //Spend the turn points
        }
        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining); //Update the turn point

        if (turnPointsRemaining <= 0) //If the turn points <= 0...
        {
            PlayerInputMenu.instance.selectedWalk = false; //Reset the animations
            PlayerInputMenu.instance.selectedDash = false; //Reset the animations

            EndTurn();
        }
        else
        {
            if (ActivePlayer.isPlayer) //If the player is not an enemy...
            {
                if (LevelManager.Instance?.PlayerCanControl ?? true)
                {
                    PlayerInputMenu.instance.ShowClassPanel(); //Show the class panel
                }
            }
            else //If it's an enemy...
            {
                PlayerInputMenu.instance.HideClassPanel(); //Hide the class panel
                ActivePlayer.aiBrain.ChooseAction();
            }
        }
    }

    public void EndTurn()
    {
        CleanupProcess();

        currentChar++;
        if (currentChar >= AllCharacters.Count()) //Returning back to the first character if it's above the count limit
        {
            currentChar = 0;
        }

        activePlayer = AllCharacters.ToList()[currentChar]; //Switch the current character
        if (LevelManager.Instance?.PlayerCanControl ?? true)
        {
            CameraSystem.instance.SetMoveTarget(ActivePlayer.transform.position);//Move the camera
        }
        turnPointsRemaining = totalTurnPoints; //Set the turn points

        if (ActivePlayer.isPlayer) //If the active player is not an enemy...
        {
            if (ActivePlayer.isStunned)
            {
                ActivePlayer.isStunned = false;
                ActivePlayer.stunParticle.Stop();
                EndTurn();
            }

            if (LevelManager.Instance?.PlayerCanControl ?? true)
            {
                PlayerInputMenu.instance.ShowClassPanel(); //Show the class panel
            }
        }
        else //If it's an enemy character...
        {
            PlayerInputMenu.instance.HideClassPanel(); //Hide class panel

            if (ActivePlayer.isStunned)
            {
                ActivePlayer.isStunned = false;
                ActivePlayer.stunParticle.Stop();
                EndTurn();
            }
            else
            {
                ActivePlayer.aiBrain.ChooseAction();
            }
        }

        currentActionCost = 1;

        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);
    }

    #region Cleanup Related
    public void CleanupProcess() 
    {
        foreach (CharacterControl cc in AllCharacters.ToList())
        {
            if (cc.currentHealth <= 0)
            {
                if (playerTeam.Contains(cc)) //If the player team has it
                {
                    playerTeam.Remove(cc); // Remove from the player team list once the character is dead
                }
                if (activeEnemies.Contains(cc)) //If enemy team has it
                {
                    activeEnemies.Remove(cc); //Remove the character from the enemy list
                }
                if (enemyTeam.Contains(cc)) //If enemy team has it
                {
                    enemyTeam.Remove(cc); //Remove the character from the enemy list
                }
            }
        }
    }
    #endregion
}
