using Amazon.DynamoDBv2.Model;

namespace IYLTDSU.Signalling.Shared
{
    public record SignallingRecord(
        string? ConnectionId,
        string? CurrentRoomId,
        string? RoomId
    )
    {
        public static explicit operator SignallingRecord(Dictionary<string, AttributeValue> values)
        {
            return new SignallingRecord
            (
                values["ConnectionId"].ToString(),
                values["CurrentRoomId"].ToString(),
                values["RoomId"].ToString()
            );
        }
    }
}