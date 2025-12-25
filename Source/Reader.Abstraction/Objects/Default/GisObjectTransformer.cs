namespace Reader.Abstraction.Objects.Default
{
    public class GisObjectTransformer : IGisObjectTransformer
    {
        private readonly Dictionary<string, string> _mapping;
        public GisObjectTransformer(IReadOnlyDictionary<string, string> mapping)
        {
            _mapping = mapping.ToDictionary(Q=>Q.Key , Q=>Q.Value);
        }
        public GisObjectTransformer()
        {
            _mapping = new Dictionary<string, string>
            {
                { "GLOBALID", "MAPID" }
            };
        }
        public string GetFieldName(in string field)
        {
            if (_mapping.Count == 0)
                return field;
            return _mapping.TryGetValue(field, out var name) ? name : field;
        }

    }
}
