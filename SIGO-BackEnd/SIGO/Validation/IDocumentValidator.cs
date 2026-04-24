namespace SIGO.Validation
{
    public interface IDocumentValidator
    {
        bool IsValid(string? value);
        string Normalize(string value);
    }
}
