namespace SIGO.Security
{
    public interface IJwtTokenService
    {
        string GenerateToken(JwtTokenRequest request);
    }
}
