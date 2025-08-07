public class LoginResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? Role { get; set; }

    public bool Success => !string.IsNullOrEmpty(Token);
}
