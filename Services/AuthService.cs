using ogloszenia.Models;
using ogloszenia.Data;
using Microsoft.EntityFrameworkCore;

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
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Wszystkie pola są wymagane");
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return (false, "Użytkownik już istnieje");
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return (false, "Email już zarejestrowany");
            }

            // Create new user
            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                IsAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return (true, "Rejestracja udana");
        }

        public async Task<(bool success, User? user)> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
            
            if (user == null)
            {
                return (false, null);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, null);
            }

            return (true, user);
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

        public async Task<(bool success, string message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "Użytkownik nie znaleziony");
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return (false, "Stare hasło jest niepoprawne");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return (true, "Hasło zmienione pomyślnie");
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return (false, "Użytkownik nie znaleziony");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return (true, "Hasło zresetowane pomyślnie");
        }
    }
}
