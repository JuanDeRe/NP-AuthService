using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AuthService.Configuration;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure;

/// <summary>
/// Creates the users table when it does not exist.
/// </summary>
public class DynamoDbTableInitializer
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbOptions _options;
    private readonly ILogger<DynamoDbTableInitializer>
        _logger;

    public DynamoDbTableInitializer(
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbOptions> options,
        ILogger<DynamoDbTableInitializer> logger)
    {
        _dynamoDb = dynamoDb;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var tables = await _dynamoDb.ListTablesAsync(
            cancellationToken
        );

        if (tables.TableNames.Contains(
                _options.TableName))
        {
            _logger.LogInformation(
                "DynamoDB table {TableName} already exists",
                _options.TableName
            );

            return;
        }

        _logger.LogInformation(
            "Creating DynamoDB table {TableName}",
            _options.TableName
        );

        await _dynamoDb.CreateTableAsync(
            new CreateTableRequest
            {
                TableName = _options.TableName,

                AttributeDefinitions =
                [
                    new AttributeDefinition
                    {
                        AttributeName = "Username",
                        AttributeType =
                            ScalarAttributeType.S
                    }
                ],

                KeySchema =
                [
                    new KeySchemaElement
                    {
                        AttributeName = "Username",
                        KeyType = KeyType.HASH
                    }
                ],

                BillingMode =
                    BillingMode.PAY_PER_REQUEST
            },
            cancellationToken
        );

        await WaitUntilActiveAsync(
            cancellationToken
        );

        _logger.LogInformation(
            "DynamoDB table {TableName} is ready",
            _options.TableName
        );
    }

    private async Task WaitUntilActiveAsync(
        CancellationToken cancellationToken)
    {
        for (var attempt = 1;
             attempt <= 30;
             attempt++)
        {
            var response =
                await _dynamoDb.DescribeTableAsync(
                    _options.TableName,
                    cancellationToken
                );

            if (response.Table.TableStatus ==
                TableStatus.ACTIVE)
            {
                return;
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(500),
                cancellationToken
            );
        }

        throw new TimeoutException(
            $"DynamoDB table '{_options.TableName}' " +
            "did not become active."
        );
    }
}