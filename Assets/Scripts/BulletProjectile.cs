using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            // Notificar al Spawner pasando el objeto que ha muerto
            ZombieSpawner spawner = Object.FindFirstObjectByType<ZombieSpawner>();
            if (spawner != null) {
                spawner.ZombieMuerto(other.gameObject);
            }

            // Desactivar IA
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null) {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Animación de muerte
            Animator enemyAnim = other.GetComponent<Animator>();
            if (enemyAnim != null) enemyAnim.SetBool("die", true);

            // Cambiar Tag para que no reciba más impactos
            other.gameObject.tag = "Untagged"; 
            
            // Desactivar colisiones para que el jugador pase a través del cadáver
            if(other.GetComponent<Collider>()) other.GetComponent<Collider>().enabled = false;

            // OPCIONAL: Si el zombie tiene Rigidbody, lo hacemos cinemático para que no ruede raro
            Rigidbody zombieRb = other.GetComponent<Rigidbody>();
            if (zombieRb != null) zombieRb.isKinematic = true;

            // --- SE HA ELIMINADO EL DESTROY(other.gameObject) PARA CONSERVAR EL CADÁVER ---

            Instantiate(vfxHitGreen, transform.position, Quaternion.identity);
        } else {
            Instantiate(vfxHitRed, transform.position, Quaternion.identity);
        }

        // Limpieza del proyectil y el rastro
        if (trail != null) {
            trail.transform.parent = null;
            Destroy(trail.gameObject, trail.time);
        }
        
        Destroy(gameObject); // Siempre destruir la bala al chocar
    }
}