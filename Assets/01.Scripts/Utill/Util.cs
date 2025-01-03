using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using System.Net.Sockets;
using System.Net;

#region Common Types
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
#endregion

namespace Util
{
    public delegate void Event();

    public enum AniState
    {
        OnStart = 0,
        OnEnd = 1,
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
    }
}

namespace Util.Math
{
    public static class MathUtils
    {
        public static T RandomEnum<T>()
        {
            var enumValues = Enum.GetValues(enumType: typeof(T));
            return (T)enumValues.GetValue(Random.Range(0, enumValues.Length));
        }

        /// <summary>
        /// 지정된 범위 내에서 랜덤한 Vector3를 생성합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomVector3(float minRange, float maxRange)
        {
            return new Vector3(
                Random.Range(minRange, maxRange),
                Random.Range(minRange, maxRange),
                Random.Range(minRange, maxRange)
            );
        }

        /// <summary>
        /// 지정된 범위 내에서 각 축별로 다른 범위를 가진 랜덤한 Vector3를 생성합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomVector3(Vector3 minRange, Vector3 maxRange)
        {
            return new Vector3(
                Random.Range(minRange.x, maxRange.x),
                Random.Range(minRange.y, maxRange.y),
                Random.Range(minRange.z, maxRange.z)
            );
        }

        /// <summary>
        /// 지정된 범위 내에서 2D 평면상의 랜덤한 Vector2를 생성합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RandomVector2(float minRange, float maxRange)
        {
            return new Vector2(
                Random.Range(minRange, maxRange),
                Random.Range(minRange, maxRange)
            );
        }

        /// <summary>
        /// 지정된 범위 내에서 각 축별로 다른 범위를 가진 랜덤한 Vector2를 생성합니다.
        /// </summary>
        /// <param name="minRange">각 축의 최소 범위</param>
        /// <param name="maxRange">각 축의 최대 범위</param>
        /// <returns>랜덤 Vector2</returns>
        public static Vector2 RandomVector2(Vector2 minRange, Vector2 maxRange)
        {
            return new Vector2(
                Random.Range(minRange.x, maxRange.x),
                Random.Range(minRange.y, maxRange.y)
            );
        }

        /// <summary>
        /// 값을 다른 범위로 재매핑합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
        }

        /// <summary>
        /// 입력값을 출력 범위로 매핑합니다.
        /// </summary>
        public static float CalculateMappedValue(float input, float minInput, float maxInput, float minOutput, float maxOutput)
        {
            float normalized = (input - minInput) / (maxInput - minInput);
            return Mathf.Lerp(maxOutput, minOutput, normalized);
        }
    }
}

namespace Util.Component
{
    public static class ComponentUtils
    {
        /// <summary>
        /// 컴포넌트 자동 주입을 처리합니다.
        /// </summary>
        public static void InjectionComponents(object root)
        {
            Type type = root.GetType();
            MonoBehaviour script = root as MonoBehaviour;
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var item in fields)
            {
                var attribute = (FindComonenetAtrribute)item.GetCustomAttribute(typeof(FindComonenetAtrribute));
                if (attribute == null) continue;

                Type fieldType = item.FieldType;
                UnityEngine.Transform tr = script.transform.Find(attribute._gameObjectName);
                UnityEngine.Component component = tr.GetComponent<UnityEngine.Component>();

                if (component == null)
                {
                    Log.Error($"Component {fieldType.Name} of object {attribute._gameObjectName} does not exist.");
                    continue;
                }

                item.SetValue(script, component);
            }
        }

        /// <summary>
        /// GameObject에서 컴포넌트를 가져오거나 없으면 추가합니다.
        /// </summary>
        public static T GetOrAddComponent<T>(UnityEngine.GameObject obj) where T : UnityEngine.Component
        {
            return obj.TryGetComponent<T>(out T com) ? com : obj.AddComponent<T>();
        }
    }
}

namespace Util.Animation
{
    public static class AnimationUtils
    {
        /// <summary>
        /// 애니메이터 이벤트를 바인딩합니다.
        /// </summary>
        public static void BindAnimatorEvent(Animator animator, string animName, Util.AniState state, Event evt)
        {
            CoroutineHelper.StartCoroutine(BindAnimatorEventRoutine(animator, animName, state, evt));
        }

        private static IEnumerator BindAnimatorEventRoutine(Animator animator, string animName, Util.AniState state, Event evt)
        {
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).IsName(animName) &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= (int)state
            );
            evt?.Invoke();
        }
    }
}

namespace Util.Transform
{
    public static class TransformUtils
    {
        /// <summary>
        /// Transform의 위치를 보간합니다.
        /// </summary>
        public static void LerpPosition(UnityEngine.Transform obj, Vector3 startPos, Vector3 endPos)
        {
            CoroutineHelper.StartCoroutine(LerpPositionRoutine(obj, startPos, endPos));
        }

        private static IEnumerator LerpPositionRoutine(UnityEngine.Transform obj, Vector3 startPos, Vector3 endPos)
        {
            float timeStarted = Time.time;
            float duration = 1f;

            while (true)
            {
                float timeSinceStarted = Time.time - timeStarted;
                float progress = timeSinceStarted / duration;

                obj.position = Vector3.Lerp(startPos, endPos, progress);

                if (progress >= 1.0f)
                    break;

                yield return null;
            }
        }

        /// <summary>
        /// RectTransform의 위치를 보간합니다.
        /// </summary>
        public static void LerpRectPosition(RectTransform obj, Vector3 startPos, Vector3 endPos)
        {
            CoroutineHelper.StartCoroutine(LerpRectPositionRoutine(obj, startPos, endPos));
        }

        private static IEnumerator LerpRectPositionRoutine(RectTransform obj, Vector3 startPos, Vector3 endPos)
        {
            float timeStarted = Time.time;
            float duration = 1f;

            while (true)
            {
                float timeSinceStarted = Time.time - timeStarted;
                float progress = timeSinceStarted / duration;

                obj.localPosition = Vector3.Lerp(startPos, endPos, progress);

                if (progress >= 1.0f)
                    break;

                yield return null;
            }
        }
    }
}

namespace Util.GameObject
{
    public static class GameObjectUtils
    {
        /// <summary>
        /// 자식 브젝트에서 특정 타입의 컴포넌트들을 찾습니다.
        /// </summary>
        public static List<T> FindChilds<T>(UnityEngine.GameObject root) where T : UnityEngine.Object
        {
            if (root == null) return null;

            List<T> list = new List<T>();
            UnityEngine.Transform[] children = root.GetComponentsInChildren<UnityEngine.Transform>(true);

            foreach (UnityEngine.Transform child in children)
            {
                if (child.TryGetComponent(out T component))
                {
                    list.Add(component);
                }
            }

            return list;
        }

        /// <summary>
        /// 특정 이름을 가진 자식 컴포넌트를 찾습니다.
        /// </summary>
        public static UnityEngine.Component FindChild(UnityEngine.GameObject root, Type type, string name)
        {
            if (root == null) return null;

            UnityEngine.Transform[] children = root.GetComponentsInChildren<UnityEngine.Transform>(true);
            foreach (UnityEngine.Transform child in children)
            {
                UnityEngine.Component component = child.GetComponent(type);
                if (child.name == name && component != null)
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// 모든 씬에서 특정 타입의 오브젝트를 찾습니다.
        /// </summary>
        public static List<T> FindAllObjects<T>() where T : UnityEngine.Object
        {
            List<T> objects = new List<T>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                var rootObjects = scene.GetRootGameObjects();
                foreach (var go in rootObjects)
                {
                    objects.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
            return objects;
        }
    }
}

namespace Util.Network
{
    public static class NetworkUtils
    {
        /// <summary>
        /// 지정된 포트가 사용 중인지 확인합니다.
        /// </summary>
        /// <param name="port">확인할 포트 번호</param>
        /// <returns>포트가 사용 중이면 true, 사용 가능하면 false</returns>
        public static bool IsPortInUse(int port)
        {
            try
            {
                // TcpListener를 사용하여 포트를 열어봅니다.
                TcpListener tcpListener = new TcpListener(IPAddress.Loopback, port);
                tcpListener.Start();
                tcpListener.Stop();
                return false; // 포트가 사용되지 않으면 성공적으로 열 수 있습니다.
            }
            catch (SocketException)
            {
                return true; // 포트가 사용 중이면 예외가 발생합니다.
            }
        }

        /// <summary>
        /// 지정된 범위 내에서 사용되지 않은 랜덤 포트 번호를 반환합니다.
        /// </summary>
        /// <param name="minPort">최소 포트 번호 (기본: 49152)</param>
        /// <param name="maxPort">최대 포트 번호 (기본: 65535)</param>
        /// <returns>사용되지 않은 랜덤 포트 번호</returns>
        public static ushort GetRandomAvailablePort(int minPort = 49152, int maxPort = 65535)
        {
            ushort port = 0;
            int count = 0;

            while (true)
            {
                if(count++ > 100000)
                {
                    Log.Error("No port numbers left");
                    return default;
                }
                // 랜덤 포트 번호 생성
                port = (ushort)Random.Range(minPort, maxPort + 1);

                // 포트가 이미 사용 중인지 확인
                if (!IsPortInUse(port))
                {
                    return port;  // 사용되지 않은 포트 번호 반환
                }
            }
        }
    }
}
