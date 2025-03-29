namespace Core
{
    public static class SqlNative
    {
        public static string SQL_GET_NPC = "select id, task0 from cq_npc;";
        public static string SQL_NPC_DELAY = "select id, npc_id, options, delay_time from cq_npc_delay;";
    }
}
