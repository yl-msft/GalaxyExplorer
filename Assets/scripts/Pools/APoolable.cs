using UnityEngine;

#pragma warning disable CS0660, CS0661  
namespace Pools
{
   public abstract class APoolable : MonoBehaviour
   {
      public delegate void OnReturnPoolable(APoolable poolable, Transform parent);

      public OnReturnPoolable onReturnToPool;
      protected Transform poolTransform;
      private bool isActive;

      public virtual void Init(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = default(Transform),Transform poolTransform = default(Transform) )
      {
         transform.position = position;
         transform.rotation = rotation;
         this.poolTransform = poolTransform;
         if (parent != null)
         {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
         }
         IsActive = true;
      }

      public virtual bool IsActive
      {
         get => isActive;
         set
         {
            gameObject.SetActive(value);
            isActive = value;
         }
      }

      public virtual void ReturnToPool()
      {
         IsActive = false;
         Reset();
         if (onReturnToPool != null)
         {
            onReturnToPool.Invoke(this, transform.parent);
         }
      }

      protected virtual void Reset()
      {
         transform.position = Vector3.zero;
         transform.rotation = Quaternion.identity;
         transform.SetParent(poolTransform);
         transform.localPosition = Vector3.zero;
      }

      public static bool operator == (APoolable a, APoolable b)
      {
         if (ReferenceEquals(a,null) || a.Equals(null) || !a.IsActive)
         {
            return ReferenceEquals(b,null) || b.Equals(null);
         }
         if (ReferenceEquals(b, null) || b.Equals(null) || !b.IsActive)
         {
            return a.Equals(null);
         }
         return a.Equals(b);
      }

      public static bool operator != (APoolable a, APoolable b)
      {
         return !(a == b);
      }
   }
}
#pragma warning restore CS0660, CS0661  
