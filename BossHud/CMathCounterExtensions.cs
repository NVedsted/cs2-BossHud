using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace BossHud;

public static class CMathCounterExtensions
{
    private static short? _outValueOffset;

    public static unsafe float GetValue(this CMathCounter counter)
    {
        _outValueOffset ??= Schema.GetSchemaOffset("CMathCounter", "m_OutValue");

        return *(float*)IntPtr.Add(counter.Handle, _outValueOffset.Value + 24);
    }
}