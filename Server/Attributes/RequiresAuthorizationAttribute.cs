namespace Server.Attributes;
public enum UserRole {
    None = 0,
    User = 1,
    Admin = 2,
    UserOrAdmin = User | Admin
}

[AttributeUsage(AttributeTargets.Method)]
public class RequiresAuthorizationAttribute : Attribute {
    public UserRole Role { get; }

    public RequiresAuthorizationAttribute(UserRole role = UserRole.UserOrAdmin) {
        Role = role;
    }
}
