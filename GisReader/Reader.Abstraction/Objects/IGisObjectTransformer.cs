namespace Reader.Abstraction.Objects;

public interface IGisObjectTransformer
{
    string GetFieldName(in string field);
}