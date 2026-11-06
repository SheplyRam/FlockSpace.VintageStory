namespace Flock.Shared.Satiation.Commands;

/// <summary>
/// Language keys of the satiation debug commands.
/// </summary>
public static class SatiationCommandLang
{
    /// <summary>
    /// Description of the <c>/fsatinfo</c> command.
    /// </summary>
    public const string InfoDescription = "cmd-fsatinfo-desc";

    /// <summary>
    /// Description of the <c>/fsatset</c> command.
    /// </summary>
    public const string SetDescription = "cmd-fsatset-desc";

    /// <summary>
    /// Description of the <c>/fsatgive</c> command.
    /// </summary>
    public const string GiveDescription = "cmd-fsatgive-desc";

    /// <summary>
    /// Description of the <c>/fsatdrain</c> command.
    /// </summary>
    public const string DrainDescription = "cmd-fsatdrain-desc";

    /// <summary>
    /// Description of the <c>/fsatclear</c> command.
    /// </summary>
    public const string ClearDescription = "cmd-fsatclear-desc";

    /// <summary>
    /// Error when the caller is not a player carrying the satiation behavior.
    /// </summary>
    public const string RequiresPlayer = "cmd-fsat-requiresplayer";

    /// <summary>
    /// Error when the caller's entity has no hunger behavior.
    /// </summary>
    public const string NoHunger = "cmd-fsat-nohunger";

    /// <summary>
    /// Error when <c>/fsatgive poison</c> runs without the poison storage attribute.
    /// </summary>
    public const string RequiresPoisonStorage = "cmd-fsat-requirespoisonstorage";

    /// <summary>
    /// Error when <c>/fsatgive psy</c> runs without the psychedelic storage attribute.
    /// </summary>
    public const string RequiresPsyStorage = "cmd-fsat-requirespsystorage";

    /// <summary>
    /// One-line composite state summary returned by every satiation command.
    /// </summary>
    public const string InfoLine = "cmd-fsatinfo-line";
}
