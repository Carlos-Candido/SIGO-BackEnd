namespace SIGO.Security
{
    public class FuncionarioRoleResolver : IFuncionarioRoleResolver
    {
        public string Resolve(string? cargo)
        {
            if (string.IsNullOrWhiteSpace(cargo))
                return SystemRoles.Funcionario;

            var normalizedCargo = cargo.Trim().ToUpperInvariant();
            return normalizedCargo is "ADMIN" or "ADMINISTRADOR"
                ? SystemRoles.Admin
                : SystemRoles.Funcionario;
        }
    }
}
