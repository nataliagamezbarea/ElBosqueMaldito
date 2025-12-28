using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;


public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private GameObject aimVirtualCamera;
    [SerializeField] private float normalSensitivity = 2f;
    [SerializeField] private float aimSensitivity = 0.5f;


    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;


    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }


    private void Update()
    {
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
        }
        else
        {
            aimVirtualCamera.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
        }
    }
}



