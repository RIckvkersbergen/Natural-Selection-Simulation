using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : MonoBehaviour
{
    public Vector3 walkPosition;
    Environment envir;
    public GameObject environment;

    Population pop;
    public GameObject population;

    bool full;

    public float senseRadius;

    GameObject organism;

    private float speed;
    public float energyCost;
    float energy = 30;

    bool resting;
    bool exhausted;

    public enum BehaviourState
    {
        Wander,
        Attack,
        Death
    };

    public BehaviourState currentState;

    // Start is called before the first frame update
    void Start()
    {
        resting = false;
        exhausted = false;
        full = false;

        senseRadius = 2f;
        speed = 1.25f;
        energyCost = 1;


        population = GameObject.FindGameObjectWithTag("Population");
        environment = GameObject.FindGameObjectWithTag("Environment");

        pop = population.GetComponent<Population>();
        envir = environment.GetComponent<Environment>();

        walkPosition = envir.tiles[Random.Range(0, envir.tiles.Count)].transform.position + new Vector3(0, 0.55f, 0);
        currentState = BehaviourState.Wander;

        StartCoroutine(SenseOrganisms());
        StartCoroutine(Death());
        //StartCoroutine(Exhaustion());
        //StartCoroutine(Resting());
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

            case BehaviourState.Attack:
                Attack();
                break;

            case BehaviourState.Death:
                Die();
                break;
        }
    }

    IEnumerator Exhaustion()
    {
        while (energy > 0)
        {
            if (exhausted) //If exhausted
            {
                senseRadius -= 0.1f; //Diminish the senseRadius
            }

            if(senseRadius <= 0f) //If its 0, start resting
            {
                exhausted = false;
                resting = true;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator Resting()
    {
        while (energy > 0)
        {
            if (resting) //If resting
            {
                senseRadius += 0.1f; //Increase senseradius
            }

            if (senseRadius >= 2f) //If it's near its max
            {
                senseRadius = 2; //Set it to its max
                resting = false; //Stop resting
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void Attack()
    {
        //If organism is null or got away, go back to normal
        if (organism == null || Vector3.Distance(organism.transform.position, this.transform.position) > senseRadius)
        {
            currentState = BehaviourState.Wander;
        }
        else //Otherwise keep attacking
        {
            //Move towards the organism and look at it
            transform.position = Vector3.MoveTowards(transform.position, organism.transform.position, speed * Time.deltaTime);

            Vector3 direction = new Vector3(organism.transform.position.x, 0, organism.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, 0.5f);

            //When close, kill it and gain energy. Start resting and be full.
            if (Vector3.Distance(organism.transform.position, this.transform.position) <= 1f)
            {
                currentState = BehaviourState.Wander;

                organism.GetComponent<Organism>().currentState = Organism.BehaviourState.Death;

                energy += 10;

                if (energy > 30)
                {
                    energy = 30;
                }

                full = true;
            }
        }      
    }

    IEnumerator Full()
    {
        while (energy > 0)
        {
            if (full) //If full
            {
                float waiting = 0;
                float waited = 5;

                //Wait 5 sec to be able to attack again
                if(waiting < waited)
                {
                    waiting++;
                }

                if (waiting == waited)
                {
                    full = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator SenseOrganisms()
    {
        while (energy > 0)
        {
            if (full == false) //If not full
            {
                for (int i = 0; i < pop.population.Count; i++)
                {
                    GameObject target = pop.population[i].gameObject; //Check each organism

                    if (Vector3.Distance(target.transform.position, this.transform.position) <= senseRadius) //if its close
                    {
                        organism = target; //The organism is your current target
                        currentState = BehaviourState.Attack; //Start attacking it
                    }
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator Death()
    {
        //Energy depletes slowly
        while (energy > 0)
        {
            energy -= energyCost;
            yield return new WaitForSeconds(1);
        }

        if (energy <= 0) //If 0, die
        {
            currentState = BehaviourState.Death;
        }
    }

    void Wander()
    {
        if (Vector3.Distance(walkPosition, transform.position) <= 1f || walkPosition == null || walkPosition.y >= 1)
        {
            walkPosition = envir.tiles[Random.Range(0, envir.tiles.Count)].transform.position + new Vector3(0, 0.55f, 0);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, walkPosition, speed * Time.deltaTime);
            Vector3 direction = (new Vector3(walkPosition.x, 0, walkPosition.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, 0.5f);
        }
    }

    void Die()
    {
        envir.predators.Remove(this.gameObject);
        Destroy(this.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, senseRadius);
    }
}
