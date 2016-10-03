using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Text;
using UnityEngine.UI;

using UniRx;
using UniRx.Triggers;

public class TestScript : MonoBehaviour
{
	public enum CoroutineType
	{
		MonoBehaviourCoroutine,
		MainThreadCoroutine,
		SendStartCoroutine,
		MicroCoroutine,
		EndOfFrameMicroCoroutine,
		FixedUpdateMicroCoroutine,
	}

	[SerializeField] CoroutineType coroutineType;
	[SerializeField] int count = 100000;

	[Header("-------UI-------")]
	[SerializeField] Button button;

	float prevTime = 0f, nowTime = 0f;
	List<GameObject> list = new List<GameObject>();

	IEnumerator Start()
	{
		bool result = false;
		yield return Observable.Start(() => SetEvent()).StartAsCoroutine(x => result = x);

		if (result)
			Debug.Log("Init Success");
		else
			Debug.Log("Init Failure");
	}

	bool SetEvent()
	{
		button.onClick.AsObservable()
			.Where(_ => IsEmpty(list))
			.Subscribe(_ => OnClickTestStart());

		list.ObserveEveryValueChanged(_list => _list.Count)
			.Where(_length => _length == count)
			.Subscribe(_ =>
			{
				nowTime = Time.realtimeSinceStartup;
				LogTime(nowTime - prevTime);
				for (int i = 0; i < list.Count; ++i)
				{
					DestroyImmediate(list[i]);
				}
				list.Clear();
			});

		return true;
	}

	IEnumerator TestCoroutine()
	{
		GameObject go = new GameObject();
		go.transform.SetParent(transform, true);
		list.Add(go);
		yield return null;
	}

	void OnClickTestStart()
	{
		prevTime = Time.realtimeSinceStartup;

		for (int i = 0; i < count; ++i)
		{
			switch (coroutineType)
			{
				
				case CoroutineType.MonoBehaviourCoroutine:
					StartCoroutine(TestCoroutine());
					break;
				case CoroutineType.MainThreadCoroutine:
					MainThreadDispatcher.StartCoroutine(TestCoroutine());
					break;
				case CoroutineType.SendStartCoroutine:
					MainThreadDispatcher.SendStartCoroutine(TestCoroutine());
					break;
				case CoroutineType.MicroCoroutine:
					MainThreadDispatcher.StartUpdateMicroCoroutine(TestCoroutine());
					break;
				case CoroutineType.EndOfFrameMicroCoroutine:
					MainThreadDispatcher.StartEndOfFrameMicroCoroutine(TestCoroutine());
					break;
				case CoroutineType.FixedUpdateMicroCoroutine:
					MainThreadDispatcher.StartFixedUpdateMicroCoroutine(TestCoroutine());
					break;
			}
		}
	}

	bool IsEmpty<T>(this List<T> list)
	{
		return list == null || list.Count == 0;
	}

	void LogTime(float time)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(coroutineType.ToString());
		sb.Append(" -> time:");
		sb.Append(time.ToString());
		Debug.Log(sb.ToString());
	}
}