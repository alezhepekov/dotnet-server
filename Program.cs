using DotNet.ContainerImage;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Server>();

var host = builder.Build();
host.Run();
