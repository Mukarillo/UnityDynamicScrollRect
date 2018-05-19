using UnityEngine;

namespace pooling
{
	public abstract class PoolObject : MonoBehaviour, IPooling
    {
		public virtual string objectName{ get { return ""; } }
		public bool isUsing { get; set; }

        public virtual void OnCollect()
        {
			isUsing = true;
            gameObject.SetActive(true);
        }

        public virtual void OnRelease()
        {
			isUsing = false;
            gameObject.SetActive(false);
        }
    }
}