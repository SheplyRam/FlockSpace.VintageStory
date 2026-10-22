using Vintagestory.API.Common;

[assembly: ModInfo("Flock Shared Library", "flockspacesharedlib",
    Authors = new string[] { "sheply" },
    Description = "A shared library associated with other Flock Space mods",
    Version = "1.0.0")]

namespace Flock.Shared;

public sealed class FlockSharedModSystem : ModSystem { }
 