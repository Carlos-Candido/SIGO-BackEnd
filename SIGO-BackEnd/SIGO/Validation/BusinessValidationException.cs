namespace SIGO.Validation
{
    public class BusinessValidationException : Exception
    {
        public IReadOnlyCollection<ValidationError> Errors { get; }

        public BusinessValidationException(IEnumerable<ValidationError> errors)
            : base("Dados inválidos.")
        {
            Errors = errors.ToList().AsReadOnly();
        }
    }
}
