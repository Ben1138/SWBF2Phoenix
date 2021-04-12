using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhxTimerDB
{
	// TODO: should be enough?
	const int MAX_TIMERS = 100;

	public struct PhxTimer
    {
		public string Name;
		public float Elapsed;
		public float Target;
		public float Rate;
		public bool InUse;
		public bool IsRunning;
	}
	public List<int> InUseIndices { get; private set; } = new List<int>();

	PhxTimer[] Timers = new PhxTimer[MAX_TIMERS];
	Dictionary<string, int> NameMap = new Dictionary<string, int>();


	public int? CreateTimer(string timerName)
	{
		if (NameMap.ContainsKey(timerName)) return null;

		for (int i = 0; i < MAX_TIMERS; i++)
        {
			if (!Timers[i].InUse)
			{
				Timers[i].Name = timerName;
				Timers[i].Elapsed = 0.0f;
				Timers[i].Rate = 1.0f;
				Timers[i].InUse = true;
				Timers[i].IsRunning = false;
				NameMap.Add(timerName, i);
				InUseIndices.Add(i);
				return i;
			}
        }
		return null;
	}

	public void DestroyTimer(int? timer)
	{
		if (!CheckTimerIdx(timer)) return;
		int idx = (int)timer;
		Timers[idx].InUse = false;
		NameMap.Remove(Timers[idx].Name);
		InUseIndices.Remove(idx);
	}

	public void StartTimer(int? timer)
	{
		if (!CheckTimerIdx(timer)) return;
		Timers[(int)timer].IsRunning = true;
	}

	public void StopTimer(int? timer)
	{
		if (!CheckTimerIdx(timer)) return;
		Timers[(int)timer].IsRunning = false;
	}

	public void SetTimerRate(int? timer, float rate)
	{
		if (!CheckTimerIdx(timer)) return;
		Timers[(int)timer].Rate = rate;
	}

	public void SetTimerValue(int? timer, float value)
	{
		if (!CheckTimerIdx(timer)) return;
		int idx = (int)timer;
		Timers[idx].Elapsed = 0.0f;
		Timers[idx].Target = value;
	}

	public int? FindTimer(string name)
    {
		if (NameMap.TryGetValue(name, out int idx))
        {
			return idx;
        }
		return null;
    }


	public void Update(float deltaTime)
    {
		for (int i = 0; i < InUseIndices.Count; ++i)
        {
			int idx = InUseIndices[i];
			if (Timers[idx].IsRunning)
            {
				Timers[idx].Elapsed += Timers[idx].Rate * deltaTime;

				if (Timers[idx].Elapsed >= Timers[idx].Target)
                {
					GameLuaEvents.Invoke(GameLuaEvents.Event.OnTimerElapse, idx);
					Timers[idx].IsRunning = false;
				}
            }
		}
    }

	public void GetTimer(int idx, out PhxTimer timer)
    {
		timer = Timers[idx];
	}


	bool CheckTimerIdx(int? idx)
    {
		if (idx == null) return false;
		if (idx < 0 || idx >= MAX_TIMERS)
        {
			Debug.LogErrorFormat($"Timer index '{idx}' is out of range '{MAX_TIMERS}'!");
			return false;
        }
		return true;
    }
}
