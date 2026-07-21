namespace AuthService.Observability;

public interface IAuthMetrics
{
    void RecordLoginAttempt();

    void RecordLoginFailure();

    void RecordRegistrationAttempt();

    void RecordRegistrationFailure();

    void RecordAuthenticationLatency(
        double elapsedMilliseconds
    );

    void RecordDynamoDbOperationFailure();

    AuthMetricsSnapshot TakeSnapshot();
}