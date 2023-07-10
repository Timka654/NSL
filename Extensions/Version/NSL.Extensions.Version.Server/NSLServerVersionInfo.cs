namespace NSL.Extensions.Version.Server
{
    public class NSLServerVersionInfo : NSLVersionInfo
    {
        public string? MinVersion { get; set; }

        public string? RequireVersion { get; set; }

        public bool ValidateMinVersion(string version)
        {
            if (long.TryParse(version, out var clientVer))
                if (long.TryParse(MinVersion, out var serverMinVer))
                    if (clientVer < serverMinVer)
                        return false;

            return true;
        }

        public bool ValidateRequireVersion(string version)
        {
            if (long.TryParse(version, out var clientVer))
                if (long.TryParse(RequireVersion, out var serverReqVer))
                    if (clientVer != serverReqVer)
                        return false;

            return true;
        }
    }
}
