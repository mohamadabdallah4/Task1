namespace Task1.Services
{
    public interface IUserService
    {
        string GeneratePassword();
        User? GetById(int id);
    }
}
