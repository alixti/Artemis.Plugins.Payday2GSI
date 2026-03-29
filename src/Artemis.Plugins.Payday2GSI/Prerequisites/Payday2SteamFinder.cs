using Narod.SteamGameFinder;

namespace Artemis.Plugins.Payday2GSI.Prerequisites;

internal static class Payday2SteamFinder
{
	public static bool TryGetInstallPath(out string installPath)
	{
		installPath = string.Empty;

		SteamGameLocator steamGameLocator = new();
		if (!steamGameLocator.getIsSteamInstalled())
			return false;

		try
		{
			var gameInfo = steamGameLocator.getGameInfoByID(Payday2GsiConstants.Payday2AppId);
			if (string.IsNullOrWhiteSpace(gameInfo.steamGameLocation))
				return false;

			installPath = gameInfo.steamGameLocation;
			return true;
		}
		catch
		{
			return false;
		}
	}
}