﻿namespace NSLLibProjectFileFormatter
{
    public record ProjectFileInfo(string Path, string[] Profiles, string dir, List<string> nSLProjectTypes);
}
