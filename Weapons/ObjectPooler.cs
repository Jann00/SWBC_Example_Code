//This script is written by Jann Mjoen
//This script is used for object pooling functionality

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        //public Transform parent;
        public int size;
        public bool enableByDefault = false;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public static ObjectPooler instance;

    //Sets up a singleton for the object pooler.
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //This sets up all of the object pools
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (Pool pool in pools)
        {
            //This sets up the queue for the pool
            Queue<GameObject> objectPool = new Queue<GameObject>();
            //The objects are instatiated based on the pool's size and adds them to the queue
            for (int i = 0; i < pool.size; i++)
            {
                if (pool.prefab != null)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    //If the pool has enableByDefault is true the object is active in the scene, if it is set to false the object will be turned off.
                    if (pool.enableByDefault)
                    {
                        obj.SetActive(true);
                        obj.transform.position = new Vector3(10000, 0, 10000);
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                    //Adds the object to the queue
                    objectPool.Enqueue(obj);
                }
                else
                {
                    Debug.LogError("Pool '" + pool.tag + "' is missing its prefab");
                }
            }
            //This adds the queue to the poolDictionary.
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    //This function is called when a bullet should be instantiated from the assigned pool using a world position.
    //This is a lot more efficient than using instantiate, and it essentially does the same thing.
    public GameObject SpawnFromPool(string tag, Vector3 pos)
    {
        //Checks wheter or not the pool tag exists in the dictionary.
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
            return null;
        }
        
        //Spawns the first object in the queue, and sets its position and rotation
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        if (objectToSpawn == null)
        {
            objectToSpawn = Instantiate(pools[poolDictionary.ContainsKey(tag).GetHashCode()].prefab);
        }

        //Sets the position and rotation of the object
        objectToSpawn.transform.position = pos;
        objectToSpawn.transform.rotation = new Quaternion();

        //This is to make sure the object is reset, before it is set to active again.
        objectToSpawn.SetActive(false);
        objectToSpawn.SetActive(true);       
      
        //Lastly it adds the object back to the queue and returns the game object to where it was called from.
        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

}
