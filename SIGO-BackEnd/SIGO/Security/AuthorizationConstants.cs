namespace SIGO.Security
{
    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Funcionario = "Funcionario";
        public const string Oficina = "Oficina";
        public const string Cliente = "Cliente";
    }

    public static class AuthorizationPolicies
    {
        public const string FullAccess = "FullAccess";
        public const string OperationalAccess = "OperationalAccess";
        public const string SelfServiceAccess = "SelfServiceAccess";
    }
}
