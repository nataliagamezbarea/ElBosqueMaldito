using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private static Transform player;

    [Header("Ajustes")]
    public float attackDistance = 1.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        // Personalizamos cada zombie para que no sean iguales
        agent.speed = Random.Range(2.5f, 4.5f); // Unos zombies corren más que otros
        agent.angularSpeed = Random.Range(100f, 200f); 
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackDistance)
        {
            Atacar();
        }
        else
        {
            Perseguir();
        }
    }

    void Perseguir()
    {
        agent.isStopped = false;
        // Actualizamos el destino cada 5 frames para ahorrar recursos y evitar errores IsFinite
        if (Time.frameCount % 5 == 0)
        {
            agent.SetDestination(player.position);
        }
        
        animator.SetBool("isRunning", agent.velocity.sqrMagnitude > 0.1f);
    }

    void Atacar()
    {
        agent.isStopped = true;
        animator.SetBool("isRunning", false);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("bite");
            lastAttackTime = Time.time;
        }
    }
}