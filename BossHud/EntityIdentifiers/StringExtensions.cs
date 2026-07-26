namespace BossHud.EntityIdentifiers;

public static class StringExtensions
{
    extension(string s)
    {
        public EntityIdentifier AsTargetNameEntityIdentifier() => new TargetNameEntityIdentifier(s);
    }
}