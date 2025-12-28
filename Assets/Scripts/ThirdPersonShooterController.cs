using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private GameObject aimVirtualCamera;
    [SerializeField] private float normalSensitivity = 2f;
    [SerializeField] private float aimSensitivity = 0.5f;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    private void Update() {
    Vector3 mouseWorldPosition = Vector3.zero;
    Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
    Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

    if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask)) {
        debugTransform.position = raycastHit.point;
        mouseWorldPosition = raycastHit.point;
    } else {
        mouseWorldPosition = ray.GetPoint(999f);
    }

    if (starterAssetsInputs.aim) {
        aimVirtualCamera.gameObject.SetActive(true);
        thirdPersonController.SetSensitivity(aimSensitivity);

        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    } else {
        aimVirtualCamera.gameObject.SetActive(false);
        thirdPersonController.SetSensitivity(normalSensitivity);
    }
}
}