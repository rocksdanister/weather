using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models;

public class AppUpdateManifest
{
    public Version ManifestVersion { get; set; } = new Version(1, 0, 0, 0);
    public Version ReleaseVersion { get; set; }
    public Uri ReleaseNotes { get; set; }
}
