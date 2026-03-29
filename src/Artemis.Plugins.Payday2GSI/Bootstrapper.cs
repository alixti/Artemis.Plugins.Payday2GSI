using Artemis.Core;
using Artemis.Plugins.Payday2GSI.Prerequisites;

namespace Artemis.Plugins.Payday2GSI;

public class Bootstrapper : PluginBootstrapper
{
	public override void OnPluginLoaded(Plugin plugin)
	{
		AddPluginPrerequisite(new BLTModPrerequisite());
	}
}
