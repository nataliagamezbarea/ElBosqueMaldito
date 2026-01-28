using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProjectilBala : MonoBehaviour {

    [SerializeField] private Transform vfxHitRed;
    [SerializeField] private Transform vfxHitYellow;
    private string nombreGrupoPool = "ProyectilBala";
    private string nombreGrupoVfxRed;
    private string nombreGrupoVfxYellow;

    private Rigidbody bulletRigidbody;
    private TrailRenderer trail;

    private void Awake() {
        bulletRigidbody = GetComponent<Rigidbody>();
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        trail = GetComponent<TrailRenderer>();
    }

    private void Start() {
        if (GestorPools.Instancia != null) {
            if (vfxHitRed != null) {
                nombreGrupoVfxRed = vfxHitRed.name;
                GestorPools.Instancia.RegistrarGrupo(nombreGrupoVfxRed, vfxHitRed.gameObject, 10);
            }
            if (vfxHitYellow != null) {
                nombreGrupoVfxYellow = vfxHitYellow.name;
                GestorPools.Instancia.RegistrarGrupo(nombreGrupoVfxYellow, vfxHitYellow.gameObject, 10);
            }
        }
    }

    private void OnEnable() {
        if (trail != null) {
            trail.Clear();
        }

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

        CancelInvoke(nameof(DesactivarBala));
        Invoke(nameof(DesactivarBala), 3f);
    }

    private void OnTriggerEnter(Collider other) {
        // Evitar fuego amigo
        if (other.CompareTag("Player")) return;

        // Si impactamos con un enemigo (asegúrate de que el Tag sea "Enemy" o "Zombie" según tu Inspector)
        if (other.CompareTag("Enemy") || other.CompareTag("Zombie")) {
            
            // --- CORRECCIÓN CRÍTICA ---
            // Delegamos la muerte al script del propio Zombi.
            // Esto asegura que se ejecute AlMorir() y la corrutina de espera.
            ControladorZombi zombieScript = other.GetComponent<ControladorZombi>();
            
            if (zombieScript != null) {
                zombieScript.RecibirDano(10); // O la vida que quieras quitarle
            }

            GenerarVfx(vfxHitRed, nombreGrupoVfxRed, transform.position);
        } else {
            GenerarVfx(vfxHitYellow, nombreGrupoVfxYellow, transform.position);
        }

        DesactivarBala();
    }

    private void DesactivarBala() {
        CancelInvoke(nameof(DesactivarBala));
        if (GestorPools.Instancia != null) {
            GestorPools.Instancia.DevolverAGrupo(nombreGrupoPool, gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void GenerarVfx(Transform vfxPrefab, string nombreGrupo, Vector3 posicion) {
        if (vfxPrefab == null) return;

        if (GestorPools.Instancia != null) {
            GameObject vfx = GestorPools.Instancia.GenerarDesdeGrupo(nombreGrupo, posicion, Quaternion.identity);
            if (vfx != null) {
                RetornoPoolAutomatico retorno = vfx.GetComponent<RetornoPoolAutomatico>();
                if (retorno != null) retorno.nombreGrupo = nombreGrupo;
            }
        } else {
            Instantiate(vfxPrefab, posicion, Quaternion.identity);
        }
    }
}