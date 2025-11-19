using ogloszenia.Models;

namespace ogloszenia.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message)> RegisterAsync(string username, string email, string password, string firstName, string lastName);
        Task<(bool success, User? user)> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task<(bool success, string message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<(bool success, string message)> ResetPasswordAsync(string email, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly List<User> _users;
        private IHttpContextAccessor? _httpContextAccessor;

        public AuthService(List<User> users)
        {
            _users = users;
        }

        public Task<(bool success, string message)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult((false, "Wszystkie pola są wymagane"));
            }

            if (_users.Any(u => u.Username == username))
            {
                return Task.FromResult((false, "Użytkownik już istnieje"));
            }

            if (_users.Any(u => u.Email == email))
            {
                return Task.FromResult((false, "Email już zarejestrowany"));
            }

            // Create new user
            var newUser = new User
            {
                Id = InMemoryDatabase.GetNextUserId(),
                Username = username,
                Email = email,
                PasswordHash = InMemoryDatabase.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                IsAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _users.Add(newUser);
            return Task.FromResult((true, "Rejestracja udana"));
        }

        public Task<(bool success, User? user)> LoginAsync(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username && u.IsActive);
            
            if (user == null)
            {
                return Task.FromResult((false, (User?)null));
            }

            if (!InMemoryDatabase.VerifyPassword(password, user.PasswordHash))
            {
                return Task.FromResult((false, (User?)null));
            }

            return Task.FromResult((true, user));
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }

        public Task<User?> GetCurrentUserAsync()
        {
            // Will be implemented with session
            return Task.FromResult((User?)null);
        }

        public Task<(bool success, string message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return Task.FromResult((false, "Użytkownik nie znaleziony"));
            }

            if (!InMemoryDatabase.VerifyPassword(oldPassword, user.PasswordHash))
            {
                return Task.FromResult((false, "Stare hasło jest niepoprawne"));
            }

            user.PasswordHash = InMemoryDatabase.HashPassword(newPassword);
            return Task.FromResult((true, "Hasło zmienione pomyślnie"));
        }

        public Task<(bool success, string message)> ResetPasswordAsync(string email, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return Task.FromResult((false, "Użytkownik nie znaleziony"));
            }

            user.PasswordHash = InMemoryDatabase.HashPassword(newPassword);
            return Task.FromResult((true, "Hasło zresetowane pomyślnie"));
        }
    }
}
