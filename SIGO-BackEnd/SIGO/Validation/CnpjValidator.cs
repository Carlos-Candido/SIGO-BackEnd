namespace SIGO.Validation
{
    public class CnpjValidator : DocumentValidatorBase, ICnpjValidator
    {
        public override bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var cnpj = Normalize(value);
            if (cnpj.Length != 14 || AllCharactersAreEqual(cnpj))
                return false;

            var peso1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var peso2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var soma = 0;
            for (var i = 0; i < 12; i++)
                soma += (cnpj[i] - '0') * peso1[i];

            var resto = soma % 11;
            var dig13 = resto < 2 ? 0 : 11 - resto;
            if (dig13 != (cnpj[12] - '0'))
                return false;

            soma = 0;
            for (var i = 0; i < 13; i++)
                soma += (cnpj[i] - '0') * peso2[i];

            resto = soma % 11;
            var dig14 = resto < 2 ? 0 : 11 - resto;

            return dig14 == (cnpj[13] - '0');
        }
    }
}
