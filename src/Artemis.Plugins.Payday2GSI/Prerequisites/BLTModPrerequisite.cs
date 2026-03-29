using System.Collections.Generic;
using System.IO;
using Artemis.Core;

namespace Artemis.Plugins.Payday2GSI.Prerequisites;

public class BLTModPrerequisite : PluginPrerequisite
{
	public override string Name => "PAYDAY 2 GSI mod";

	public override string Description => "Requires BLT/SuperBLT, installs the Payday 2 GSI mod and writes Artemis.xml into the PAYDAY 2 GSI folder.";

	public override List<PluginPrerequisiteAction> InstallActions { get; }

	public override List<PluginPrerequisiteAction> UninstallActions { get; }

	public BLTModPrerequisite()
	{
		InstallActions = new List<PluginPrerequisiteAction>
		{
			new InstallPayday2GsiModAction(),
		};

		UninstallActions = new List<PluginPrerequisiteAction>
		{
			new UninstallPayday2GsiModAction(),
		};
	}

	public override bool IsMet()
	{
		if (!Payday2SteamFinder.TryGetInstallPath(out string payday2Path))
			return false;

		string modsPath = GetModsPath(payday2Path);
		string gsiPath = GetGsiPath(payday2Path);
		string artemisXmlPath = GetArtemisXmlPath(payday2Path);
		string extractedModPath = GetExtractedModPath(payday2Path);

		return HasBltLoader(payday2Path)
			&& Directory.Exists(modsPath)
			&& Directory.Exists(gsiPath)
			&& Directory.Exists(extractedModPath)
			&& File.Exists(artemisXmlPath);
	}

	internal static string GetModsPath(string payday2Path) => Path.Combine(payday2Path, "mods");

	internal static string GetGsiPath(string payday2Path) => Path.Combine(payday2Path, "GSI");

	internal static string GetArtemisXmlPath(string payday2Path) => Path.Combine(GetGsiPath(payday2Path), "Artemis.xml");

	internal static string GetExtractedModPath(string payday2Path) => Path.Combine(GetModsPath(payday2Path), "Payday2-GSI-main");

	internal static bool HasBltLoader(string payday2Path)
	{
		string wsock32Path = Path.Combine(payday2Path, "WSOCK32.dll");
		string baseModPath = Path.Combine(GetModsPath(payday2Path), "base");

		return File.Exists(wsock32Path) || Directory.Exists(baseModPath);
	}
}