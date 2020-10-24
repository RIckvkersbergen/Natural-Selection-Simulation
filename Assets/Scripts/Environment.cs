using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public int resolution;
    public int amountOfTrees;
    public int amountOfFood;
    public int amountOfInsects;
    public int amountOfPredators;

    public ParticleSystem snow;
    public Material groundMat;
    [HideInInspector] public bool cold;

    public GameObject foodPrefab;
    public GameObject predatorPrefab;
    public GameObject cube;
    public GameObject insectPrefab;
    GameObject tile;
    public List<GameObject> tiles;

    public GameObject[] trees;
    public List<GameObject> currentFood;
    public List<GameObject> insects;
    public List<GameObject> predators;

    void Start()
    {
        CreateMap();
        PlaceObjects();

        snow.Stop();
        cold = false;
    }

    void Update()
    {
        SpawnFood();
        SpawnInsects();
    }

    void CreateMap()
    {
        tiles = new List<GameObject>();

        for (int x = 0; x < resolution; x++) //For each cell
        {
            for (int z = 0; z < resolution; z++)//For each cell
            { 
                tile = Instantiate(cube, new Vector3(x, 0, z), Quaternion.identity);
                tiles.Add(tile);
                tile.transform.parent = this.transform;
            }
        }
    }

    void PlaceObjects()
    {
        for (int i = 0; i < amountOfTrees; i++)
        {
            PlaceTrees();
        }

        currentFood = new List<GameObject>();
        for (int i = 0; i < amountOfFood; i++)
        {
            PlaceFood();
        }

        insects = new List<GameObject>();
        for (int i = 0; i < amountOfInsects; i++)
        {
            PlaceInsects();
        }

        predators = new List<GameObject>();
        for (int i = 0; i < amountOfPredators; i++)
        {
            PlacePredators();
        }
    }

    void PlaceTrees()
    {
        GameObject tile = tiles[Random.Range(0, tiles.Count)];

        bool hasTree = tile.GetComponent<Tile>().hasTree;

        if (hasTree == false)
        {
            GameObject tree = trees[Random.Range(0, trees.Length)];
            GameObject spawnedTree = Instantiate(tree, new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z), Quaternion.Euler(1, Random.Range(1, 360), 1));
            spawnedTree.transform.parent = this.transform;
            tile.GetComponent<Tile>().hasTree = true;
        }
        else
        {
            PlaceTrees();
        }
    } 

    void PlaceInsects()
    {
        GameObject tile = tiles[Random.Range(0, tiles.Count)];

        bool hasTree = tile.GetComponent<Tile>().hasTree;
        bool hasFood = tile.GetComponent<Tile>().hasFood;

        if (hasTree == false)
        {
            GameObject insect = Instantiate(insectPrefab, new Vector3(tile.transform.position.x, tile.transform.position.y + 1f, tile.transform.position.z), Quaternion.identity);
            insect.transform.parent = this.transform;
            insects.Add(insect);
        }
        else
        {
            PlaceInsects();
        }
    }

    void PlacePredators()
    {   
        GameObject tile = tiles[Random.Range(0, tiles.Count)];

        bool hasTree = tile.GetComponent<Tile>().hasTree;
        bool hasFood = tile.GetComponent<Tile>().hasFood;
      
        if (hasTree == false && hasFood == false)
        {
            GameObject predator = Instantiate(predatorPrefab, new Vector3(tile.transform.position.x, tile.transform.position.y + 1f, tile.transform.position.z), Quaternion.identity);
            predator.transform.parent = this.transform;
            predators.Add(predator);
            tile.GetComponent<Tile>().hasFood = true;
        }
        else
        {
            PlacePredators();
        }
    }

    void PlaceFood()
    {
        GameObject tile = tiles[Random.Range(0, tiles.Count)];

        bool hasFood = tile.GetComponent<Tile>().hasFood;
        bool hasTree = tile.GetComponent<Tile>().hasTree;

        if (hasFood == false && hasTree == false)
        {
            GameObject food = Instantiate(foodPrefab, new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z), Quaternion.identity);
            food.transform.parent = this.transform;
            currentFood.Add(food);
            tile.GetComponent<Tile>().hasFood = true;
            tile.GetComponent<Tile>().placedObject = food;
        }
        else
        {
            PlaceFood();
        }
    }

    void SpawnFood()
    {
        if(currentFood.Count < amountOfFood)
        {
            GameObject tile = tiles[Random.Range(0, tiles.Count)];

            bool hasFood = tile.GetComponent<Tile>().hasFood;
            bool hasTree = tile.GetComponent<Tile>().hasTree;
          
            if (hasFood == false && hasTree == false)
            {
                GameObject food = Instantiate(foodPrefab, new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z), Quaternion.identity);
                food.transform.parent = this.transform;
                currentFood.Add(food);
                tile.GetComponent<Tile>().placedObject = food;
            }             
        }
    }

    void SpawnInsects()
    {
        if (insects.Count < amountOfInsects)
        {
            GameObject tile = tiles[Random.Range(0, tiles.Count)];

            bool hasFood = tile.GetComponent<Tile>().hasFood;
            bool hasTree = tile.GetComponent<Tile>().hasTree;


            if (hasFood == false && hasTree == false)
            {
                GameObject insect = Instantiate(insectPrefab, new Vector3(tile.transform.position.x, tile.transform.position.y + 1f, tile.transform.position.z), Quaternion.identity);
                insect.transform.parent = this.transform;
                insects.Add(insect);
            }       
        }
    }

    public IEnumerator Storm()
    {
        cold = true;

        float counter = 0;
        float stormDuration = 10;

        snow.Play();
        groundMat.color = Color.white;

        while (counter < stormDuration)
        {
            counter++;
            yield return new WaitForSeconds(1);
        }       

        if(counter == stormDuration)
        {
            snow.Stop();
            counter = 0;
            groundMat.color = Color.green;

            cold = false;

            StopCoroutine(Storm());
        }
    }
}
