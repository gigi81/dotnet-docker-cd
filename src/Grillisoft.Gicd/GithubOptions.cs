namespace Grillisoft.Gicd;

public class GithubOptions
{
    public const string SectionName = "Github";
    
    public string Token { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Repository { get; set; } = "";

    public string RemoteUrl =>
        !string.IsNullOrEmpty(Token)
            ? $"https://x-access-token:{Token}@github.com/{Owner}/{Repository}.git"
            : $"https://github.com/{Owner}/{Repository}.git";

}