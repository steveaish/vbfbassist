using System.Reflection;

namespace VBFB_API
{
    public static class Globals
    {
        // Baked in at build time (InformationalVersion). History lives in CHANGES.md.
        public static readonly string VersionNumber =
            (typeof(Globals).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0-local").Split('+')[0];

        public static Ready Ready { get; set; } = new Ready();
    }
}
