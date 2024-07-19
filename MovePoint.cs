using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePoint : MonoBehaviour
{
    [Header("These variables will be filled automatically")]
    public GameObject player;
    public CharacterControl characterControl;

    

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        characterControl = GameObject.FindObjectOfType<CharacterControl>();

    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");

    }

    private void OnMouseDown()
    {
        //FindObjectOfType<CharacterControl>().MoveToPoint(transform.position);

        if (Input.mousePosition.y > Screen.height * .4f) //Preventing accidentally clicking on tiles when wanted to touch the button
        {
            GameManager.instance.ActivePlayer.MoveToPoint(transform.position);

            MoveGrid.instance.HideMovePoints();

            PlayerInputMenu.instance.HideClassPanel();


        }




    }


}
