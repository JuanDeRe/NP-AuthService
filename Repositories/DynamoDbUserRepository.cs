using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AuthService.Configuration;
using AuthService.Models;
using Microsoft.Extensions.Options;
using AuthService.Observability;

namespace AuthService.Repositories;

/// <summary>
/// Stores and retrieves authentication users in DynamoDB.
/// </summary>
public class DynamoDbUserRepository : IUserRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbOptions _options;
    private readonly IAuthMetrics _metrics;

    public DynamoDbUserRepository(
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbOptions> options,
        IAuthMetrics metrics)
    {
        _dynamoDb = dynamoDb;
        _options = options.Value;
        _metrics = metrics;
    }

    public async Task<UserAccount?> GetByUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default)
    {
        GetItemResponse response;
        try
        {
            response = await _dynamoDb.GetItemAsync(
                new GetItemRequest
                {
                    TableName = _options.TableName,

                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["Username"] = new()
                        {
                            S = normalizedUsername
                        }
                    },

                    ConsistentRead = true
                },
                cancellationToken
            );
        }
        catch (AmazonDynamoDBException)
        {
            _metrics.RecordDynamoDbOperationFailure();
            throw;
        }

        if (response.Item is null ||
            response.Item.Count == 0)
        {
            return null;
        }

        return MapUser(response.Item);
    }

    public async Task<bool> CreateAsync(
        UserAccount user,
        CancellationToken cancellationToken = default)
    {
        var request = new PutItemRequest
        {
            TableName = _options.TableName,

            Item = new Dictionary<string, AttributeValue>
            {
                ["Username"] = new()
                {
                    S = user.Username
                },
                ["PlayerId"] = new()
                {
                    S = user.PlayerId
                },
                ["PasswordHash"] = new()
                {
                    S = user.PasswordHash
                },
                ["Role"] = new()
                {
                    S = user.Role
                },
                ["CreatedAtUtc"] = new()
                {
                    S = user.CreatedAtUtc.ToString("O")
                },
                ["IsActive"] = new()
                {
                    BOOL = user.IsActive
                }
            },

            // Evita sobrescribir un usuario existente.
            ConditionExpression =
                "attribute_not_exists(Username)"
        };

        try
        {
            await _dynamoDb.PutItemAsync(
                request,
                cancellationToken
            );

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
        catch (AmazonDynamoDBException)
        {
            _metrics.RecordDynamoDbOperationFailure();
            throw;
        }
    }

    private static UserAccount MapUser(
        IReadOnlyDictionary<string, AttributeValue> item)
    {
        return new UserAccount
        {
            Username = item["Username"].S,
            PlayerId = item["PlayerId"].S,
            PasswordHash = item["PasswordHash"].S,
            Role = item["Role"].S,

            CreatedAtUtc = DateTime.Parse(
                item["CreatedAtUtc"].S,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind
            ),

            IsActive = item.TryGetValue(
                "IsActive",
                out var activeValue
            )
                ? activeValue.BOOL == true
                : true
        };
    }
}