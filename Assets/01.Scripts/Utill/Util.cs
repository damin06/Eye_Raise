using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;


public enum FirebaseState : ulong
{
    Failed,
    success
}
[Serializable]
public struct MessageResult
{
    public string Message;
    public int ErrorCode;
    public FirebaseState State;
    public object Result;
}



namespace Util
{
    using System.Diagnostics;
    using UnityEngine;

    public delegate void Event();
    public enum AniState
    {
        OnStart = 0,
        OnEnd = 1,

    }


    public  class Util
    {
        public static void InjectionComponents(object root)
        {
            Type type = root.GetType();
            MonoBehaviour script = root as MonoBehaviour;

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach(var item in fields)
            {
                var atrribute = (FindComonenetAtrribute)item.GetCustomAttribute(typeof(FindComonenetAtrribute));

                if (atrribute == null)
                    continue;

                Type filedType = item.FieldType;
                Transform tr = script.transform.Find(atrribute._gameObjectName);
                
                Component component = tr.GetComponent<Component>();

                if(component == null)
                {
                    UnityEngine.Debug.LogError($"Component {filedType.Name} of object {atrribute._gameObjectName} does not exist.");
                    continue;
                }

                item.SetValue(script, component);
            }
        }

        private float CalculateMappedValue(float input, float minInput, float maxInput, float minOutput, float maxOutput)
        {
            float normalized = (input - minInput) / (maxInput - minInput);
            return Mathf.Lerp(maxOutput, minOutput, normalized);
        }
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
        }
        public static void BindAnimatorEvent(Animator _ani, string _aniName, AniState _state, Event _event)
        {
            CoroutineHelper.StartCoroutine(BindAnimatorEventRoutine(_ani, _aniName, _state, _event));
        }

        public static IEnumerator BindAnimatorEventRoutine(Animator _ani, string _aniName, AniState _state, Event _event)
        {
            yield return new WaitUntil(() => _ani.GetCurrentAnimatorStateInfo(0).IsName(_aniName) && _ani.GetCurrentAnimatorStateInfo(0).normalizedTime >= (int)_state);
            _event();
        }

        public static void LearpPosition(Transform _obj, Vector3 _startPos, Vector3 _endPos)
        {
            CoroutineHelper.StartCoroutine(LearpPositionRoutine(_obj, _startPos, _endPos));
        }

        public static IEnumerator LearpPositionRoutine(Transform _obj, Vector3 _startPos, Vector3 _endPos)
        {
            float _timeStartedLerping = Time.time;
            float timeTakenDuringLerp = 1f;

            while (true)
            {
                float timeSinceStarted = Time.time - _timeStartedLerping;
                float percentageComplete = timeSinceStarted / timeTakenDuringLerp;

                _obj.transform.position = Vector3.Lerp(_startPos, _endPos, percentageComplete);

                if (percentageComplete >= 1.0f)
                {
                    yield return null;
                }
            }
        }

        public void LearpRectPosition(RectTransform _obj, Vector3 _startPos, Vector3 _endPos)
        {
            CoroutineHelper.StartCoroutine(LearpRectPositionRoutine(_obj, _startPos, _endPos));
        }

        public IEnumerator LearpRectPositionRoutine(RectTransform _obj, Vector3 _startPos, Vector3 _endPos)
        {
            float _timeStartedLerping = Time.time;
            float timeTakenDuringLerp = 1f;

            while (true)
            {
                float timeSinceStarted = Time.time - _timeStartedLerping;
                float percentageComplete = timeSinceStarted / timeTakenDuringLerp;

                _obj.transform.localPosition = Vector3.Lerp(_startPos, _endPos, percentageComplete);

                if (percentageComplete >= 1.0f)
                {
                    break;
                }
            }
            yield return null;
        }

        public static List<T> FindChilds<T>(GameObject root) where T : UnityEngine.Object
        {
            if (root == null)
                return null;

            List<T> list = new List<T>();
            Transform[] childs = root.GetComponentsInChildren<Transform>(true);

            //for (int i = 0; i < childs.Length; i++)
            //{
            //    if (childs[i].TryGetComponent(out T com))
            //    {
            //        list.Add(com);
            //    }
            //}

            foreach (Transform child in childs)
            {
                if (child.TryGetComponent(out T com))
                {
                    list.Add(com);
                }
            }

            return list;
        }

        public static T FindChild<T>(GameObject root, string name) where T : UnityEngine.Object
        {
            if (root == null)
                return null;

            List<T> list = new List<T>();
            for (int i = 0; i < root.transform.childCount; i++)
            {
                if (root.TryGetComponent(out T com))
                {
                    list.Add(com);
                }
            }

            return list.Where(a => a.name == name) as T;
        }

        public static Component Findchild(GameObject root, Type type, string name)
        {
            if (root == null)
                return null;

            Transform[] childs = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in childs)
            {
                Component component = child.GetComponent(type);
                if(child.name == name && component != null)
                {
                    return component;
                }
            }

            return null;
        }

        public static T GetOrAddComponent<T>(GameObject obj) where T : UnityEngine.Component
        {
            if (obj.TryGetComponent<T>(out T com))
            {
                return com;
            }

            return obj.AddComponent<T>();
        }

        public static List<T> FindAllObjects<T>() where T : UnityEngine.Object
        {
            List<T> objects = new List<T>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                var rootObject = scene.GetRootGameObjects();
                for (int j = 0; j < rootObject.Length; j++)
                {
                    var go = rootObject[j];
                    objects.AddRange(go.GetComponentsInChildren<T>(true));
                }

            }
            return objects;
        }
    }

    public class MultiKeyDictionary<K1, K2, V> : Dictionary<K1, Dictionary<K2, V>>
    {
        public V this[K1 key1, K2 key2]
        {
            get
            {
                if (!ContainsKey(key1) || !this[key1].ContainsKey(key2))
                    throw new ArgumentOutOfRangeException();
                return base[key1][key2];
            }
            set
            {
                if (!ContainsKey(key1))
                    this[key1] = new Dictionary<K2, V>();
                this[key1][key2] = value;
            }
        }

        public void Add(K1 key1, K2 key2, V value)
        {
            if (!ContainsKey(key1))
                this[key1] = new Dictionary<K2, V>();
            this[key1][key2] = value;
        }

        public bool ContainsKey(K1 key1, K2 key2)
        {
            return base.ContainsKey(key1) && this[key1].ContainsKey(key2);
        }

        public new IEnumerable<V> Values
        {
            get
            {
                return from baseDict in base.Values
                       from baseKey in baseDict.Keys
                       select baseDict[baseKey];
            }
        }
    }
}
