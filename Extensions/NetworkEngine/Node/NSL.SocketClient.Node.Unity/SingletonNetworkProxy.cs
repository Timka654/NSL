using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork
{
//    public class SingletonNetworkProxy<T, TType, TOptions> : T
//        where T : NetworkProxyClient
//        where TType : MonoBehaviour
//    {
//        #region Singletone

//        public bool dontDestroyOnLoad;

//        private static TType instance;

//        public static TType Instance
//        {
//            get
//            {
//                if (instance == null)
//                    instance = (TType)Resources.FindObjectsOfTypeAll(typeof(TType)).FirstOrDefault();
//                if (instance == null)
//                    Debug.Log("Singleton<" + typeof(TType) + "> instance has been not found.");
//                return instance;
//            }
//        }

//        protected void Awake()
//        {
//            if (this.GetType() != typeof(TType))
//                DestroySelf();

//            if (instance == null)
//            {
//                instance = this as TType;
//            }
//            else if (instance != this)
//            {
//                DestroySelf();
//                return;
//            }

//            if (dontDestroyOnLoad)
//                DontDestroyOnLoad(this.gameObject);

//            base.Awake();
//        }

//        protected void OnValidate()
//        {
//            if (this.GetType() != typeof(TType)) //Change to solve the problem
//            {
//                Debug.Log("Singleton<" + typeof(TType) + "> has a wrong Type Parameter. " +
//                    "Try Singleton<" + this.GetType() + "> instead.");
//#if UNITY_EDITOR
//                UnityEditor.EditorApplication.delayCall -= DestroySelf;
//                UnityEditor.EditorApplication.delayCall += DestroySelf;
//#endif
//            }

//            if (instance == null)
//                instance = this as TType;
//            else if (instance != this)
//            {
//                Debug.Log("Singleton<" + this.GetType() + "> already has an instance on scene. Component will be destroyed.");
//#if UNITY_EDITOR
//                UnityEditor.EditorApplication.delayCall -= DestroySelf;
//                UnityEditor.EditorApplication.delayCall += DestroySelf;
//#endif
//            }
//        }

//        private void DestroySelf()
//        {
//            if (Application.isPlaying)
//                Destroy(this.gameObject);
//            else
//                DestroyImmediate(this.gameObject);
//        }

//        #endregion
//    }
}
