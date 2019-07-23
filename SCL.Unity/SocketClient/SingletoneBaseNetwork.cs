using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SCL.SocketClient
{
    public class SingletoneBaseNetwork<T, TType> : BaseNetwork<T> where T : BaseSocketNetworkClient
        where TType : MonoBehaviour
    {
        #region Singletone

        public bool dontDestroyOnLoad;

        private static TType instance;
        public static TType Instance
        {
            get
            {
                if (instance == null)
                    instance = (TType)Resources.FindObjectsOfTypeAll(typeof(TType)).FirstOrDefault();
                if (instance == null)
                    Debug.LogError("Singleton<" + typeof(TType) + "> instance has been not found.");
                return instance;
            }
        }

        protected override void Awake()
        {
            if (this.GetType() != typeof(TType))
                DestroySelf();

            if (instance == null)
            {
                instance = this as TType;
            }
            else if (instance != this && !dontDestroyOnLoad)
                DestroySelf();
            else if (instance != this && dontDestroyOnLoad)
                instance = this as TType;

                if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            base.Awake();
        }

        protected void OnValidate()
        {
            if (this.GetType() != typeof(TType)) //Change to solve the problem
            {
                Debug.LogError("Singleton<" + typeof(TType) + "> has a wrong Type Parameter. " +
                    "Try Singleton<" + this.GetType() + "> instead.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall -= DestroySelf;
                UnityEditor.EditorApplication.delayCall += DestroySelf;
#endif
            }

            if (instance == null)
                instance = this as TType;
            else if (instance != this && !dontDestroyOnLoad)
            {
                Debug.LogError("Singleton<" + this.GetType() + "> already has an instance on scene. Component will be destroyed.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall -= DestroySelf;
                UnityEditor.EditorApplication.delayCall += DestroySelf;
#endif
            }
            else if (instance != this && dontDestroyOnLoad)
                instance = this as TType;
        }

        private void DestroySelf()
        {
            if (Application.isPlaying)
                Destroy(this);
            else
                DestroyImmediate(this);
        }

        #endregion
    }
}
