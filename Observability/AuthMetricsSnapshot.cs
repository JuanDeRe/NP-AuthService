namespace AuthService.Observability;

public sealed record AuthMetricsSnapshot(
    long LoginAttempts,
    long LoginFailures,
    long RegistrationAttempts,
    long RegistrationFailures,
    long DynamoDbOperationFailures,
    long AuthenticationSamples,
    double AuthenticationLatencySum,
    double AuthenticationLatencyMinimum,
    double AuthenticationLatencyMaximum
);