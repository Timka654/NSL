namespace NSL.Utils.RecoveryManager
{
    public class RecoverySessionInfo<T>
    {
        public string[] RestoreKeys { get; set; }

        public T Client { get; set; }

        internal RecoverySessionInfo(T client, string[] keys)
        {
            Client = client;
            RestoreKeys = keys;
        }
    }
}
