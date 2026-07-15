namespace AuthService.Configuration;

public class DynamoDbOptions
{
    public string TableName { get; set; } =
        "NP-Users";

    public string ServiceUrl { get; set; } =
        "http://localhost:8000";

    public bool UseLocalMode { get; set; } = true;
}