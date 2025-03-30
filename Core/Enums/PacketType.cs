namespace Core;

public enum PacketType : int
{
    UserInfoResponse = 1009,
    LoginRequest = 1090,
    UserInfoRequest = 1052,
    LoginResponse = 1055,
    RobotRepsonse = 1117,
    /// <summary>
    /// Click NPC
    /// </summary>
    ClickNpcRequest = 2031,
    NPCAction = 2032,
    AddinationItemRequest = 2041

}
