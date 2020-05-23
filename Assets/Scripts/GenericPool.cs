using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericPool<T> : MonoBehaviour where T : Component
{
    [SerializeField] protected T m_PooledObjectPrefab;
    [SerializeField] [Min(1)] protected int m_PoolSize;

    protected int m_PoolCurrentCount;
    protected Queue<T> m_Pool;
    protected Transform m_PoolRoot;
    protected Vector3 m_PoolSpawnPosition = Vector3.zero;
    protected Vector3 m_PoolSpawnScale = Vector3.one;
    protected Quaternion m_PoolSpawnRotation = Quaternion.identity;

    public int PoolSize { get => m_PoolSize; }
    public int PoolCurrentCount { get => m_PoolCurrentCount; }

    protected virtual void Awake()
    {
        if (m_PooledObjectPrefab == null) throw new UnityException($"[{gameObject.name}] prefab is null. Must be not null.");

        //Root'u oluştur
        m_PoolRoot = new GameObject(m_PooledObjectPrefab.gameObject.name + " Pool Root").transform;

        //Obje havuzunu doldur
        FillPool();
    }

    protected virtual void OnDestroy()
    {
        ClearPool();
#if UNITY_EDITOR
        Debug.Log(m_PooledObjectPrefab.gameObject.name + " pool is no longer available.");
#endif
    }

    protected void ClearPool()
    {
        foreach (var po in m_Pool)
        {
            if (po != null)
            {
                Destroy(po.gameObject);
            }
        }

        if (m_PoolRoot)
            Destroy(m_PoolRoot.gameObject);
    }

    protected void QueueObject(T objectToAddQueue)
    {
        m_PoolCurrentCount++;

        m_Pool.Enqueue(objectToAddQueue);
    }

    protected virtual T SpawnObject()
    {
        T spawnedObject = Instantiate(m_PooledObjectPrefab, m_PoolSpawnPosition, m_PoolSpawnRotation, m_PoolRoot);

        spawnedObject.gameObject.SetActive(false);

        return spawnedObject;
    }

    protected virtual void IncreasePoolSize()
    {
        int increaseCount = m_PoolSize / 3;

        for (int i = 0; i < increaseCount; i++)
        {
            QueueObject(SpawnObject());
        }

        m_PoolSize += increaseCount;
    }

    protected virtual void FillPool()
    {
        if (m_Pool == null) m_Pool = new Queue<T>();

        for (int i = 0; i < m_PoolSize; i++)
        {
            QueueObject(SpawnObject());
        }
    }

    public virtual T GetObjectFromPool()
    {
        if (m_PoolCurrentCount > 0)
        {
            T gameObjectFromPool = m_Pool.Dequeue();

            gameObjectFromPool.transform.parent = null;
            gameObjectFromPool.gameObject.SetActive(true);

            m_PoolCurrentCount--;

            return gameObjectFromPool;
        }

        IncreasePoolSize();

        return GetObjectFromPool();
    }

    public virtual T GetObjectWithPosAndRot(Vector3 position, Quaternion rotation)
    {
        T objectFromPool = GetObjectFromPool();

        objectFromPool.transform.position = position;
        objectFromPool.transform.rotation = rotation;

        return objectFromPool;
    }

    public virtual void AddObjectToPool(T objectToAdd)
    {
        m_Pool.Enqueue(objectToAdd);

        m_PoolCurrentCount++;

        objectToAdd.transform.position = m_PoolSpawnPosition;
        objectToAdd.transform.rotation = m_PoolSpawnRotation;
        objectToAdd.transform.localScale = m_PoolSpawnScale;
        objectToAdd.transform.parent = m_PoolRoot;
        objectToAdd.gameObject.SetActive(false);
    }

    public override string ToString()
    {
        return $"{typeof(T).Name} pool. Pool size: {m_PoolSize}, Current count: {m_PoolCurrentCount}";
    }
}
