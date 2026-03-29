using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.Plugins.Payday2GSI.Prerequisites;

public class UninstallPayday2GsiModAction : PluginPrerequisiteAction
{
	public UninstallPayday2GsiModAction() : base("Uninstall Payday 2 GSI mod")
	{
	}

	public override Task Execute(CancellationToken cancellationToken)
	{
		if (!Payday2SteamFinder.TryGetInstallPath(out string payday2Path))
			return Task.CompletedTask;

		string artemisXmlPath = BLTModPrerequisite.GetArtemisXmlPath(payday2Path);
		string extractedModPath = BLTModPrerequisite.GetExtractedModPath(payday2Path);

		if (File.Exists(artemisXmlPath))
			File.Delete(artemisXmlPath);

		if (Directory.Exists(extractedModPath))
			Directory.Delete(extractedModPath, true);

		return Task.CompletedTask;
	}
}