using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Insect : MonoBehaviour
{
    Vector3 walkPosition;
    Environment envir;
    public GameObject environment;
    float speed;

    public enum BehaviourState
    {
        Wander,
        Death
    };

    public BehaviourState currentState;

    // Start is called before the first frame update
    void Start()
    {
        speed = 0.2f;

        environment = GameObject.FindGameObjectWithTag("Environment");
        envir = environment.GetComponent<Environment>();

        walkPosition = envir.tiles[Random.Range(0, envir.tiles.Count)].transform.position + new Vector3(0, 0.55f, 0);
        currentState = BehaviourState.Wander;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case BehaviourState.Wander:
                Wander();
                break;

            case BehaviourState.Death:
                Die();
                break;
        }
    }

    void Wander()
    {
        if (Vector3.Distance(walkPosition, transform.position) <= 1f || walkPosition == null)
        {
            walkPosition = envir.tiles[Random.Range(0, envir.tiles.Count)].transform.position + new Vector3(0, 0.55f, 0);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, walkPosition, speed * Time.deltaTime);
            Vector3 direction = (new Vector3(walkPosition.x, 0, walkPosition.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = look;
        }
    }

    void Die()
    {
        envir.insects.Remove(this.gameObject);
        Destroy(this.gameObject);
    }
}
