namespace Core
{
    public static class SqlNative
    {
        public static string SQL_GET_NPC = "select id, task0 from cq_npc;";
        public static string SQL_NPC_DELAY = "select id, npc_id, options, delay_time from cq_npc_delay;";
        public static string SQL_USER_CHAR_NAME = "select name from cq_user where account_id = {0};";
        public static string SQL_GET_ITEMEX_BY_ID = "select id, type from cq_itemex where id in [{0}];";
    }
}
