using ActualizacionService.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<SubtesWorker>();
        services.AddHostedService<BicisStatusWorker>();
        services.AddHostedService<BicisInfoWorker>();
    })
    .Build();

await host.RunAsync();
