namespace SIGO.Security
{
    public interface IPasswordHasher
    {
        string Hash(string input);
    }
}
