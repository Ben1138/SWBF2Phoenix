using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class GameLuaAPI
{
	public class Unicode : Attribute {}

	static RuntimeEnvironment ENV { get { return GameRuntime.GetEnvironment(); } }
	static LuaRuntime RT { get { return GameRuntime.GetLuaRuntime(); } }
	static Lua L { get { return RT.GetLua(); } }


	public static string ScriptCB_GetPlatform()
	{
		// "PC", "XBox", "PS2"
		return "PC";
	}

	public static (float, float, float, float) ScriptCB_GetScreenInfo()
    {
		return (Screen.width, Screen.height, 0.0f, Screen.width / Screen.height);
	}

	public static string ScriptCB_GetOnlineService()
	{
		return "GameSpy";
	}

	public static (string, int) ScriptCB_GetLanguage()
	{
		// TODO: second parameter '0' needs verification
		return ("english", 0);
	}

	public static int ScriptCB_GetCONMaxTimeLimit()
	{
		return 0;
	}

	public static int ScriptCB_GetCONNumBots()
	{
		return 0;
	}

	public static void ScriptCB_SetNumBots(int numBots)
	{
		
	}

	public static int ScriptCB_GetCTFNumBots()
    {
		return 0;
    }

	public static float ScriptCB_GetCTFMaxTimeLimit()
    {
		return 0.0f;
    }

	public static int ScriptCB_GetCTFCaptureLimit()
	{
		return 0;
	}

	public static bool ScriptCB_IsMissionSetupSaved()
	{
		return false;
	}

	public static int ScriptCB_LoadMissionSetup()
	{
		return 0;
	}

	[return: Unicode]
	public static string ScriptCB_getlocalizestr(string localizePath)
	{
		return ENV.GetLocalized(localizePath, false);
	}

	[return: Unicode]
	public static string ScriptCB_getlocalizestr(string localizePath, bool bReturnNullIfNotFound)
	{
		return ENV.GetLocalized(localizePath, bReturnNullIfNotFound);
	}

	[return: Unicode]
	public static string ScriptCB_tounicode(string ansiString)
	{
		string res = Encoding.Unicode.GetString(Encoding.Convert(Encoding.ASCII, Encoding.Unicode, Encoding.ASCII.GetBytes(ansiString)));
		return res;
	}

	public static string ScriptCB_ununicode([Unicode] string unicodeString)
	{
		string res = Encoding.ASCII.GetString(Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(unicodeString)));
		return res;
	}

	[return: Unicode]
	public static string ScriptCB_usprintf([Unicode] object[] args)
	{
		Debug.Assert(args.Length > 1);

		// first argument is the localize path, the rest are format placements
		string localizePath = args[0] as string;
		object[] placements = new object[args.Length - 1];
		Array.Copy(args, 1, placements, 0, placements.Length);

		int GetNextIndex(string format)
        {
			int idx = format.IndexOf("%s");
			if (idx >= 0) return idx;

			idx = format.IndexOf("%i");
			if (idx >= 0) return idx;

			idx = format.IndexOf("%d");
			if (idx >= 0) return idx;

			idx = format.IndexOf("%f");
			if (idx >= 0) return idx;

			return -1;
		}

		// convert C-style printf format to C# format
		string format = ENV.GetLocalized(localizePath);
		int idx = GetNextIndex(format);
		for (int i = 0; idx >= 0; idx = GetNextIndex(format), ++i)
		{
			string sub = format.Substring(0, idx);
			sub += "{" + i + "}";
			sub += format.Substring(idx + 2, format.Length - idx - 2);
			format = sub;
		}

		string res = string.Format(format, placements);
		return res;
	}

	public static void ScriptCB_DoFile(string scriptName)
	{
		ENV.Execute(scriptName);
	}

	public static void SetPS2ModelMemory(int PS2Mem)
	{
		
	}

	public static void StealArtistHeap(int numBytes)
	{
		
	}

	public static void SetTeamAggressiveness(int teamIdx, float aggr)
	{
		
	}

	public static void SetMemoryPoolSize(string poolName, int size)
	{
		
	}

	public static void ClearWalkers()
	{
		
	}

	public static void AddWalkerType(int pairOfLegs, int unkwn1)
	{
		
	}
	public static int AddAIGoal(int teamIdx, string goalName, float goalWeight)
	{
		return 0;
	}

	public static int AddAIGoal(int teamIdx, string goalName, float goalWeight, string captureRegion)
	{
		return 0;
	}

	public static int AddAIGoal(int teamIdx, string goalName, float goalWeight, string captureRegion, int flagPtr)
	{
		return 0;
	}

	public static void ClearAIGoals(int teamIdx)
    {

    }

	public static void SetSpawnDelay(float unkwn1, float unkwn2)
	{
		
	}

	public static void SetHeroClass(int teamIdx, string className)
	{
		
	}

	public static void SetTeamAsEnemy(int teamIdx1, int teamIdx2)
	{
		
	}

	public static void SetTeamAsFriend(int teamIdx1, int teamIdx2)
	{
		
	}

	public static void SetTeamName(int teamIdx, string name)
	{
		
	}

	public static void SetUnitCount(int teamIdx, int numUnits)
	{
		
	}

	public static void AddUnitClass(int teamIdx, string className, int unitCount)
	{

	}

	public static void AddUnitClass(int teamIdx, string className, int unitCount, int unkwn1)
	{
		
	}

	public static void SetDenseEnvironment(string isDense)
	{
		// seems to be always called with string "false"
	}

	public static void SetMinFlyHeight(float height)
	{
		
	}

	public static void SetMaxFlyHeight(float height)
	{
		
	}

	public static void SetMaxPlayerFlyHeight(float height)
	{
		
	}

	public static void SetAttackingTeam(int teamIdx)
	{
		
	}

	public static void AddCameraShot(object[] args)
	{
		
	}

	public static void SetTeamIcon(int teamIdx, string iconName, string hudIconName, string flagIconName)
	{
		
	}

	public static void SetTeamIcon(int teamIdx, string iconName)
	{

	}

	public static void SetBleedRate(int teamIdx, float rate)
	{
		
	}

	public static void GetReinforcementCount()
	{
		
	}

	public static void SetReinforcementCount(int teamIdx, int count)
	{
		
	}

	public static void AddReinforcements()
	{
		
	}

	public static int OpenAudioStream(string lvlPath, string streamName)
	{
		// TODO: what exactly does this function return? An audio stream object?
		return 0;
	}

	public static void AudioStreamAppendSegments(string lvlPath, string voiceOverName, int audioStream)
	{
		
	}

	public static void SetBleedingVoiceOver(int teamIdx1, int teamIdx2, string voiceOverName, int unkwn1)
	{
		
	}

	public static void SetLowReinforcementsVoiceOver(int teamIdx1, int teamIdx2, string voiceOverName, float unkwn1, int unkwn2)
	{
		
	}

	public static void SetOutOfBoundsVoiceOver(int teamIdx, string soundName)
	{
		
	}

	public static void SetAmbientMusic(int teamIdx, float unkwn1, string musicName, int unkwn2, int unkwn3)
	{
		
	}

	public static void SetVictoryMusic(int teamIdx, string soundName)
	{
		
	}

	public static void SetDefeatMusic(int teamIdx, string soundName)
	{
		
	}

	public static void SetSoundEffect(string eventName, string soundName)
	{
		
	}

	public static void SetMapNorthAngle(int unkwn1)
	{

	}

	public static void SetMapNorthAngle(float angle, int unkwn1)
	{
		
	}

	public static void AISnipeSuitabilityDist(float distance)
	{
		
	}

	public static void EnableSPHeroRules()
	{
		
	}

	public static void AddDeathRegion(string regionName)
	{
		
	}

	public static void SetProperty(string instName, string propName, object propValue)
	{
		RuntimeScene scene = GameRuntime.GetScene();
		scene?.SetProperty(instName, propName, propValue);
	}

	public static void SetClassProperty(string className, string propName, object propValue)
	{
		RuntimeScene scene = GameRuntime.GetScene();
		scene?.SetClassProperty(className, propName, propValue);
	}

	public static void DisableBarriers(string barrierName)
	{
		
	}

	public static void PlayAnimation(string animName)
	{
		
	}

	public static void PlayAnimationFromTo(string animName, float start, float end)
	{

	}

	public static void PauseAnimation(string animName)
    {

    }

	public static void RewindAnimation(string animName)
	{

	}

	public static void BlockPlanningGraphArcs(string planNodeName)
	{

	}

	public static void BlockPlanningGraphArcs(int planNode)
	{

	}

	public static void UnblockPlanningGraphArcs(string planNodeName)
    {

    }

	public static void UnblockPlanningGraphArcs(int planNode)
	{

	}

	public static void SetUberMode()
	{
		
	}

	public static float GetCommandPostBleedValue(string cpName, int teamIdx)
	{
		return 0.0f;
	}

	public static void AICanCaptureCP(string cpName, int teamIdx, bool canCapture)
    {

    }

	public static void MapHideCommandPosts()
    {
		MapHideCommandPosts(true);
	}

	public static void MapHideCommandPosts(bool hide)
	{

	}

	public static void SetFlagGameplayType(string typeName)
    {

    }

	public static void SetAIViewMultiplier(float multiplier)
    {

    }

	public static void AddMissionObjective(int teamIdx, string localizePath)
	{

	}

	public static void AddMissionObjective(int teamIdx, string colorName, string localizePath)
	{

	}

	public static void ActivateObjective(string objectiveName)
    {

    }

	public static int CreateTimer(string timerName)
    {
		return 0;
    }

	public static void StopTimer(int timer)
    {

    }

	public static void SetTimerRate(int timer, float rate)
	{
		
	}

	public static void SetTimerValue(int timer, float value)
    {

    }

	public static void StartTimer(int timer)
    {

    }

	public static int GetObjectTeam(string objName)
    {
		return 0;
    }

	public static int GetObjectTeam(int objPtr)
	{
		return 0;
	}

	public static bool IsObjectAlive(string objName)
	{
		return false;
	}

	public static void KillObject(string objName)
    {

    }

	public static void SetNumBirdTypes(int num)
    {

    }

	public static void SetBirdType(int birdIdx, float unkwn1, string typeName)
    {

    }

	public static void SetNumFishTypes(int num)
    {

    }

	public static void SetFishType(int fishIdx, float unkwn1, string typeName)
	{

	}

	public static void SetAllowBlindJetJumps(int num)
	{

	}

	public static void SetAIDamageThreshold(string objName, float threshold)
    {

    }

	public static void SetParticleLODBias(int bias)
    {

    }

	public static void SetMaxCollisionDistance(float distance)
	{

	}

	public static void SetWorldExtents(float distance)
	{

	}

	public static int GetRegion(string regionName)
    {
		return 0;
    }

	public static string GetRegionName(int region)
	{
		return "";
	}

	public static void ActivateRegion(string regionName)
	{

    }

	public static void DeactivateRegion(string regionName)
	{

	}

	public static void ReleaseEnterRegion(int regionEventRef)
    {

    }

	public static void ShowTeamPoints(int teamIdx, bool show)
    {

    }

	public static int GetObjectPtr(string objName)
    {
		return 0;
    }

	public static void MapAddEntityMarker(string objName, string iconName, float size, int teamIdx, string color, bool unkwn1)
    {

    }

	public static void MapAddEntityMarker(string objName, string iconName, float size, int teamIdx, string color, bool unkwn1, bool unkwn2, bool unkwn3)
	{

	}

	public static void MapAddEntityMarker(string objName, string iconName, float size, int teamIdx, string color, bool unkwn1, bool unkwn2, bool unkwn3, bool unkwn4)
	{

	}

	public static void MapRemoveRegionMarker(int region)
	{

	}

	public static void MapRemoveRegionMarker(string regionName)
    {

    }

	public static int GetFlagCarrier(string flagName)
    {
		return 0;
    }

	public static void ReadDataFile(object[] args)
	{
		// NOTE: ReadDataFile has dynamic parameters and can be either called like:
		// - ReadDataFile("file.lvl", "subLVL1", "subLVL2", ...)
		// - ReadDataFile("file.lvl;subLVL1;subLVL2")
		// or potentially as a mixture. Though I personally didn't see a mixture yet.

		string path = "";

		List<string> subLVLs = new List<string>();
		for (int i = 0; i < args.Length; ++i)
        {
			string arg = (string)args[i];
			string[] splits = arg.Split(';');
			subLVLs.AddRange(splits);

			if (i == 0)
            {
				path = splits[0];
				subLVLs.RemoveAt(0);
			}
		}

		bool bForceLocal = path.StartsWith("dc:", StringComparison.InvariantCultureIgnoreCase);
		if (bForceLocal)
        {
			path = path.Remove(0, 3);
		}

		//Debug.LogFormat("Called ReadDataFile with {0} arguments, path '{1}'", subLVLs.Count, path);
		ENV.ScheduleLVLRel(path, subLVLs.ToArray(), bForceLocal);
	}

	public static void AddDownloadableContent(string threeLetterName, string scriptName, int levelMemoryModifier)
	{
		GameRuntime.Instance.RegisterAddonScript(scriptName, threeLetterName);
	}

	// event callbacks

	public static void OnCharacterDeath(LuaRuntime.LFunction callback)
    {
		// TODO: store callback in a list and execute it when event occours
    }
	public static void OnCharacterDeathTeam(LuaRuntime.LFunction callback, int teamIdx)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnTicketCountChange(LuaRuntime.LFunction callback)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnTimerElapse(LuaRuntime.LFunction callback, int timer)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnEnterRegion(LuaRuntime.LFunction callback, string regionName)
    {
		GameLuaEvents.Register(GameLuaEvents.Event.OnEnterRegion, callback, regionName);
	}
	public static void OnEnterRegionTeam(LuaRuntime.LFunction callback, string regionName, int teamIdx)
	{
		GameLuaEvents.Register(GameLuaEvents.Event.OnEnterRegionTeam, callback, (regionName, teamIdx));
	}
	public static void OnLeaveRegion(LuaRuntime.LFunction callback, string regionName)
    {
		GameLuaEvents.Register(GameLuaEvents.Event.OnLeaveRegion, callback, regionName);

	}
	public static void OnFinishCapture(LuaRuntime.LFunction callback)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnFinishCaptureName(LuaRuntime.LFunction callback, string cpName)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnFinishNeutralize(LuaRuntime.LFunction callback)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnCommandPostRespawn(LuaRuntime.LFunction callback)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnCommandPostKill(LuaRuntime.LFunction callback)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnObjectKillName(LuaRuntime.LFunction callback, string objName)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnObjectKillTeam(LuaRuntime.LFunction callback, int teamIdx)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnObjectKillClass(LuaRuntime.LFunction callback, string className)
	{
		// TODO: store callback in a list and execute it when event occours
	}
	public static void OnObjectRespawnName(LuaRuntime.LFunction callback, string objName)
	{
		// TODO: store callback in a list and execute it when event occours
	}
}

public static class GameLuaEvents
{
	static RuntimeEnvironment ENV { get { return GameRuntime.GetEnvironment(); } }
	static LuaRuntime RT { get { return GameRuntime.GetLuaRuntime(); } }
	static Lua L { get { return RT.GetLua(); } }

	public enum Event
	{
		OnEnterRegion,
		OnEnterRegionTeam,
		OnLeaveRegion
	}

	class CallbackDict<T>
    {
		Dictionary<T, List<LuaRuntime.LFunction>> Callbacks = new Dictionary<T, List<LuaRuntime.LFunction>>();

		public int AddCallback(T key, LuaRuntime.LFunction callback)
		{
			if (Callbacks.TryGetValue(key, out List<LuaRuntime.LFunction> callbacks))
			{
				callbacks.Add(callback);
				return callbacks.Count - 1;
			}
			callbacks = new List<LuaRuntime.LFunction>() { callback };
			Callbacks.Add(key, callbacks);
			return callbacks.Count - 1;
		}

		public void RemoveCallback(T key, int idx)
        {
			if (Callbacks.TryGetValue(key, out List<LuaRuntime.LFunction> callbacks))
			{
				callbacks.RemoveAt(idx);
			}
		}

		public void Invoke(T key, object[] args)
        {
			if (Callbacks.TryGetValue(key, out List<LuaRuntime.LFunction> callbacks))
			{
				for (int i = 0; i < callbacks.Count; ++i)
                {
					callbacks[i].Invoke(args);
				}
			}
		}
	}

	//class CallbackDict<T1, T2>
	//{
	//	Dictionary<T2, CallbackDict<T1>> Callbacks = new Dictionary<T2, CallbackDict<T1>>();

	//	public int AddCallback(T1 key1, T2 key2, Lua.Function callback)
	//	{
	//		CallbackDict<T1> innerDict = null;
	//		if (Callbacks.TryGetValue(key2, out innerDict))
	//		{
	//			return innerDict.AddCallback(key1, callback);
	//		}

	//		innerDict = new CallbackDict<T1>();
	//		Callbacks.Add(key2, innerDict);
	//		return innerDict.AddCallback(key1, callback);
	//	}

	//	public void RemoveCallback(T1 key1, T2 key2, int idx)
 //       {
	//		CallbackDict<T1> innerDict = null;
	//		if (Callbacks.TryGetValue(key2, out innerDict))
	//		{
	//			innerDict.RemoveCallback(key1, idx);
	//		}
	//	}

	//	public void Invoke(T1 key1, T2 key2)
	//	{
	//		if (Callbacks.TryGetValue(key2, out CallbackDict<T1> callbacks))
	//		{
	//			callbacks.Invoke(key1);
	//		}
	//	}
	//}

	//class CallbackDict<T1, T2, T3>
	//{
	//	CallbackDict<T1, CallbackDict<T2, T3>> Callbacks = new CallbackDict<T1, CallbackDict<T2, T3>>();
	//}

	static Dictionary<Event, CallbackDict<object>> Callbacks = new Dictionary<Event, CallbackDict<object>>();
	static Dictionary<int, (Event, int)> GlobalIdxDict = new Dictionary<int, (Event, int)>();
	static int GlobalIdxCounter = 0;

	static CallbackDict<object> Get(Event ev)
	{
		if (Callbacks.TryGetValue(ev, out CallbackDict<object> inner))
		{
			return inner;
		}

		inner = new CallbackDict<object>();
		Callbacks.Add(ev, inner);
		return inner;
	}

	public static void Clear()
    {
		Callbacks.Clear();
		GlobalIdxDict.Clear();
	}

	public static int Register(Event ev, LuaRuntime.LFunction callback, object key)
	{
		int localIdx = Get(ev).AddCallback(key, callback);
		GlobalIdxDict.Add(GlobalIdxCounter, (ev, localIdx));
		return GlobalIdxCounter++;
	}

	// To be called by environment
	public static void Invoke(Event ev, object key, params object[] eventArgs)
    {
		Get(ev).Invoke(key, eventArgs);
	}
}