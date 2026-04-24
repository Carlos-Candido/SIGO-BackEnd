namespace SIGO.Validation
{
    public class CpfValidator : DocumentValidatorBase, ICpfValidator
    {
        public override bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var cpf = Normalize(value);
            if (cpf.Length != 11 || AllCharactersAreEqual(cpf))
                return false;

            var soma = 0;
            for (var i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * (10 - i);

            var resto = (soma * 10) % 11;
            if (resto == 10 || resto == 11)
                resto = 0;

            if (resto != (cpf[9] - '0'))
                return false;

            soma = 0;
            for (var i = 0; i < 10; i++)
                soma += (cpf[i] - '0') * (11 - i);

            resto = (soma * 10) % 11;
            if (resto == 10 || resto == 11)
                resto = 0;

            return resto == (cpf[10] - '0');
        }
    }
}
