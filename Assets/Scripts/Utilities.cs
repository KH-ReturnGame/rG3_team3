using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 다기능 도구 집합 패키지 / 다른 스크립트에서도 불러와서 사용가능함!
namespace Utility.DataManagement
{
   public static class ListManagement
    {
        private class CoroutineRunner : MonoBehaviour { }
        private static CoroutineRunner _runner;
        private static CoroutineRunner Runner
        {
            get
            {
                if (_runner == null)
                {
                    GameObject obj = new GameObject("[ListManagement_Runner]");
                    _runner = obj.AddComponent<CoroutineRunner>();
                    Object.DontDestroyOnLoad(obj);
                }
                return _runner;
            }
        }


        public static string AddData(string dataName, List<string> dataList, float duration = 0f)
        {
            if (duration > 0)
            {
                Runner.StartCoroutine(ActivateTimer(dataName, dataList, duration));
                return "타이머 시작";
            }
            else
            {
                dataList.Add(dataName);
                return dataName;
            }
        }

        private static IEnumerator ActivateTimer(string dataName, List<string> dataList, float duration)
        {
            dataList.Add(dataName);
            yield return new WaitForSeconds(duration);
            dataList.Remove(dataName);
        }
    }
}