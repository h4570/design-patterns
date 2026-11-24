namespace proxy.Users
{
    public enum Role { Anonymous, Employee, Hr }

    /// <summary>
    /// User of the current request. To keep the example simple, user is taken from "?user=" query parameter
    /// instead of a real authentication (JWT, cookies...).
    /// </summary>
    public class CurrentUser
    {

        private static readonly Dictionary<string, Role> _users = new(StringComparer.OrdinalIgnoreCase)
        {
            ["anna"] = Role.Employee,
            ["piotr"] = Role.Employee,
            ["kasia"] = Role.Hr,
        };

        public string Name { get; }
        public Role Role { get; }

        public CurrentUser(IHttpContextAccessor accessor)
        {
            var name = accessor.HttpContext?.Request.Query["user"].ToString();
            if (!string.IsNullOrEmpty(name) && _users.TryGetValue(name, out var role))
            {
                Name = name.ToLowerInvariant();
                Role = role;
            }
            else
            {
                Name = "anonymous";
                Role = Role.Anonymous;
            }
        }

    }
}
