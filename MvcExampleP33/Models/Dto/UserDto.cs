namespace MvcExampleP33.Models.Dto;

public class UserDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = [];

    public string? AvatarSrc { get; set; }
}
