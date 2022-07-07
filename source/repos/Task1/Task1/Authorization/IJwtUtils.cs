namespace Task1.Authorization
{
    public interface IJwtUtils
    {
        string GenerateToken(User user);
        int? GetIdFromToken(string token);
        string? GetIATFromToken(string token);
    }
}
