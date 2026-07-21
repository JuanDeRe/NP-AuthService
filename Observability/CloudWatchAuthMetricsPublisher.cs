using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Options;

namespace AuthService.Observability;

public sealed class CloudWatchAuthMetricsPublisher
    : BackgroundService
{
    private const string ServiceName =
        "auth-service";

    private readonly IAuthMetrics _metrics;
    private readonly IAmazonCloudWatch _cloudWatch;
    private readonly AuthMetricsOptions _options;

    private readonly ILogger<
        CloudWatchAuthMetricsPublisher
    > _logger;

    public CloudWatchAuthMetricsPublisher(
        IAuthMetrics metrics,
        IAmazonCloudWatch cloudWatch,
        IOptions<AuthMetricsOptions> options,
        ILogger<CloudWatchAuthMetricsPublisher>
            logger)
    {
        _metrics = metrics;
        _cloudWatch = cloudWatch;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "CloudWatch AuthService metrics are disabled."
            );

            return;
        }

        var interval =
            _options.PublishIntervalSeconds <= 0
                ? 60
                : _options.PublishIntervalSeconds;

        _logger.LogInformation(
            "CloudWatch AuthService metrics publisher " +
            "started. Namespace={Namespace}, " +
            "Environment={Environment}, " +
            "IntervalSeconds={IntervalSeconds}",
            _options.Namespace,
            _options.Environment,
            interval
        );

        await PublishAsync(stoppingToken);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(interval)
        );

        try
        {
            while (await timer.WaitForNextTickAsync(
                       stoppingToken))
            {
                await PublishAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task PublishAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var snapshot =
                _metrics.TakeSnapshot();

            var timestamp = DateTime.UtcNow;

            var metricData = new List<MetricDatum>
            {
                CreateMetric(
                    "LoginAttempts",
                    snapshot.LoginAttempts,
                    StandardUnit.Count,
                    timestamp
                ),
                CreateMetric(
                    "LoginFailures",
                    snapshot.LoginFailures,
                    StandardUnit.Count,
                    timestamp
                ),
                CreateMetric(
                    "RegistrationAttempts",
                    snapshot.RegistrationAttempts,
                    StandardUnit.Count,
                    timestamp
                ),
                CreateMetric(
                    "RegistrationFailures",
                    snapshot.RegistrationFailures,
                    StandardUnit.Count,
                    timestamp
                ),
                CreateMetric(
                    "DynamoDbOperationFailures",
                    snapshot.DynamoDbOperationFailures,
                    StandardUnit.Count,
                    timestamp
                )
            };

            if (snapshot.AuthenticationSamples > 0)
            {
                metricData.Add(
                    new MetricDatum
                    {
                        MetricName =
                            "AuthenticationLatency",

                        Unit =
                            StandardUnit.Milliseconds,

                        Timestamp = timestamp,

                        Dimensions =
                            CreateDimensions(),

                        StatisticValues =
                            new StatisticSet
                            {
                                SampleCount =
                                    snapshot
                                        .AuthenticationSamples,

                                Sum =
                                    snapshot
                                        .AuthenticationLatencySum,

                                Minimum =
                                    snapshot
                                        .AuthenticationLatencyMinimum,

                                Maximum =
                                    snapshot
                                        .AuthenticationLatencyMaximum
                            }
                    }
                );
            }

            await _cloudWatch.PutMetricDataAsync(
                new PutMetricDataRequest
                {
                    Namespace = _options.Namespace,
                    MetricData = metricData
                },
                cancellationToken
            );

            _logger.LogInformation(
                "Published AuthService metrics. " +
                "LoginAttempts={LoginAttempts}, " +
                "LoginFailures={LoginFailures}, " +
                "RegistrationAttempts=" +
                "{RegistrationAttempts}, " +
                "RegistrationFailures=" +
                "{RegistrationFailures}, " +
                "DynamoDbOperationFailures=" +
                "{DynamoDbFailures}",
                snapshot.LoginAttempts,
                snapshot.LoginFailures,
                snapshot.RegistrationAttempts,
                snapshot.RegistrationFailures,
                snapshot.DynamoDbOperationFailures
            );
        }
        catch (OperationCanceledException)
            when (cancellationToken
                .IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish AuthService " +
                "metrics to CloudWatch."
            );
        }
    }

    private MetricDatum CreateMetric(
        string name,
        double value,
        StandardUnit unit,
        DateTime timestamp)
    {
        return new MetricDatum
        {
            MetricName = name,
            Value = value,
            Unit = unit,
            Timestamp = timestamp,
            Dimensions = CreateDimensions()
        };
    }

    private List<Dimension> CreateDimensions()
    {
        return
        [
            new Dimension
            {
                Name = "Environment",
                Value = _options.Environment
            },
            new Dimension
            {
                Name = "Service",
                Value = ServiceName
            }
        ];
    }
}