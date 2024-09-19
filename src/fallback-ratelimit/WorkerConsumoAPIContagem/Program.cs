using WorkerConsumoAPIContagem;
using WorkerConsumoAPIContagem.Resilience;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(FallbackContagem.CreateFallbackPolicy());
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
