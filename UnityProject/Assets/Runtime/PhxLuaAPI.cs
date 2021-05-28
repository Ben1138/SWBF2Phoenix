using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class PhxLuaAPI
{
	public class Unicode : Attribute {}

	static PhxRuntimeEnvironment ENV => PhxGameRuntime.GetEnvironment();
	static PhxRuntimeScene RTS => PhxGameRuntime.GetScene();
	static PhxGameMatch MT => PhxGameRuntime.GetMatch();
	static PhxLuaRuntime RT => PhxGameRuntime.GetLuaRuntime();
	static PhxTimerDB TDB => PhxGameRuntime.GetTimerDB();
	static Lua L => RT.GetLua();


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
		// I think this is the "galactic conquest" special items setup (e.g. sabotage, extra health, ...)
		// see setup_teams.lua:12
		return false;
	}

	public static int ScriptCB_LoadMissionSetup()
	{
		// I think this is the "galactic conquest" special items setup (e.g. sabotage, extra health, ...)
		// see setup_teams.lua:13
		return 0;
	}

	public static void ScriptCB_SetDopplerFactor(float factor)
    {

    }

	public static int ScriptCB_GetNumCameras()
    {
		// Apparently used to determine number of view ports in split screen
		// 1 = no split screen
		return 1;
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

		string localized = ENV.GetLocalized(localizePath);
		string res = SWBFHelpers.Format(localized, placements);
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

	public static void SetAIVehicleNotifyRadius(float radius)
    {

    }

	public static void SetSpawnDelay(float unkwn1, float unkwn2)
	{
		
	}

	public static void SetHeroClass(int teamIdx, string className)
	{
		MT.SetHeroClass(teamIdx, className);
	}

	public static void SetTeamAsEnemy(int teamIdx1, int teamIdx2)
	{
		MT.SetTeamAsEnemy(teamIdx1, teamIdx2);
	}

	public static void SetTeamAsFriend(int teamIdx1, int teamIdx2)
	{
		MT.SetTeamAsFriend(teamIdx1, teamIdx2);
	}

	public static void SetTeamName(int teamIdx, string name)
	{
		MT.SetTeamName(teamIdx, name);
	}

	public static void SetUnitCount(int teamIdx, int numUnits)
	{
		MT.SetUnitCount(teamIdx, numUnits);
	}

	public static void AddUnitClass(int teamIdx, string className, int unitCount)
	{
		MT.AddUnitClass(teamIdx, className, unitCount);
	}

	public static void AddUnitClass(int teamIdx, string className, int unitCount, int unkwn1)
	{
		// TODO: unkwn1
		MT.AddUnitClass(teamIdx, className, unitCount);
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

	public static void SetMinPlayerFlyHeight(float height)
    {

    }

	public static void SetMaxPlayerFlyHeight(float height)
	{
		
	}

	public static void SetAttackingTeam(int teamIdx)
	{
		
	}

	public static void AddCameraShot(float quatX, float quatY, float quatZ, float quatW, float posX, float posY, float posZ)
	{
		RTS.AddCameraShot(quatX, quatY, quatZ, quatW, posX, posY, posZ);
	}

	public static void SetTeamIcon(int teamIdx, string iconName, string hudIconName, string flagIconName)
	{
		
	}

	public static void SetTeamIcon(int teamIdx, string iconName)
	{
		MT.SetTeamIcon(teamIdx, iconName);
	}

	public static void SetBleedRate(int teamIdx, float rate)
	{
		// a.k.a. reinforcement losses per second (e.g. 0.33)
	}

	public static int GetReinforcementCount(int teamIdx)
	{
		return MT.GetReinforcementCount(teamIdx);
	}

	public static void SetReinforcementCount(int teamIdx, int count)
	{
		MT.SetReinforcementCount(teamIdx, count);
	}

	public static void AddReinforcements(int teamIdx, int count)
	{
		MT.AddReinforcements(teamIdx, count);
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

	public static void BroadcastVoiceOver(string voName, int teamIdx)
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

	public static void ScaleSoundParameter(string soundName, string paramName, float scale)
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

	public static void AddLandingRegion(string regionName)
    {

    }

	public static void SetProperty(string instName, string propName, object propValue)
	{
		PhxRuntimeScene scene = PhxGameRuntime.GetScene();
		scene?.SetProperty(instName, propName, propValue);
	}

	public static void SetClassProperty(string className, string propName, object propValue)
	{
		PhxRuntimeScene scene = PhxGameRuntime.GetScene();
		scene?.SetClassProperty(className, propName, propValue);
	}

	public static void SetObjectTeam(string instName, int teamIdx)
    {
		SetProperty(instName, "Team", teamIdx);
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

	public static void SetUberMode(int enable)
	{
		
	}

	public static void SetGroundFlyerMap(int enable)
    {

    }

	public static void SetDefenderSnipeRange(float range)
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

	public static void MissionVictory(object teams)
    {
		// teams can either be one int (1), or a table of ints {1,2}
	}

	public static int? CreateTimer(string timerName)
    {
		return TDB.CreateTimer(timerName);
    }

	public static void DestroyTimer(int? timer)
	{
		TDB.DestroyTimer(timer);
	}

	public static void StartTimer(int? timer)
	{
		TDB.StartTimer(timer);
	}

	public static void StopTimer(int? timer)
    {
		TDB.StopTimer(timer);
	}

	public static void SetTimerRate(int? timer, float rate)
	{
		TDB.SetTimerRate(timer, rate);
	}

	public static void SetTimerValue(int? timer, float value)
    {
		TDB.SetTimerValue(timer, value);
	}

	public static void ShowTimer(int? timer)
    {
		// only one neutral timer can be shown at a time.
		// when called with 'nil', hide the timer
    }

	public static void SetDefeatTimer(int? timer, int teamIdx)
    {
		// only one defeat timer can be shown at a time.
	}

	public static void SetVictoryTimer(int? timer, int teamIdx)
	{
		// only one victory timer can be shown at a time.
	}

	public static int? FindTimer(string timerName)
    {
		return TDB.FindTimer(timerName);
    }

	public static int GetObjectTeam(string objName)
    {
		return 0;
    }

	public static int GetObjectTeam(int? objPtr)
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

	public static string GetEntityName(object objPtr)
    {
		return null;
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

	public static void MapRemoveEntityMarker(object objectPtr, int teamIdx)
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

	public static void SpaceAssaultEnable(bool enable)
    {

    }

	public static void SpaceAssaultSetupBitmaps(
		object shipBitmapATT, object shipBitmapDEF,
		object shieldBitmapATT, object shieldBitmapDEF,
		object criticalSystemBitmapATT, object criticalSystemBitmapDEF)
	{

	}

	public static void SpaceAssaultAddCriticalSystem(string name, float pointValue, float hudPosX, float hudPosY)
    {
		SpaceAssaultAddCriticalSystem(name, pointValue, hudPosX, hudPosY, true);
	}

	public static void SpaceAssaultAddCriticalSystem(string name, float pointValue, float hudPosX, float hudPosY, bool displayHudMarker)
	{

	}

	public static void SpaceAssaultLinkCriticalSystems(object obj)
    {

    }

	public static void AddSpaceAssaultDestroyPoints(object killer, string instName)
    {

    }

	public static void EnableBuildingLockOn(string instName, bool lockOn)
    {

    }

	public static void DisableSmallMapMiniMap()
    {

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
		PhxGameRuntime.Instance.RegisterAddonScript(scriptName, threeLetterName);
	}




	// ===============================================================================================================
	// Event Callbacks
	// ===============================================================================================================

	public static void OnCharacterDeath(PhxLuaRuntime.LFunction callback)
    {
		
    }
	public static void OnCharacterDeathTeam(PhxLuaRuntime.LFunction callback, int teamIdx)
	{
		
	}
	public static void OnTicketCountChange(PhxLuaRuntime.LFunction callback)
	{
		// TicketCount seems to be the reinforcement count, see Objective.lua:192
	}
	public static void OnTimerElapse(PhxLuaRuntime.LFunction callback, int timer)
	{
		GameLuaEvents.Register(GameLuaEvents.Event.OnTimerElapse, callback, timer);
	}
	public static void OnEnterRegion(PhxLuaRuntime.LFunction callback, string regionName)
    {
		GameLuaEvents.Register(GameLuaEvents.Event.OnEnterRegion, callback, regionName);
	}
	public static void OnEnterRegionTeam(PhxLuaRuntime.LFunction callback, string regionName, int teamIdx)
	{
		GameLuaEvents.Register(GameLuaEvents.Event.OnEnterRegionTeam, callback, (regionName, teamIdx));
	}
	public static void OnLeaveRegion(PhxLuaRuntime.LFunction callback, string regionName)
    {
		GameLuaEvents.Register(GameLuaEvents.Event.OnLeaveRegion, callback, regionName);

	}
	public static void OnFinishCapture(PhxLuaRuntime.LFunction callback)
	{
		// callback paramters:
		// - postPtr
	}
	public static void OnFinishCaptureName(PhxLuaRuntime.LFunction callback, string cpName)
	{
		
	}
	public static void OnFinishNeutralize(PhxLuaRuntime.LFunction callback)
	{
		// callback paramters:
		// - postPtr
	}
	public static void OnCommandPostRespawn(PhxLuaRuntime.LFunction callback)
	{
		// callback paramters:
		// - postPtr
	}
	public static void OnCommandPostKill(PhxLuaRuntime.LFunction callback)
	{
		// callback paramters:
		// - postPtr
	}
	public static void OnObjectKillName(PhxLuaRuntime.LFunction callback, string objName)
	{
		
	}
	public static void OnObjectKillTeam(PhxLuaRuntime.LFunction callback, int teamIdx)
	{
		
	}
	public static void OnObjectKillClass(PhxLuaRuntime.LFunction callback, string className)
	{
		
	}
	public static void OnObjectRespawnName(PhxLuaRuntime.LFunction callback, string objName)
	{
		
	}

	public static void OnObjectDamageName(PhxLuaRuntime.LFunction callback, string objName)
    {

    }

	public static void OnTeamPointsChange(PhxLuaRuntime.LFunction callback)
    {

    }

	public static void OnTeamPointsChangeTeam(PhxLuaRuntime.LFunction callback, int teamIdx)
	{

	}
}

public static class GameLuaEvents
{
	static PhxRuntimeEnvironment ENV { get { return PhxGameRuntime.GetEnvironment(); } }
	static PhxLuaRuntime RT { get { return PhxGameRuntime.GetLuaRuntime(); } }
	static Lua L { get { return RT.GetLua(); } }

	public enum Event
	{
		OnEnterRegion,
		OnEnterRegionTeam,
		OnLeaveRegion,
		OnTimerElapse
	}

	class CallbackDict<T>
    {
		Dictionary<T, List<PhxLuaRuntime.LFunction>> Callbacks = new Dictionary<T, List<PhxLuaRuntime.LFunction>>();

		public int AddCallback(T key, PhxLuaRuntime.LFunction callback)
		{
			if (Callbacks.TryGetValue(key, out List<PhxLuaRuntime.LFunction> callbacks))
			{
				callbacks.Add(callback);
				return callbacks.Count - 1;
			}
			callbacks = new List<PhxLuaRuntime.LFunction>() { callback };
			Callbacks.Add(key, callbacks);
			return callbacks.Count - 1;
		}

		public void RemoveCallback(T key, int idx)
        {
			if (Callbacks.TryGetValue(key, out List<PhxLuaRuntime.LFunction> callbacks))
			{
				callbacks.RemoveAt(idx);
			}
		}

		public void Invoke(T key, object[] args)
        {
			if (Callbacks.TryGetValue(key, out List<PhxLuaRuntime.LFunction> callbacks))
			{
				for (int i = 0; i < callbacks.Count; ++i)
                {
					callbacks[i].Invoke(args);
				}
			}
		}
	}

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

	public static int Register(Event ev, PhxLuaRuntime.LFunction callback, object key)
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

public static class SWBFHelpers
{
	// format string using SWBFs C-style printf format (%s, ...)
	public static string Format(string fmt, params object[] args)
    {
		fmt = ConvertFormat(fmt);
		return string.Format(fmt, args);
	}

	// convert C-style printf format to C# format
	static string ConvertFormat(string swbfFormat)
	{
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
		string format = swbfFormat;
		int idx = GetNextIndex(format);
		for (int i = 0; idx >= 0; idx = GetNextIndex(format), ++i)
		{
			string sub = format.Substring(0, idx);
			sub += "{" + i + "}";
			sub += format.Substring(idx + 2, format.Length - idx - 2);
			format = sub;
		}
		return format;
	}
}