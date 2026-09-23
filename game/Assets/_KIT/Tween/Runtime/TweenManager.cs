using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KIT
{
	public class TweenManager : MonoBehaviour
	{
		private static TweenManager instance;

		internal static TweenManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GameObject("TweenManager").AddComponent<TweenManager>();
					DontDestroyOnLoad(instance);
				}

				return instance;
			}
			private set => instance = value;
		}

		[SerializeField] private int beginSize = 10;
		[SerializeField] private TweenCore[] tweens;
		[SerializeField] private List<int> activeTweens; // Chỉ chứa index của item đang chạy
		private Stack<int> freeIndices; // // Danh sách index trống
		private HashSet<int> hashRemoves; // Đánh dấu phần tử cần xóa
		private int id = 1;

		private void Awake()
		{
			Instance = this;
			tweens = new TweenCore[beginSize];
			activeTweens = new List<int>(beginSize);
			freeIndices = new Stack<int>(beginSize);
			for (int i = tweens.Length - 1; i >= 0; i--) freeIndices.Push(i);
			hashRemoves = new HashSet<int>();
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			int count = activeTweens.Count;
			for (int i = 0; i < count; i++)
			{
				int index = activeTweens[i];
				ref TweenCore core = ref tweens[index];
				if (core.IsValid)
				{
					core.Execute(deltaTime);
					if (core.IsFinished && core.IsValid)
					{
						core.IsValid = false;
						hashRemoves.Add(index);
					}
				}
			}
			
			CleanInvalidTween();
		}

		private void LateUpdate()
		{
			CleanInvalidTween();
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		internal TweenCore CreateTween(float duration, Action<float> onProgression, PlayerLoop playerLoop)
		{
			if (freeIndices.Count <= 0)
			{
				int length = tweens.Length;
				Array.Resize(ref tweens, length * 2);
				for (int i = length; i < length * 2; i++) freeIndices.Push(i);
			}

			int index = freeIndices.Pop();
			tweens[index].Create(id++, index, duration, onProgression, playerLoop);
			activeTweens.Add(index);
			return tweens[index];
		}

		internal ref TweenCore Get(int index, PlayerLoop playerLoop)
		{
			return ref tweens[index];
		}

		internal void Kill(TweenCore target)
		{
			ref TweenCore core = ref Get(target.Index, target.PlayerLoop);
			if (core.IsValid && core.ID == target.ID && core.Index == target.Index)
			{
				core.IsValid = false;
				hashRemoves.Add(core.Index);
			}
		}

		private void CleanInvalidTween()
		{
			if (hashRemoves.Count > 0)
			{
				foreach (int index in hashRemoves)
				{
					// Ý tưởng tối ưu là đảo chỗ item bị xoá về cuối, sau đó xoá item cuối (sẽ khng phải resize lại)
					activeTweens.Remove(index);
					freeIndices.Push(index);
				}

				hashRemoves.Clear();
			}
		}
	}
}