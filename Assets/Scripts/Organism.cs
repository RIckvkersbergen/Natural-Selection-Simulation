using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Organism : MonoBehaviour
{
    public DNA dna;
    public GameObject environment;

    public float energy;
    public float energyCost;

    public float fitness = 0;

    [HideInInspector]
    public float[] genome;

    GameObject food;
    public GameObject hair;

    [HideInInspector]
    public bool recentlySelected = false;

    public Vector3 walkPosition;

    Vector3 foodPosition;

    GameObject child;

    Population world;
    Environment envir;

    public float sensoryDistance;
    public float resistance;

    public enum BehaviourState
    {
        Wander,
        Eat,
        Mate,
        Death
    };

    public BehaviourState currentState;

    void Start()
    {
        sensoryDistance = genome[1] * 5f;
        resistance = genome[3];

        transform.localScale = new Vector3(genome[2] * 0.5f, genome[2] * 0.5f, genome[2] * 0.5f);

        environment = GameObject.FindGameObjectWithTag("Environment");
        envir = environment.GetComponent<Environment>();
        world = GetComponentInParent<Population>();

        walkPosition = envir.tiles[Random.Range(0, envir.tiles.Count)].transform.position + new Vector3(0, 0.55f, 0);
        currentState = BehaviourState.Wander;

        StartCoroutine(Death());
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case BehaviourState.Wander:
                Wander();
                StartCoroutine(SenseFood());
                break;

            case BehaviourState.Mate:
                StopCoroutine(SenseFood());
                break;

            case BehaviourState.Eat:
                EatFood();
                StopCoroutine(SenseFood());
                break;

            case BehaviourState.Death:
                Die();
                break;
        }
    }

    public void SetState(BehaviourState state)
    {
        if (state != currentState)
        {
             currentState = state;
        }
    }

    public void Update()
    {
        UpdateState();

        if (fitness >= world.neededFitness && recentlySelected == false)
        {
            world.selectedOrganisms.Add(this);
            recentlySelected = true;
        }
    }

    public void RandomizeTraits()
    {
        //Get the genome
        genome = new float[4];

        //Randomize all traits
        dna.sense = Random.Range(DNA.minSense, DNA.maxSense);
        dna.speed = Random.Range(DNA.minSpeed, DNA.maxSpeed);
        dna.size = Random.Range(DNA.minSize, DNA.maxSize);
        dna.resistance = Random.Range(DNA.minResistance, DNA.maxResistance);

        //Fill the list with the right values
        genome[0] = dna.speed;
        genome[1] = dna.sense;
        genome[2] = dna.size;
        genome[3] = dna.resistance;

        //Change the colour based on the DNA of the organism
        this.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(genome[0], genome[1], genome[2]);

        hair.GetComponent<MeshRenderer>().material.color = new Color(1 - genome[3], 1 - genome[3], 1 - genome[3]);

        MeshRenderer[] alsoHair = hair.GetComponentsInChildren<MeshRenderer>();

        foreach(MeshRenderer rend in alsoHair)
        {
            rend.material.color = new Color(1 - genome[3], 1 - genome[3], 1 - genome[3]);
        }
    }

    public IEnumerator Exhausted()
    {
        int resting = 0;
        int rested = 15;

        recentlySelected = true;

        while(resting < rested)
        {
            resting += 1;
            yield return new WaitForSeconds(1f);
        }

        if (resting == rested)
        {
            resting = 0;
            recentlySelected = false;
            StopCoroutine(Exhausted());
        }
    }

    IEnumerator Death()
    {
        //Energy depletes slowly
        while (energy > 0)
        {
            if (envir.cold) //If a storm happens
            {
                energyCost = ((Mathf.Pow(genome[2], 3) * Mathf.Pow(genome[0], 2)) + genome[1] + 1) + (1 - genome[3]); //Increase energy cost with the resistance
            }
            else if (envir.cold == false) //else
            {
                energyCost = (Mathf.Pow(genome[2], 3) * Mathf.Pow(genome[0], 2)) + genome[1] + 1; //Use the normal energy cost
            }

            //subtract energy each second
            energy -= energyCost;
            yield return new WaitForSeconds(1);
        }

        //If it runs out and the organism is not mating, kill it.
        if(energy <= 0)
        {
            SetState(BehaviourState.Death);
        }
    }

    void Die()
    {
        if (world.selectedOrganisms.Contains(this)) { world.selectedOrganisms.Remove(this); }    
        world.population.Remove(this);      

        Destroy(this.gameObject);
    }

    void EatFood()
    {
        transform.position = Vector3.MoveTowards(transform.position, foodPosition, genome[0] * Time.deltaTime);
        Vector3 direction = (new Vector3(foodPosition.x, 0, foodPosition.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        Quaternion look = Quaternion.LookRotation(direction);
        transform.rotation = look;

        if (Vector3.Distance(transform.position, foodPosition) <= 0.3f)
        {
            SetState(BehaviourState.Wander);

            if (envir.currentFood != null)
            {
                if (envir.currentFood.Contains(food))
                {
                    envir.currentFood.Remove(food);
                    Destroy(food);
                    energy += 10;

                    if (energy > world.maxEnergy)
                    {
                        energy = world.maxEnergy;
                    }
                }
            }

            if (envir.insects != null)
            {
                if (envir.insects.Contains(food))
                {
                    food.GetComponent<Insect>().currentState = Insect.BehaviourState.Death;
                    energy += 15;

                    if (energy > world.maxEnergy)
                    {
                        energy = world.maxEnergy;
                    }
                }
            }
        } 
    }

    public IEnumerator SenseFood()
    {
        while (energy > 0)
        {           
            if (genome[2] >= 0.6)
            {
                if (envir.insects.Count > 0)
                {
                    for (int i = 0; i < envir.insects.Count; i++)
                    {
                        GameObject currentInsect = envir.insects[i];

                        if (Vector3.Distance(transform.position, currentInsect.transform.position) <= sensoryDistance)
                        {
                            food = currentInsect;
                            foodPosition = food.transform.position;
                            SetState(BehaviourState.Eat);
                        }
                    }
                    yield return new WaitUntil(() => food == null);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (envir.currentFood.Count > 0)
            {
                for (int i = 0; i < envir.currentFood.Count; i++)
                {
                    GameObject currentFood = envir.currentFood[i];

                    if (Vector3.Distance(transform.position, currentFood.transform.position) <= sensoryDistance)
                    {
                        food = currentFood;
                        foodPosition = food.transform.position;
                        SetState(BehaviourState.Eat);
                    }
                }
                yield return new WaitUntil(() => food == null);
                yield return new WaitForSeconds(0.5f);
            }            
        }
    }

    public void Procreate()
    {
        //If the fitness is enough
        if(fitness == world.neededFitness)
        {
            fitness = 0;

            //Create a copy of yourself and let it inherit your genes. Add the child to the population
            child = Instantiate(this.gameObject, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z), Quaternion.identity);         
            child.GetComponent<Organism>().SingleInherit();
            child.GetComponent<Organism>().energy = world.maxEnergy;
            world.population.Add(child.GetComponent<Organism>());
        }     
    }

    public void SingleInherit()
    {
        //Get the genome
        genome = new float[3];

        //Set the genome the same as the parent + a certain mutation
        genome[0] = Mathf.Clamp(dna.speed + Random.Range(-world.mutationAmount, world.mutationAmount), DNA.minSpeed, DNA.maxSpeed);
        genome[1] = Mathf.Clamp(dna.sense + Random.Range(-world.mutationAmount, world.mutationAmount), DNA.minSense, DNA.maxSense);
        genome[2] = Mathf.Clamp(dna.size + Random.Range(-world.mutationAmount, world.mutationAmount), DNA.minSize, DNA.maxSize);

        //Use the genes to set the colour of the organism
        this.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(genome[0], genome[1], genome[2]);
    }

    void Wander()
    {
        if (Vector3.Distance(walkPosition, transform.position) <= 1f || walkPosition == null) 
        {
            walkPosition = envir.tiles[Random.Range(0, envir.tiles.Count)].transform.position + new Vector3(0, 0.55f, 0);
        }
        else
        {           
            transform.position = Vector3.MoveTowards(transform.position, walkPosition, genome[0] * Time.deltaTime);
            Vector3 direction = (new Vector3(walkPosition.x, 0, walkPosition.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = look;
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sensoryDistance);
    }
}
