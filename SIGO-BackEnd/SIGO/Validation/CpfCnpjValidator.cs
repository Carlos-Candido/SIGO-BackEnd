namespace SIGO.Validation
{
    public class CpfCnpjValidator : DocumentValidatorBase, ICpfCnpjValidator
    {
        private readonly ICpfValidator _cpfValidator;
        private readonly ICnpjValidator _cnpjValidator;

        public CpfCnpjValidator(ICpfValidator cpfValidator, ICnpjValidator cnpjValidator)
        {
            _cpfValidator = cpfValidator;
            _cnpjValidator = cnpjValidator;
        }

        public override bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var digits = Normalize(value);
            return digits.Length switch
            {
                11 => _cpfValidator.IsValid(digits),
                14 => _cnpjValidator.IsValid(digits),
                _ => false
            };
        }
    }
}
