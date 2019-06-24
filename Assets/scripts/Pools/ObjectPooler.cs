using System.Collections.Generic;
using UnityEngine;

namespace Pools
{
   public class ObjectPooler : MonoBehaviour
   {
      [SerializeField] private int poolSize;
      [SerializeField] private int maxPoolSize;

      private Queue<APoolable> queue;
      private int lastInitializedSize;

      public static ObjectPooler CreateObjectPool<T>(int poolSize, Transform parent = null) where T : APoolable
      {
         var go = new GameObject(typeof(ObjectPooler).ToString() + typeof(T).ToString());
         var result = go.AddComponent<ObjectPooler>();
         result.poolSize = poolSize;
         result.Init<T>();
         if (parent != null)
         {
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
         }
         return result;
      }

      private void Init<T>() where T : APoolable
      {
         while (queue != null)
         {
            var objectToDestroy = queue.Dequeue();
            if (objectToDestroy != null)
            {
               Destroy(objectToDestroy.gameObject);
            }

            if (queue.Count <= 0)
            {
               queue = null;
            }
         }
         queue = new Queue<APoolable>();
         for (int i = 0; i < poolSize; i++)
         {
            var instantiatedPoolable = InstantiateNewObject<T>();
            instantiatedPoolable.transform.SetParent(transform);
            queue.Enqueue(instantiatedPoolable);
         }

         lastInitializedSize = poolSize;
      }

      public T GetNextObject<T>(
         Vector3 position = default(Vector3), 
         Quaternion rotation = default(Quaternion), 
         Transform parent = default(Transform)) where T : APoolable
      {
         if (queue == null || lastInitializedSize != poolSize)
         {
            Init<T>();
         }

         T result = null;
         if (queue.Count < 1)
         {
            result = InstantiateNewObject<T>();
         }
         else
         {
            result = (T) queue.Dequeue();
         }
         result.Init(position, rotation, parent == null ? transform : parent);
         return result;
      }

      private void HandleReturnToPool(APoolable poolable)
      {
         poolable.transform.SetParent(transform);
         queue.Enqueue(poolable);
      }

      private T InstantiateNewObject<T>() where T : APoolable
      {
         var go = new GameObject(typeof(T).ToString());
         var result = go.AddComponent<T>();
         result.OnDestroy += HandleReturnToPool;
         return result;
      }
   }
}
