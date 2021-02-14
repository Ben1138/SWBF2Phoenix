using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class GameLuaAPI
{
	public static string ScriptCB_GetPlatform()
	{
		return "PC";
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

	public static bool ScriptCB_IsMissionSetupSaved()
	{
		return false;
	}

	public static int ScriptCB_LoadMissionSetup()
	{
		return 0;
	}

	public static string ScriptCB_getlocalizestr(string localizePath)
	{
		return localizePath;
	}

	public static byte[] ScriptCB_tounicode(string ansiString)
	{
		return Encoding.Convert(Encoding.ASCII, Encoding.Unicode, Encoding.ASCII.GetBytes(ansiString));
	}

	public static string ScriptCB_ununicode(string unicodeString)
	{
		return Encoding.ASCII.GetString(Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(unicodeString)));
	}

	public static string ScriptCB_usprintf(string[] args)
	{
		return args[0];
	}

	public static void ScriptCB_DoFile(string scriptName)
	{
		LuaRuntime runtime = GameRuntime.GetLuaRuntime();
		if (runtime == null)
		{
			Debug.LogError("ScriptCB_DoFile has been called without the LuaRuntime present!");
			return;
		}
		// TODO: execute lvl script
	}

	public static void SetPS2ModelMemory()
	{
		
	}

	public static void StealArtistHeap()
	{
		
	}

	public static void SetTeamAggressiveness()
	{
		
	}

	public static void SetMemoryPoolSize()
	{
		
	}

	public static void ClearWalkers()
	{
		
	}

	public static void AddWalkerType()
	{
		
	}

	public static void SetSpawnDelay()
	{
		
	}

	public static void SetHeroClass()
	{
		
	}

	public static void SetTeamAsEnemy()
	{
		
	}

	public static void SetTeamAsFriend()
	{
		
	}

	public static void SetTeamName()
	{
		
	}

	public static void SetUnitCount()
	{
		
	}

	public static void AddUnitClass()
	{
		
	}

	public static void SetDenseEnvironment()
	{
		
	}

	public static void SetMinFlyHeight()
	{
		
	}

	public static void SetMaxFlyHeight()
	{
		
	}

	public static void SetMaxPlayerFlyHeight()
	{
		
	}

	public static void SetAttackingTeam()
	{
		
	}

	public static void AddCameraShot()
	{
		
	}

	public static void SetTeamIcon()
	{
		
	}

	public static void SetBleedRate()
	{
		
	}

	public static void GetReinforcementCount()
	{
		
	}

	public static void SetReinforcementCount()
	{
		
	}

	public static void AddReinforcements()
	{
		
	}

	public static void OpenAudioStream()
	{
		
	}

	public static void AudioStreamAppendSegments()
	{
		
	}

	public static void SetBleedingVoiceOver()
	{
		
	}

	public static void SetLowReinforcementsVoiceOver()
	{
		
	}

	public static void SetOutOfBoundsVoiceOver()
	{
		
	}

	public static void SetAmbientMusic()
	{
		
	}

	public static void SetVictoryMusic()
	{
		
	}

	public static void SetDefeatMusic()
	{
		
	}

	public static void SetSoundEffect()
	{
		
	}

	public static void SetMapNorthAngle()
	{
		
	}

	public static void AISnipeSuitabilityDist()
	{
		
	}

	public static void EnableSPHeroRules()
	{
		
	}

	public static void AddDeathRegion()
	{
		
	}

	public static void SetProperty()
	{
		
	}

	public static void DisableBarriers()
	{
		
	}

	public static void PlayAnimation()
	{
		
	}

	public static void SetUberMode()
	{
		
	}

	public static float GetCommandPostBleedValue()
	{
		return 0.0f;
	}

	public static void ReadDataFile(object[] args)
	{
		string path = args[0] as string;
		Debug.LogFormat("Called ReadDataFile with {0} arguments, path '{1}'", args.Length, path);
	}

	public static void AddDownloadableContent(string threeLetterName, string scriptName, int levelMemoryModifier)
	{
		
	}
}