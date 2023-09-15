namespace NSL.Extensions.Version.Server
{
    public class NSLServerVersionInfo : NSLVersionInfo
    {
        public string MinVersion { get; set; }

        public string RequireVersion { get; set; }

        public bool ValidateMinVersion(string version)
        {
            if (long.TryParse(MinVersion, out var serverMinVer))
                if (long.TryParse(version, out var clientVer))
                {
                    if (clientVer < serverMinVer)
                        return false;
                }
                else
                    return false;

            return true;
        }

        public bool ValidateRequireVersion(string version)
        {
            if (long.TryParse(RequireVersion, out var serverReqVer))
                if (long.TryParse(version, out var clientVer))
                {
                    if (clientVer != serverReqVer)
                        return false;
                }
                else
                    return false;

            return true;
        }
    }
}
