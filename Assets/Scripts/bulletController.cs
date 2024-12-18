using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;

public class bulletController : MonoBehaviour
{
    [SerializeField] private GameObject bulletDecal;

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
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if(!hit && Vector3.Distance(transform.position, target) < .01f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        ContactPoint contact = other.GetContact(0);

        if (other.gameObject.CompareTag("Player"))
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

    public void SetPlayerId(int id)
    {
        playerId = id;
    }
}
