namespace SIGO.Validation
{
    public abstract class DocumentValidatorBase : IDocumentValidator
    {
        public abstract bool IsValid(string? value);

        public string Normalize(string value)
        {
            return new(value.Where(char.IsDigit).ToArray());
        }

        protected static bool AllCharactersAreEqual(string value)
        {
            return value.All(c => c == value[0]);
        }
    }
}
