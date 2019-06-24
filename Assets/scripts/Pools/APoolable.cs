using UnityEngine;

#pragma warning disable CS0660, CS0661  
namespace Pools
{
   public abstract class APoolable : MonoBehaviour
   {
      public delegate void OnUsePoolable(APoolable poolable);

      public delegate void OnDestroyPoolable(APoolable poolable, Transform parent);

      public event OnUsePoolable OnPoolableUsed;
      public event OnDestroyPoolable OnPoolableDestroyed;

      public virtual void Init(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = default(Transform))
      {
         transform.position = position;
         transform.rotation = rotation;
         if (parent != null)
         {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
         }
         gameObject.SetActive(true);
         IsActive = true;
      }
   
      public virtual bool IsActive { get; set; }

      public virtual void Use()
      {
         gameObject.SetActive(false);
         IsActive = false;
         Reset();
         if (OnPoolableUsed != null)
         {
            OnPoolableUsed.Invoke(this);
         }
      }

      protected virtual void OnDestroy()
      {
         if (OnPoolableDestroyed != null)
         {
            OnPoolableDestroyed.Invoke(this, transform.parent);
         }
      }

      protected virtual void Reset()
      {
         transform.position = Vector3.zero;
         transform.rotation = Quaternion.identity;
         transform.SetParent(null);
         transform.localPosition = Vector3.zero;
      }

      public static bool operator == (APoolable a, APoolable b)
      {
         if (ReferenceEquals(a, null) || !a.IsActive)
         {
            return ReferenceEquals(b,null) || !b.IsActive;
         }
         if (ReferenceEquals(b, null) || !b.IsActive)
         {
            return !a.IsActive;
         }
         return ReferenceEquals(a,b);
      }

      public static bool operator != (APoolable a, APoolable b)
      {
         return !(a == b);
      }
   }
}
#pragma warning restore CS0660, CS0661  
