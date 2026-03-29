using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.Plugins.Payday2GSI.Prerequisites;

public class InstallPayday2GsiModAction : PluginPrerequisiteAction
{
	private const string DownloadUrl = "https://github.com/Aurora-RGB/Payday2-GSI/archive/main.zip";

	public InstallPayday2GsiModAction() : base("Install Payday 2 GSI mod")
	{
	}

	public override async Task Execute(CancellationToken cancellationToken)
	{
		if (!Payday2SteamFinder.TryGetInstallPath(out string payday2Path))
			throw new ArtemisPluginException("PAYDAY 2 is not installed or could not be located through Steam.");

		if (!BLTModPrerequisite.HasBltLoader(payday2Path))
			throw new ArtemisPluginException("PAYDAY 2 was found, but BLT/SuperBLT is not installed. Install the BLT loader first.");

		string modsPath = Path.Combine(payday2Path, "mods");
		string gsiPath = Path.Combine(payday2Path, "GSI");
		string artemisXmlPath = Path.Combine(gsiPath, "Artemis.xml");
		string tempZipPath = Path.Combine(Path.GetTempPath(), "Payday2-GSI-main.zip");

		Directory.CreateDirectory(modsPath);
		Directory.CreateDirectory(gsiPath);

		await File.WriteAllTextAsync(artemisXmlPath, Payday2GsiConstants.ArtemisXmlContent, cancellationToken).ConfigureAwait(false);

		using HttpClient client = new();
		await using (Stream downloadStream = await client.GetStreamAsync(DownloadUrl, cancellationToken).ConfigureAwait(false))
		await using (FileStream fileStream = new(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			await downloadStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
		}

		string extractedFolder = Path.Combine(modsPath, "Payday2-GSI-main");
		if (Directory.Exists(extractedFolder))
			Directory.Delete(extractedFolder, true);

		ZipFile.ExtractToDirectory(tempZipPath, modsPath, true);
		File.Delete(tempZipPath);
	}
}