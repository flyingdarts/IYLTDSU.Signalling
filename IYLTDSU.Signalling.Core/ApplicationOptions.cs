using Amazon.DynamoDBv2.DataModel;

namespace IYLTDSU.Signalling.Core;

public class ApplicationOptions
{
    public string TableName { get; set; }
    public const string ConnectionIdField = "connectionId";
    public DynamoDBOperationConfig ToOperationConfig => new() { OverrideTableName = TableName };
}
