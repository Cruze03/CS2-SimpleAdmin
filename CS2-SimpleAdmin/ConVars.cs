using CounterStrikeSharp.API.Modules.Cvars;

namespace CS2_SimpleAdmin;

public static class ConVars
{
    // This convar is registered from the plugin instance but can be used anywhere.
    public static FakeConVar<string> ServerIP = new("server_ip", "A fake convar for the server IP address", "127.0.0.1");
}
