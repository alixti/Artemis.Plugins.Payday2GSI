namespace Artemis.Plugins.Payday2GSI;

public static class Payday2GsiConstants
{
	public const string Payday2AppId = "218620";
	public const int ServerPort = 17042;
	public const string RoutePath = "/gameState/pd2";
	public const string ServerHost = "127.0.0.1";
	public static string Prefix => $"http://{ServerHost}:{ServerPort}/";
	public static string ArtemisXmlContent => $"<GSIServer uri=\"{ServerHost}:{ServerPort}\" name=\"Artemis Integration\" />";
}