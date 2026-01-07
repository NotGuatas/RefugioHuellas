namespace RefugioHuellas.Services.Compatibility.Rules
{
    /// Factory Method: resuelve la regla adecuada para una clave de rasgo.
    public interface ITraitRuleFactory
    {
        ITraitRule? GetForKey(string key);
    }
}
