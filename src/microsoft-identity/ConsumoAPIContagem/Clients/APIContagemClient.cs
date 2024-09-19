using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Polly;
using Polly.Retry;
using Refit;
using ConsumoAPIContagem.Extensions;
using ConsumoAPIContagem.Interfaces;
using ConsumoAPIContagem.Models;

namespace ConsumoAPIContagem.Clients;

public class APIContagemClient : IDisposable
{
    private IConfidentialClientApplication? _clientMicrosoftEntraID;
    private IContagemAPI? _contagemAPI;
    private IConfiguration? _configuration;
    private Serilog.Core.Logger? _logger;
    private Token? _token;
    private AsyncRetryPolicy? _jwtPolicy;
    private JsonSerializerOptions? _serializerOptions;
    private string? _scopeEntra;
    private string? _subscriptionKey;
    private string? _functionAppKey;

    public bool IsAuthenticatedUsingToken
    {
        get => _token?.Authenticated ?? false;
    }

    public APIContagemClient(IConfiguration configuration,
        Serilog.Core.Logger logger)
    {
        _configuration = configuration;
        _logger = logger;

        string urlBase = _configuration.GetSection(
            "APIContagem_Access:UrlBase").Value!;

        _clientMicrosoftEntraID = ConfidentialClientApplicationBuilder
            .Create(_configuration["APIContagem_Access:ClientIdAppRegistration"])
            .WithClientSecret(_configuration["APIContagem_Access:ClientSecretAppRegistration"])
            .WithAuthority(
                new Uri($"{_configuration["APIContagem_Access:EndpointLogin"]}/{_configuration["APIContagem_Access:TenantIdEntra"]}"))
            .Build();
        _scopeEntra = _configuration["APIContagem_Access:ScopeEntra"];
        _subscriptionKey = _configuration["APIContagem_Access:SubscriptionKeyAPIM"];
        _functionAppKey = _configuration["APIContagem_Access:FunctionKey"];

        _contagemAPI = RestService.For<IContagemAPI>(urlBase);
        _jwtPolicy = CreateAccessTokenPolicy();
        _serializerOptions = new JsonSerializerOptions() { WriteIndented = true };
    }

    public async Task Autenticar()
    {
        _token = null;
        try
        {
            // Envio da requisição ao Microsoft Entra ID a fim de autenticar
            // e obter o token de acesso
            var resultAuthMicrosoftEntraID = await _clientMicrosoftEntraID!
                .AcquireTokenForClient([_scopeEntra]).ExecuteAsync();

            if (!String.IsNullOrWhiteSpace(resultAuthMicrosoftEntraID?.AccessToken))
            {
                _token = new Token()
                {
                    Authenticated = true,
                    AccessToken = resultAuthMicrosoftEntraID.AccessToken
                };
                _logger!.Information("Token JWT:" +
                    Environment.NewLine +
                    FormatJSONPayload<Token>(_token));
                _logger.Information("Payload do Access Token JWT:" +
                    Environment.NewLine +
                    FormatJSONPayload<PayloadAccessToken>(
                        Jose.JWT.Payload<PayloadAccessToken>(_token.AccessToken)));
            }
            else
                _logger!.Error("Falha na autenticacao com o Microsoft Entra ID...");
        }
        catch
        {
            _logger!.Error("Falha ao autenticar...");
        }
    }

    private string FormatJSONPayload<T>(T payload) =>
        JsonSerializer.Serialize(payload,
            new JsonSerializerOptions() { WriteIndented = true });

    private AsyncRetryPolicy CreateAccessTokenPolicy()
    {
        return Policy
            .HandleInner<ApiException>(
                ex => ex.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(1, async (ex, retryCount, context) =>
            {
                var corAnterior = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync(
                    Environment.NewLine + "Token expirado ou usuário sem permissão!");
                Console.ForegroundColor = corAnterior;

                Console.ForegroundColor = ConsoleColor.Green;
                await Console.Out.WriteLineAsync(
                    Environment.NewLine + "Execução de RetryPolicy..." +
                    Environment.NewLine);
                Console.ForegroundColor = corAnterior;

                await Autenticar();
                if (!(_token?.Authenticated ?? false))
                    throw new InvalidOperationException("Token inválido!");

                context["AccessToken"] = _token.AccessToken;
            });
    }

    public async Task ExibirResultadoContador()
    {
        var retorno = await _jwtPolicy!.ExecuteWithTokenAsync<ResultadoContador>(
            _token!, async (context) =>
        {
            string tokenAppRegistration = context["AccessToken"].ToString()!;
            var resultado = await _contagemAPI!.ObterValorAtualAsync(
              $"Bearer {tokenAppRegistration}",
              _subscriptionKey!, _functionAppKey!);
            return resultado;
        });
        _logger!.Information("Retorno da API de Contagem: " +
            Environment.NewLine +
            FormatJSONPayload<ResultadoContador>(retorno));
    }

    public void Dispose()
    {
        _clientMicrosoftEntraID = null;
        _contagemAPI = null;
        _configuration = null;
        _logger = null;
        _token = null;
        _jwtPolicy = null;
        _serializerOptions = null;
    }
}