using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;

public class bulletController : MonoBehaviour
{
    [SerializeField] private GameObject bulletDecal;
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private GameObject bloodPrefab; // Prefab de sangre agregado


    private float speed = 50f;
    private float timeToDestroy = 3f;
    private int playerId;
    public Vector3 target { get; set; }
    public bool hit { get; set; }

    public bool original = false;
    // Start is called before the first frame update
    private void OnEnable()
    {
        Destroy(gameObject, timeToDestroy);
    }


    // Update is called once per frame
    void Update()
    {
        // Calcular la dirección hacia el target
        Vector3 directionToTarget = (target - transform.position).normalized;

        // Rotar la bala hacia el target
        transform.rotation = Quaternion.LookRotation(directionToTarget);
        if (original)
        {

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
        else
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if(!hit && Vector3.Distance(transform.position, target) < .01f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        ContactPoint contact = other.GetContact(0);

        if (other.gameObject.CompareTag("Player") && original)
        {
            PlayerController otherPlayer = other.collider.GetComponent<PlayerController>();
            if (otherPlayer != null && otherPlayer != this) // Evita detectar al propio jugador
            {
                Debug.Log("Hit another player!");

               // StartCoroutine(ChangeReticleColor(Color.red, 1f)); // Cambiar a rojo por 1 segundo

                MessageManager.SendMessage(new HitPlayer(otherPlayer.GetPlayerId()));

            }
        }
        //GameObject.Instantiate(bulletDecal, contact.point,Quaternion.LookRotation(contact.normal));
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Usamos la posición del objeto que entró al trigger
        // Realizar el Raycast hacia el objeto que entra en el trigger
        Vector3 direction = other.transform.position - transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction.normalized, out hit))
        {
            // Usamos la posición del impacto como el punto de contacto
            Vector3 contactPoint = hit.point;


            if (original && other.CompareTag("Enemy"))
            {
                other.GetComponent<EnemiesIA>().TakeDMG();

                if (bloodPrefab != null)
                {
                    GameObject bloodEffect = GameObject.Instantiate(bloodPrefab, contactPoint, Quaternion.LookRotation(hit.normal));
                    Destroy(bloodEffect, 2f);
                    return; // Ajusta el tiempo de vida del efecto si es necesario
                }
            }

            // Instanciamos el VFX en el punto de impacto
            GameObject hitVFX = GameObject.Instantiate(hitVFXPrefab, contactPoint, Quaternion.LookRotation(hit.normal));

            Destroy(hitVFX, 1f);
        }

       
        Destroy(gameObject);


    }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }
}
