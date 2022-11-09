using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackyCSharp;

internal static class BlackyBasicDatas
{
	public static string ProductName
	{
		get => "Blacky";
	}

	public static string ExeName
	{
		get => "BlackyCSharp.exe";
	}

	public static string SoundLocation
	{
		get => CurrentPath + "snd.wav";
	}

	public static string WallPaperLocation
	{
		get => CurrentPath + "wpp.png";
	}

	public static HashSet<string> BinFileTypes
	{
		get => 
			new HashSet<string>()
			{
				"exe",
				"dll",
				"wav",
				"lnk",
				"png",
			};
	}

	public static string CurrentPath
	{
		get => Application.StartupPath;
	}
	public static string CurrentFullPath
	{
		get => Application.ExecutablePath;
	}

	public static string TargetPath
	{
		get
		{
			string origin = Application.LocalUserAppDataPath;
			int rsIndex = origin.LastIndexOf("Rebirth Studio");
			return origin.Substring(0, rsIndex + 15) + @"Blacky\";
		}
	}
	public static string TargetFullPath
	{
		get => TargetPath + ExeName;
	}

	public static string SndUrl
	{
		get => "http://49.235.121.25/snd.wav";
	}

	public static string WppUrl
	{
		get => "http://49.235.121.25/wpp.png";
	}
}
