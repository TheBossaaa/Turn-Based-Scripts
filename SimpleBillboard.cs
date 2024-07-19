using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{

    private void LateUpdate()
    {
        transform.rotation = CameraSystem.instance.transform.rotation;
    }


}
