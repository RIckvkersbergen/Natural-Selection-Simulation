using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Population : MonoBehaviour
{
    public List<Organism> population = new List<Organism>();
    public List<Organism> selectedOrganisms = new List<Organism>();

    [Range(-1, 1)]
    public float mutationAmount;

    public float neededFitness;
    public float firstGenSize;
    public Organism organism;
    float iterations = 1;
    public float maxEnergy;

    float averageSpeed;
    float averageSense;
    float averageSize;
    float averageResistance;
    float amountAlive;

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI senseText;
    public TextMeshProUGUI sizeText;
    public TextMeshProUGUI resistanceText;
    public TextMeshProUGUI iterationCount;
    public TextMeshProUGUI bestGenomeText;
    public TextMeshProUGUI totalPop;

    float timer;
    public GameObject directionalLight;

    public Environment environment;

    float angle;

    void Start()
    {
        StartCoroutine(Iterations());

        for (int i = 0; i < firstGenSize; i++)
        {
            Spawn();
        }

        StartCoroutine(VisualizeData());

        iterationCount.text = "Iteration: 1";
    }

    void Update()
    {
        angle = Mathf.Lerp(0f, 36f, Time.deltaTime);
        directionalLight.transform.Rotate(Vector3.right, angle);

        LookForMates();
    }

    void Spawn()
    {     
        GameObject tile = environment.tiles[Random.Range(0, environment.tiles.Count)];

        if(tile.GetComponent<Tile>().hasFood || tile.GetComponent<Tile>().hasTree)
        {
            Spawn();                           
        }
        else
        {
            Organism firstGen = Instantiate(organism, tile.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            firstGen.transform.parent = this.transform;
            population.Add(firstGen);
        }      
  
        foreach (Organism organism in population)
        {
            organism.RandomizeTraits();
            organism.energy = maxEnergy;
        }
    }

    IEnumerator ChooseFittest()
    {
        Camera orgCam = null;
        Organism bestOrg = null;
        float[] bestGenome = null;
        float currentFitness = 0;
        float bestFitness = 0;

        while (population.Count > 0)
        {
            if (orgCam != null)
            {
                orgCam.cullingMask = 0;
            }

            for (int i = 0; i < population.Count; i++)
            {
                if (population[i].fitness > currentFitness)
                {
                    currentFitness = population[i].fitness;
                    bestFitness = currentFitness;

                    bestOrg = population[i];
                    bestGenome = bestOrg.genome;                  
                }
            }

            if(bestOrg != null)
            {
                orgCam = bestOrg.gameObject.GetComponentInChildren<Camera>();
                orgCam.cullingMask = LayerMask.NameToLayer("Everything");

                orgCam.enabled = false;
                orgCam.enabled = true;

                bestGenomeText.text = (Mathf.Round(bestGenome[0] * 100) / 100) + "," + (Mathf.Round(bestGenome[1] * 100) / 100) + "," + (Mathf.Round(bestGenome[2] * 100) / 100) + "," + (Mathf.Round(bestGenome[3] * 100) / 100);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator VisualizeData()
    {
        while(population.Count > 0)
        {
            averageSpeed = 0;
            averageSense = 0;
            averageSize = 0;
            averageResistance = 0;
            amountAlive = 0;

            foreach (Organism organism in population)
            {
                averageSpeed += organism.genome[0];
                averageSense += organism.genome[1];
                averageSize += organism.genome[2];
                averageResistance += organism.genome[3];
            }

            averageSpeed = averageSpeed / population.Count;
            averageSense = averageSense / population.Count;
            averageSize = averageSize / population.Count;
            averageResistance = averageResistance / population.Count;
            amountAlive = population.Count;

            speedText.text = (Mathf.Round(averageSpeed * 100) / 100).ToString();
            senseText.text = (Mathf.Round(averageSense * 100) / 100).ToString();
            sizeText.text = (Mathf.Round(averageSize * 100) / 100).ToString();
            resistanceText.text = (Mathf.Round(averageResistance * 100) / 100).ToString();
            totalPop.text = amountAlive.ToString();

            yield return new WaitForSeconds(10);
        }
    }

    IEnumerator Iterations()
    {
        while (timer < 10)
        {
            timer++;

            if (timer == 10)
            {
                float stormChance = Random.Range(1, 11);

                if(stormChance == 2)
                {
                    environment.StartCoroutine(environment.GetComponent<Environment>().Storm());
                }

                iterations++;
                iterationCount.text = "Iteration: " + iterations.ToString();

                foreach (Organism organism in population)
                {
                    organism.fitness += 1;
                }

                if(iterations == 2)
                {
                    StartCoroutine(ChooseFittest());
                }

                timer = 0;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void LookForMates()
    {
        //Loop through all the selected organisms
        for (int i = 0; i < selectedOrganisms.Count; i++)
        {
            for (int j = 0; j < selectedOrganisms.Count; j++)
            {
                if (selectedOrganisms[i] != null && selectedOrganisms[j] != null)
                {                  
                    if (selectedOrganisms[i] != selectedOrganisms[j]) //And the two variables don't point at the same organism
                    {
                        //The needed range is the biggest of the two senseRadiï
                        float neededRange = Mathf.Max(selectedOrganisms[i].sensoryDistance, selectedOrganisms[j].sensoryDistance);

                        //If the distance of the organisms is smaller than the needed range to mate.
                        if (Vector3.Distance(selectedOrganisms[i].transform.position, selectedOrganisms[j].transform.position) <= neededRange)
                        {
                            Organism org1 = selectedOrganisms[i];
                            Organism org2 = selectedOrganisms[j];

                            //Mate and remove the two organisms
                            org1.SetState(Organism.BehaviourState.Mate);
                            org2.SetState(Organism.BehaviourState.Mate);

                            //Move towards eachother while the other is still mating, otherwise go back to wandering and remove the other org from list
                            if (org2.currentState == Organism.BehaviourState.Mate && org2 != null)
                            {
                                org1.transform.position = Vector3.MoveTowards(org1.transform.position, org2.transform.position, org1.genome[0] * Time.deltaTime);
                                org1.transform.LookAt(org2.transform.position);
                            }
                            else
                            {
                                org1.SetState(Organism.BehaviourState.Wander);
                            }

                            if (org1.currentState == Organism.BehaviourState.Mate && org1 != null)
                            {
                                org2.transform.position = Vector3.MoveTowards(org2.transform.position, org1.transform.position, org2.genome[0] * Time.deltaTime);
                                org2.transform.LookAt(org1.transform.position);
                            }
                            else
                            {
                                org2.SetState(Organism.BehaviourState.Wander);
                            }

                            //When close, create offspring
                            if (Vector3.Distance(org1.transform.position, org2.transform.position) <= 1f)
                            {
                                //Create offspring
                                Mate(org1, org2);
                            }
                        }
                    }
                    
                }
                else if(selectedOrganisms[i] == null)
                {
                    selectedOrganisms.Remove(selectedOrganisms[i]);
                }
                else if(selectedOrganisms[j] == null)
                {
                    selectedOrganisms.Remove(selectedOrganisms[j]);
                }
            }
        }
    }


    void Mate(Organism org1, Organism org2)
    {
        //Initialize the DNA of the parents
        float[] org1genes = org1.genome;
        float[] org2genes = org2.genome;

        //Create new DNA strings for the children
        float[] childGenes1 = new float[4];
        float[] childGenes2 = new float[4];

        //Crossoverpoint = 2. Create genes for the children with a mutation amount.
        childGenes1[0] = Mathf.Clamp(org1genes[0] + Random.Range(-mutationAmount, mutationAmount), DNA.minSpeed, DNA.maxSpeed);
        childGenes1[1] = Mathf.Clamp(org1genes[1]+ Random.Range(-mutationAmount, mutationAmount), DNA.minSense, DNA.maxSense);
        childGenes1[2] = Mathf.Clamp(org2genes[2]+ Random.Range(-mutationAmount, mutationAmount), DNA.minSize, DNA.maxSize);
        childGenes1[3] = Mathf.Clamp(org2genes[3] + Random.Range(-mutationAmount, mutationAmount), DNA.minResistance, DNA.maxResistance);

        childGenes2[0] = Mathf.Clamp(org2genes[0] + Random.Range(-mutationAmount, mutationAmount), DNA.minSpeed, DNA.maxSpeed);
        childGenes2[1] = Mathf.Clamp(org2genes[1] + Random.Range(-mutationAmount, mutationAmount), DNA.minSense, DNA.maxSense);
        childGenes2[2] = Mathf.Clamp(org1genes[2] + Random.Range(-mutationAmount, mutationAmount), DNA.minSize, DNA.maxSize);
        childGenes2[3] = Mathf.Clamp(org1genes[3] + Random.Range(-mutationAmount, mutationAmount), DNA.minResistance, DNA.maxResistance);

        //Spawn the two children.
        Organism child1 = Instantiate(organism, org1.transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        Organism child2 = Instantiate(organism, org2.transform.position + new Vector3(0, 1, 0), Quaternion.identity);

        //Make sure children have the organism script
        if (!child1.GetComponent<Organism>())
        {
            child1.gameObject.AddComponent<Organism>();
        }

        if (!child2.GetComponent<Organism>())
        {
            child2.gameObject.AddComponent<Organism>();
        }

        //Use the genes of as colours for the material of the children. Add the children to the population
        child1.transform.parent = this.transform;
        child1.GetComponent<Organism>().genome = childGenes1;
        child1.GetComponent<Organism>().energy = maxEnergy;
        child1.gameObject.GetComponent<MeshRenderer>().material.color = new Color(childGenes1[0], childGenes1[1], childGenes1[2]);

        child1.hair.GetComponent<MeshRenderer>().material.color = new Color(1 - child1.genome[3], 1 - child1.genome[3], 1 - child1.genome[3]);

        MeshRenderer[] alsoHair1 = child1.hair.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer rend in alsoHair1)
        {
            rend.material.color = new Color(1 - child1.genome[3], 1 - child1.genome[3], 1 - child1.genome[3]);
        }

        population.Add(child1.GetComponent<Organism>());

        child2.transform.parent = this.transform;
        child2.GetComponent<Organism>().genome = childGenes2;
        child2.GetComponent<Organism>().energy = maxEnergy;
        child2.gameObject.GetComponent<MeshRenderer>().material.color = new Color(childGenes2[0], childGenes2[1], childGenes2[2]);

        child2.hair.GetComponent<MeshRenderer>().material.color = new Color(1 - child2.genome[3], 1 - child2.genome[3], 1 - child2.genome[3]);

        MeshRenderer[] alsoHair2 = child2.hair.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer rend in alsoHair2)
        {
            rend.material.color = new Color(1 - child2.genome[3], 1 - child2.genome[3], 1 - child2.genome[3]);
        }

        population.Add(child2.GetComponent<Organism>());

        //Parents go wandering again
        org1.SetState(Organism.BehaviourState.Wander);
        org2.SetState(Organism.BehaviourState.Wander);

        //Parents cant mate again for a while
        org1.StartCoroutine(org1.Exhausted());
        org2.StartCoroutine(org2.Exhausted());

        //Remove parents from selectedlist
        selectedOrganisms.Remove(org1);
        selectedOrganisms.Remove(org2);
    }
}
