using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Necesario para controlar el NavMeshAgent

public class BulletProjectile : MonoBehaviour {

    [SerializeField] private Transform vfxHitGreen;
    [SerializeField] private Transform vfxHitRed;

    private Rigidbody bulletRigidbody;
    private TrailRenderer trail;

    private void Awake() {
        bulletRigidbody = GetComponent<Rigidbody>();
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        trail = GetComponent<TrailRenderer>();
    }

    private void Start() {
        float speed = 100f;
        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, ~LayerMask.GetMask("Player"))) {
            targetPoint = hit.point;
        } else {
            targetPoint = ray.GetPoint(999f);
        }

        Vector3 direction = (targetPoint - transform.position).normalized;
        transform.forward = direction;
        bulletRigidbody.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy")) {
            Debug.Log("Hit Enemy");
            Instantiate(vfxHitGreen, transform.position, Quaternion.identity);
            
            // 1. Detener el movimiento de la IA
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null) {
                agent.isStopped = true; // Detiene el avance
                agent.enabled = false;  // Desactiva el componente para evitar conflictos
            }

            // 2. Activar animación de muerte
            Animator enemyAnim = other.GetComponent<Animator>();
            if (enemyAnim != null) {
                enemyAnim.SetBool("die", true);
            }

            // 3. Desactivar físicas y tags para que se quede como cadáver decorativo
            Collider enemyCollider = other.GetComponent<Collider>();
            if (enemyCollider != null) {
                enemyCollider.enabled = false;
            }
            other.gameObject.tag = "Untagged";

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