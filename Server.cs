using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DotNet.ContainerImage;

public class Server : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<Server> _logger;
    private string _ipAddress;
    private string _port;
    private bool _isStopped = true;
    private TcpListener _listener;

    public Server(IConfiguration configuration, ILogger<Server> logger)
    {
        _configuration = configuration;
        _ipAddress = _configuration["ListenAddr"];
        _port = _configuration["ListenPort"];
        _logger = logger;
        Start();
    }

    ~Server()
    {
       Stop();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            _isStopped = true;
            await Stop();
        }
    }

    private async Task Start()
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), int.Parse(_port));
        _listener = new(ipEndPoint);       
        _listener.Start();
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Server started {address}:{port}", _ipAddress, _port);
        }
        _isStopped = false;

        TcpClient handler = await _listener.AcceptTcpClientAsync();
        await using NetworkStream stream = handler.GetStream();

        var buffer = new byte[1_024];
        int receivedByteCount = 0;
        while (!_isStopped && (receivedByteCount = stream.Read(buffer)) > 0)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, receivedByteCount);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Message received: \"{message}\"");
            }
        }
    }

    private async Task Stop()
    {
        _listener.Stop();
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Server stpped");
        }
    }
}
