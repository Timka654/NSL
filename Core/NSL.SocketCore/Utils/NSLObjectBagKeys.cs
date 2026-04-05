namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Centralised ObjectBag string keys used by NSL infrastructure components.
    /// </summary>
    public static class NSLObjectBagKeys
    {
        // ── RequestProcessor ────────────────────────────────────────────────
        public const string RequestProcessor = "NSL__DEFAULT__REQUEST__PROCESSOR";
        public const string ResponsePID      = "NSL__DEFAULT__RESPONSE_PID";

        // ── WaitBuffer ──────────────────────────────────────────────────────
        public const string WaitPacketBuffer = "NSL__DEFAULT__WPB";

        // ── Version ─────────────────────────────────────────────────────────
        public const string Version = "NSL__VERSION";

        // ── Session ─────────────────────────────────────────────────────────
        public const string SessionServerOptions = "NSL__SESSION__SERVEROPTIONS";
        public const string SessionInfo          = "NSL__SESSION__INFO";
        public const string SessionManager       = "NSL__SESSION__MANAGER";
        public const string SessionClientOptions = "NSL__SESSION__CLIENTOPTIONS";
    }
}
