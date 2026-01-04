using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Animations.Rigging;

public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Referencias de Sistema")]
    [SerializeField] private SwitchCameraSystem cameraSystem; 

    [SerializeField] private Rig aimRig;
    [SerializeField] private GameObject aimVirtualCamera;
    [SerializeField] private float normalSensitivity = 2f;
    [SerializeField] private float aimSensitivity = 0.5f;
    [SerializeField] private LayerMask aimColliderLayerMask;
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform spawnBulletPosition;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    private float aimRigWeight;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        bool es3P = (cameraSystem != null) ? cameraSystem.EsTerceraPersona : true;

        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = hit.point;
            if (debugTransform != null) debugTransform.position = hit.point;
        }
        else
        {
            mouseWorldPosition = ray.GetPoint(999f);
        }

        bool isAiming = !es3P || starterAssetsInputs.aim || starterAssetsInputs.shoot;

        if (isAiming)
        {
            if (es3P) aimVirtualCamera.SetActive(true);
            else aimVirtualCamera.SetActive(false);

            thirdPersonController.SetSensitivity(aimSensitivity);
            
            Vector3 worldCameraForward = Camera.main.transform.forward;
            worldCameraForward.y = 0; 

            if (worldCameraForward != Vector3.zero)
            {
                if (es3P)
                {
                    thirdPersonController.SetRotateOnMove(false);
                    Quaternion targetRotation = Quaternion.LookRotation(worldCameraForward);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
                }
            }
            
            animator.SetBool("IsAiming", true);
            aimRigWeight = 1f;
        }
        else
        {
            aimVirtualCamera.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            animator.SetBool("IsAiming", false);
            aimRigWeight = 0f;
        }

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);

        if (starterAssetsInputs.shoot)
        {
            animator.SetTrigger("Shoot");
            
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            
            starterAssetsInputs.shoot = false;
        }
    }
}