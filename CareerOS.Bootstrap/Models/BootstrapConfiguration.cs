namespace CareerOS.Bootstrap.Models;

public class BootstrapConfiguration
{
    public string DestinationRoot { get; set; } = string.Empty;

    public List<ProfileConfiguration> Profiles { get; set; } = [];
}
