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
        _ipAddress = _configuration.GetSection("ListenAddr").Get<string>();
        _port = _configuration.GetSection("ListenPort").Get<string>();
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

        while (!_isStopped)
        {
            using TcpClient handler = await _listener.AcceptTcpClientAsync();
            await using NetworkStream stream = handler.GetStream();

            var message = $"📅 {DateTime.Now} 🕛";
            var dateTimeBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(dateTimeBytes);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Sent message: \"{message}\"");
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
