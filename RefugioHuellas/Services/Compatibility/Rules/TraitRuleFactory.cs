namespace RefugioHuellas.Services.Compatibility.Rules
{
    public class TraitRuleFactory : ITraitRuleFactory
    {
        private readonly Dictionary<string, ITraitRule> _rules;

        public TraitRuleFactory(IEnumerable<ITraitRule> rules)
        {
            // Si hay duplicados, el último gana.
            _rules = rules
                .Where(r => !string.IsNullOrWhiteSpace(r.Key))
                .ToDictionary(r => r.Key, r => r, StringComparer.OrdinalIgnoreCase);
        }

        public ITraitRule? GetForKey(string key)
            => _rules.TryGetValue(key, out var rule) ? rule : null;
    }
}
