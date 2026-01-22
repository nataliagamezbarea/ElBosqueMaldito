using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletProjectile : MonoBehaviour {

    [SerializeField] private Transform vfxHitGreen;
    [SerializeField] private Transform vfxHitRed;

    private Rigidbody bulletRigidbody;
    private TrailRenderer trail;

    private void Awake() {
        bulletRigidbody = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    private void Start() {
        float speed = 50f;
        bulletRigidbody.linearVelocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            Debug.Log("Hit Enemy");
            Instantiate(vfxHitGreen, transform.position, Quaternion.identity);
        } else {
            Instantiate(vfxHitRed, transform.position, Quaternion.identity);
        }

        if (trail != null) {
            trail.transform.parent = null;
            Destroy(trail.gameObject, trail.time);
        }

        Destroy(gameObject);
    }
}
