namespace Core
{
    public class UserContext
    {
        public uint AccountId { get; set; }
        public uint TokenId { get; set; }
        /// <summary>
        /// Offset: 816
        /// </summary>
        public uint UserId { get; set; }
        public RobotModel Robot { get; set; }

    }
}
