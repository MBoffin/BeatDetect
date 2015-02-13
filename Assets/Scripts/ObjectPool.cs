using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Super basic object pooler
public class ObjectPool : MonoBehaviour {

    [HideInInspector]
    public static ObjectPool current;
    
    public GameObject objectToPool;
    public int poolSize = 100;
    private List<GameObject> pool;

    void Awake() {
        current = this;
    }

	void Start () {
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++) {
            CreateNewObject();
        }
	}

    public GameObject getObject() {
        for (int i = 0; i < poolSize; i++) {
            if (!pool[i].activeInHierarchy) {
                return pool[i];
            }
        }
        return CreateNewObject();
    }

    GameObject CreateNewObject() {
        GameObject obj = Instantiate(objectToPool) as GameObject;
        obj.SetActive(false);
        pool.Add(obj);
        if (pool.Count > poolSize) { poolSize = pool.Count; }
        return obj;
    }
}
