namespace AuthService.Observability;

public sealed class AuthMetrics : IAuthMetrics
{
    private long _loginAttempts;
    private long _loginFailures;
    private long _registrationAttempts;
    private long _registrationFailures;
    private long _dynamoDbFailures;

    private readonly object _latencyLock = new();

    private long _latencySamples;
    private double _latencySum;
    private double _latencyMinimum = double.MaxValue;
    private double _latencyMaximum;

    public void RecordLoginAttempt() =>
        Interlocked.Increment(ref _loginAttempts);

    public void RecordLoginFailure() =>
        Interlocked.Increment(ref _loginFailures);

    public void RecordRegistrationAttempt() =>
        Interlocked.Increment(
            ref _registrationAttempts
        );

    public void RecordRegistrationFailure() =>
        Interlocked.Increment(
            ref _registrationFailures
        );

    public void RecordDynamoDbOperationFailure() =>
        Interlocked.Increment(
            ref _dynamoDbFailures
        );

    public void RecordAuthenticationLatency(
        double elapsedMilliseconds)
    {
        lock (_latencyLock)
        {
            _latencySamples++;
            _latencySum += elapsedMilliseconds;

            _latencyMinimum = Math.Min(
                _latencyMinimum,
                elapsedMilliseconds
            );

            _latencyMaximum = Math.Max(
                _latencyMaximum,
                elapsedMilliseconds
            );
        }
    }

    public AuthMetricsSnapshot TakeSnapshot()
    {
        lock (_latencyLock)
        {
            var samples = _latencySamples;

            var snapshot = new AuthMetricsSnapshot(
                Interlocked.Exchange(
                    ref _loginAttempts,
                    0
                ),
                Interlocked.Exchange(
                    ref _loginFailures,
                    0
                ),
                Interlocked.Exchange(
                    ref _registrationAttempts,
                    0
                ),
                Interlocked.Exchange(
                    ref _registrationFailures,
                    0
                ),
                Interlocked.Exchange(
                    ref _dynamoDbFailures,
                    0
                ),
                samples,
                _latencySum,
                samples == 0
                    ? 0
                    : _latencyMinimum,
                _latencyMaximum
            );

            _latencySamples = 0;
            _latencySum = 0;
            _latencyMinimum = double.MaxValue;
            _latencyMaximum = 0;

            return snapshot;
        }
    }
}