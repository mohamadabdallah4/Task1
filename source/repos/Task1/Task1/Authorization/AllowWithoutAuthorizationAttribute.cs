namespace Task1.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowWithoutAuthorizationAttribute : Attribute
    { }
}
