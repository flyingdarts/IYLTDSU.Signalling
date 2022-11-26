namespace IYLTDSU.Signalling.Shared
{
    public record SignallingRecord(
        string ConnectionId,
        string CurrentRoomId,
        string RoomId
    );
}