using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Payday2GSI.DataModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Payday2GSI;

[PluginFeature(Name = "Payday 2 GSI Endpoint", Description = "Receives Payday 2 game state JSON over Artemis web server")]
public class Payday2GsiModule : Module<Payday2DataModel>
{
	private readonly ILogger _logger;
	private HttpListener? _listener;
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _listenerTask;
	private readonly object _dataLock = new();

	private static string Prefix => Payday2GsiConstants.Prefix;
	private static string RoutePath => Payday2GsiConstants.RoutePath;

	public Payday2GsiModule(ILogger logger)
	{
		_logger = logger;
	}

	public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

	public override void Enable()
	{
		_cancellationTokenSource = new CancellationTokenSource();

		// Custom HTTP server as Artemis web server doesn't support query parameters or raw body for GET requests, which Payday 2 GSI uses.
		// Payday 2 also doesn't support POST requests, so we can't switch to that.
		_listener = new HttpListener();
		_listener.Prefixes.Add(Prefix);
		_listener.Start();

		_listenerTask = Task.Run(() => ListenLoopAsync(_cancellationTokenSource.Token));
		_logger.Information("Payday 2 local server enabled at {Url}{Path}", Prefix.TrimEnd('/'), RoutePath);
	}

	public override void Disable()
	{
		_cancellationTokenSource?.Cancel();

		if (_listener != null)
		{
			if (_listener.IsListening)
				_listener.Stop();
			_listener.Close();
			_listener = null;
		}

		_listenerTask = null;
		_cancellationTokenSource?.Dispose();
		_cancellationTokenSource = null;
		_logger.Information("Payday 2 local server disabled");
	}

	public override void Update(double deltaTime)
	{
	}

	private async Task ListenLoopAsync(CancellationToken cancellationToken)
	{
		if (_listener == null)
			return;

		while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
		{
			HttpListenerContext? context = null;
			try
			{
				context = await _listener.GetContextAsync().ConfigureAwait(false);
				await HandleContextAsync(context).ConfigureAwait(false);
			}
			catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Unhandled error in Payday 2 local server loop");
				if (context?.Response is { OutputStream: var stream })
				{
					try
					{
						context.Response.StatusCode = 500;
						stream.Close();
					}
					catch
					{
						// Best effort response cleanup.
					}
				}
			}
		}
	}

	private async Task HandleContextAsync(HttpListenerContext context)
	{
		HttpListenerRequest request = context.Request;
		HttpListenerResponse response = context.Response;

		if (!string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
		{
			response.StatusCode = 405;
			response.Close();
			return;
		}

		if (!string.Equals(request.Url?.AbsolutePath, RoutePath, StringComparison.OrdinalIgnoreCase))
		{
			response.StatusCode = 404;
			response.Close();
			return;
		}

		string payload = request.QueryString["json"] ?? string.Empty;
		if (string.IsNullOrWhiteSpace(payload))
			payload = await ReadBodyAsync(request).ConfigureAwait(false);

		if (string.IsNullOrWhiteSpace(payload))
		{
			_logger.Warning("Received GET {Path} without JSON payload", RoutePath);
			response.StatusCode = 400;
			response.Close();
			return;
		}

		try
		{
			using JsonDocument document = JsonDocument.Parse(payload);
			ApplyGameState(document.RootElement);
			//_logger.Information("Payday 2 game state updated");
			response.StatusCode = 204;
		}
		catch (JsonException ex)
		{
			_logger.Warning(ex, "Invalid JSON payload received on {Path}", RoutePath);
			response.StatusCode = 400;
		}

		response.Close();
	}

	private static async Task<string> ReadBodyAsync(HttpListenerRequest request)
	{
		if (request.InputStream == null || !request.InputStream.CanRead)
			return string.Empty;

		using StreamReader reader = new(request.InputStream, request.ContentEncoding ?? Encoding.UTF8, true, 1024, true);
		return await reader.ReadToEndAsync().ConfigureAwait(false);
	}

	private void ApplyGameState(JsonElement root)
	{
		string phase = TryGetString(root, "level", "phase");

		int localPeerId = TryGetInt(root, "provider", "local_peer_id");
		JsonElement playerElement = FindLocalPlayer(root, localPeerId);

		lock (_dataLock)
		{
			DataModel.Level.Phase = phase;

			DataModel.Player.HealthTotal = TryGetDouble(playerElement, "health", "total");
			DataModel.Player.HealthCurrent = TryGetDouble(playerElement, "health", "current");
			DataModel.Player.ArmorTotal = TryGetDouble(playerElement, "armor", "total");
			DataModel.Player.ArmorCurrent = TryGetDouble(playerElement, "armor", "current");
			DataModel.Player.IsSwansong = TryGetBool(playerElement, "is_swansong");
			DataModel.Player.State = TryGetString(playerElement, "state");
			DataModel.Player.Suspicion = TryGetDouble(playerElement, "suspicion");
			DataModel.Player.WeaponPrimaryLeft = TryGetInt(playerElement, "weapons", "primary", "current_left");
			DataModel.Player.WeaponSecondaryLeft = TryGetInt(playerElement, "weapons", "secondary", "current_left");
			DataModel.Player.WeaponSelected = ResolveWeaponSelected(playerElement);
		}
	}

	private static JsonElement FindLocalPlayer(JsonElement root, int localPeerId)
	{
		if (localPeerId <= 0)
			return default;

		if (!root.TryGetProperty("players", out JsonElement players) || players.ValueKind != JsonValueKind.Object)
			return default;

		string peerKey = localPeerId.ToString();
		if (players.TryGetProperty(peerKey, out JsonElement byPeer)
			&& byPeer.ValueKind == JsonValueKind.Object
			&& TryGetBool(byPeer, "is_local_player"))
			return byPeer;

		foreach (JsonProperty candidate in players.EnumerateObject())
		{
			if (candidate.Value.ValueKind == JsonValueKind.Object && TryGetBool(candidate.Value, "is_local_player"))
				return candidate.Value;
		}

		return default;
	}

	private static int ResolveWeaponSelected(JsonElement player)
	{
		bool primarySelected = TryGetBool(player, "weapons", "primary", "is_selected");
		bool secondarySelected = TryGetBool(player, "weapons", "secondary", "is_selected");

		if (primarySelected)
			return 1;
		if (secondarySelected)
			return 2;

		return 0;
	}

	private static bool TryGetBool(JsonElement element, params string[] path)
	{
		if (!TryGetPathElement(element, path, out JsonElement value))
			return false;

		if (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
			return value.GetBoolean();

		return false;
	}

	private static int TryGetInt(JsonElement element, params string[] path)
	{
		if (!TryGetPathElement(element, path, out JsonElement value))
			return 0;

		if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
			return intValue;

		return 0;
	}

	private static double TryGetDouble(JsonElement element, params string[] path)
	{
		if (!TryGetPathElement(element, path, out JsonElement value))
			return 0;

		if (value.ValueKind == JsonValueKind.Number)
			return value.GetDouble();

		return 0;
	}

	private static string TryGetString(JsonElement element, params string[] path)
	{
		if (!TryGetPathElement(element, path, out JsonElement value))
			return string.Empty;

		if (value.ValueKind == JsonValueKind.String)
			return value.GetString() ?? string.Empty;

		return string.Empty;
	}

	private static bool TryGetPathElement(JsonElement element, string[] path, out JsonElement result)
	{
		result = element;
		foreach (string segment in path)
		{
			if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty(segment, out result))
				return false;
		}

		return true;
	}
}