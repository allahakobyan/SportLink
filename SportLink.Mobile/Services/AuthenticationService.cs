using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Mail;
using System.Net;

namespace SportLink.Mobile.Services;

/// <summary>
/// Enhanced Authentication Service with email verification and strong passwords
/// </summary>
public static class AuthenticationService
{
    private const string UsersKey = "users_database";
    private const string CurrentUserKey = "current_user";
    private const string AuthTokenKey = "auth_token";

    public class User
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string ProfileImageUrl { get; set; } = "";
        public string Bio { get; set; } = "";
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
    }

    // ============ PASSWORD VALIDATION ============

    /// <summary>
    /// Validate password strength
    /// Requirements: 8+ chars, at least 1 uppercase, 1 lowercase, 1 number, 1 symbol
    /// </summary>
    public static (bool isValid, string error) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters long.");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least 1 uppercase letter (A-Z).");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least 1 lowercase letter (a-z).");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least 1 number (0-9).");

        // Check for symbols: !@#$%^&*()_+-=[]{}|;:,.<>?
        string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        if (!password.Any(c => symbols.Contains(c)))
            return (false, "Password must contain at least 1 symbol (!@#$%^&* etc).");

        return (true, "");
    }

    // ============ REGISTRATION ============

    /// <summary>
    /// Register new user with email verification
    /// </summary>
    public static async Task<AuthResult> RegisterAsync(string fullName, string username, string email, string password)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(fullName))
            return new AuthResult { Success = false, Message = "Full name is required." };

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return new AuthResult { Success = false, Message = "Username must be at least 3 characters." };

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return new AuthResult { Success = false, Message = "Please enter a valid email address." };

        // Validate password strength
        var (isPasswordValid, passwordError) = ValidatePassword(password);
        if (!isPasswordValid)
            return new AuthResult { Success = false, Message = passwordError };

        var users = await LoadUsersAsync();

        //  PREVENT DUPLICATE USERNAMES
        if (users.Any(u => u.Username.ToLower() == username.ToLower()))
            return new AuthResult { Success = false, Message = "This username is already taken. Please choose another one." };

        // Prevent duplicate emails
        if (users.Any(u => u.Email.ToLower() == email.ToLower()))
            return new AuthResult { Success = false, Message = "This email is already registered." };

        //  GENERATE AND SEND EMAIL VERIFICATION CODE
        string verificationCode = GenerateVerificationCode();
        bool emailSent = await SendVerificationEmailAsync(email, verificationCode, fullName);

        if (!emailSent)
            return new AuthResult { Success = false, Message = "Failed to send verification email. Please check your email address." };

        // Store verification code temporarily
        await SaveVerificationCodeAsync(email, verificationCode);

        // Create user but mark as NOT verified yet
        var newUser = new User
        {
            FullName = fullName,
            Username = username,
            Email = email,
            PasswordHash = "", // Will be set after email verification
            PasswordSalt = "",
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false
        };

        // Store temporary user
        await SaveTemporaryUserAsync(email, password, newUser);

        return new AuthResult
        {
            Success = true,
            Message = $"Verification code sent to {email}. Please check your inbox.",
            User = newUser
        };
    }

    /// <summary>
    /// Verify email with code and complete registration
    /// </summary>
    public static async Task<AuthResult> VerifyEmailAsync(string email, string verificationCode, string password)
    {
        // Check if verification code matches
        var savedCode = await GetVerificationCodeAsync(email);
        if (savedCode != verificationCode)
            return new AuthResult { Success = false, Message = "Invalid verification code. Please try again." };

        // Get temporary user data
        var tempUser = await GetTemporaryUserAsync(email);
        if (tempUser == null)
            return new AuthResult { Success = false, Message = "Registration session expired. Please sign up again." };

        // Hash password
        var (hash, salt) = HashPassword(password);
        tempUser.PasswordHash = hash;
        tempUser.PasswordSalt = salt;
        tempUser.IsEmailVerified = true;

        // Save user permanently
        var users = await LoadUsersAsync();
        users.Add(tempUser);
        await SaveUsersAsync(users);

        // Clear temporary data
        await ClearVerificationCodeAsync(email);
        await ClearTemporaryUserAsync(email);

        // Generate token
        var token = GenerateToken(tempUser);
        await SaveCurrentUserAsync(tempUser);
        await SaveAuthTokenAsync(token);

        return new AuthResult
        {
            Success = true,
            Message = "Email verified! Account created successfully.",
            User = tempUser,
            Token = token
        };
    }

    // ============ LOGIN ============

    /// <summary>
    /// Login user with username/email and password
    /// </summary>
    public static async Task<AuthResult> LoginAsync(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            return new AuthResult { Success = false, Message = "Username/Email and password are required." };

        var users = await LoadUsersAsync();
        var user = users.FirstOrDefault(u =>
            (u.Username.ToLower() == usernameOrEmail.ToLower() ||
             u.Email.ToLower() == usernameOrEmail.ToLower()) &&
            u.IsEmailVerified); // Only allow verified users

        if (user == null)
            return new AuthResult { Success = false, Message = "Invalid username/email or account not verified." };

        // Verify password
        if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            return new AuthResult { Success = false, Message = "Incorrect password." };

        // Generate token and save session
        var token = GenerateToken(user);
        await SaveCurrentUserAsync(user);
        await SaveAuthTokenAsync(token);

        return new AuthResult
        {
            Success = true,
            Message = "Login successful!",
            User = user,
            Token = token
        };
    }

    // ============ USER MANAGEMENT ============

    /// <summary>
    /// Get currently logged-in user
    /// </summary>
    public static async Task<User> GetCurrentUserAsync()
    {
        var json = Preferences.Get(CurrentUserKey, "");
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<User>(json);
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public static bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(Preferences.Get(AuthTokenKey, ""));
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    public static async Task LogoutAsync()
    {
        Preferences.Remove(CurrentUserKey);
        Preferences.Remove(AuthTokenKey);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    public static async Task<AuthResult> UpdateProfileAsync(string fullName, string bio, string profileImageUrl)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return new AuthResult { Success = false, Message = "No user logged in." };

        user.FullName = fullName;
        user.Bio = bio;
        user.ProfileImageUrl = profileImageUrl;

        var users = await LoadUsersAsync();
        var index = users.FindIndex(u => u.UserId == user.UserId);
        if (index >= 0)
        {
            users[index] = user;
            await SaveUsersAsync(users);
            await SaveCurrentUserAsync(user);
        }

        return new AuthResult
        {
            Success = true,
            Message = "Profile updated successfully!",
            User = user
        };
    }

    /// <summary>
    /// Update profile picture
    /// </summary>
    public static async Task<AuthResult> UpdateProfilePictureAsync(string imagePath)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return new AuthResult { Success = false, Message = "No user logged in." };

        user.ProfileImageUrl = imagePath;

        var users = await LoadUsersAsync();
        var index = users.FindIndex(u => u.UserId == user.UserId);
        if (index >= 0)
        {
            users[index] = user;
            await SaveUsersAsync(users);
            await SaveCurrentUserAsync(user);
        }

        return new AuthResult
        {
            Success = true,
            Message = "Profile picture updated!",
            User = user
        };
    }

    //  PRIVATE HELPER METHODS 

    private static (string hash, string salt) HashPassword(string password)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            string salt = Convert.ToBase64String(saltBytes);

            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            string hashString = Convert.ToBase64String(hash);

            return (hashString, salt);
        }
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(20);
            string hashString = Convert.ToBase64String(hashBytes);

            return hashString == hash;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateVerificationCode()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString(); // 6-digit code
    }

    private static async Task<bool> SendVerificationEmailAsync(string email, string code, string name)
    {
        try
        {
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("sportlink000@gmail.com", "svumprghgxubkadm"),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress("sportlink000@gmail.com"),
                Subject = "SportLink Verification Code",
                Body = $"Hello {name},\n\nYour verification code is: {code}"
            };

            message.To.Add(email);

            await smtp.SendMailAsync(message);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateToken(User user)
    {
        var tokenData = new
        {
            userId = user.UserId,
            username = user.Username,
            email = user.Email,
            timestamp = DateTime.UtcNow.Ticks
        };
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tokenData)));
    }

    private static async Task SaveCurrentUserAsync(User user)
    {
        var json = JsonSerializer.Serialize(user);
        Preferences.Set(CurrentUserKey, json);
        await Task.CompletedTask;
    }

    private static async Task SaveAuthTokenAsync(string token)
    {
        Preferences.Set(AuthTokenKey, token);
        await Task.CompletedTask;
    }

    private static async Task<List<User>> LoadUsersAsync()
    {
        try
        {
            var json = Preferences.Get(UsersKey, "[]");
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        catch
        {
            Preferences.Remove(UsersKey);
            return new List<User>();
        }
    }

    private static async Task SaveUsersAsync(List<User> users)
    {
        var json = JsonSerializer.Serialize(users);
        Preferences.Set(UsersKey, json);
        await Task.CompletedTask;
    }

    // ============ EMAIL VERIFICATION HELPERS ============

    private static async Task SaveVerificationCodeAsync(string email, string code)
    {
        Preferences.Set($"verify_code_{email}", code);
        Preferences.Set($"verify_time_{email}", DateTime.UtcNow.Ticks.ToString());
        await Task.CompletedTask;
    }

    private static async Task<string> GetVerificationCodeAsync(string email)
    {
        var code = Preferences.Get($"verify_code_{email}", "");
        var timeStr = Preferences.Get($"verify_time_{email}", "0");

        if (string.IsNullOrEmpty(code))
            return null;

        // Code expires in 10 minutes
        if (long.TryParse(timeStr, out long ticks))
        {
            var savedTime = new DateTime(ticks);
            if ((DateTime.UtcNow - savedTime).TotalMinutes > 10)
            {
                await ClearVerificationCodeAsync(email);
                return null;
            }
        }

        return code;
    }

    private static async Task ClearVerificationCodeAsync(string email)
    {
        Preferences.Remove($"verify_code_{email}");
        Preferences.Remove($"verify_time_{email}");
        await Task.CompletedTask;
    }

    // ============ TEMPORARY USER HELPERS ============

    private static async Task SaveTemporaryUserAsync(string email, string password, User user)
    {
        var tempData = new { user = user, password = password };
        var json = JsonSerializer.Serialize(tempData);
        Preferences.Set($"temp_user_{email}", json);
        Preferences.Set($"temp_time_{email}", DateTime.UtcNow.Ticks.ToString());
        await Task.CompletedTask;
    }

    private static async Task<User> GetTemporaryUserAsync(string email)
    {
        var json = Preferences.Get($"temp_user_{email}", "");
        var timeStr = Preferences.Get($"temp_time_{email}", "0");

        if (string.IsNullOrEmpty(json))
            return null;

        // Temporary data expires in 30 minutes
        if (long.TryParse(timeStr, out long ticks))
        {
            var savedTime = new DateTime(ticks);
            if ((DateTime.UtcNow - savedTime).TotalMinutes > 30)
            {
                await ClearTemporaryUserAsync(email);
                return null;
            }
        }

        try
        {
            var tempData = JsonSerializer.Deserialize<JsonElement>(json);
            var user = JsonSerializer.Deserialize<User>(tempData.GetProperty("user").GetRawText());
            return user;
        }
        catch
        {
            return null;
        }
    }

    private static async Task ClearTemporaryUserAsync(string email)
    {
        Preferences.Remove($"temp_user_{email}");
        Preferences.Remove($"temp_time_{email}");
        await Task.CompletedTask;
    }
}