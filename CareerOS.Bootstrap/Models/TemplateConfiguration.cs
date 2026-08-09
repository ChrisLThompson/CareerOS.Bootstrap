namespace CareerOS.Bootstrap.Models;

public class TemplateConfiguration
{
    public List<CareerTemplate> Templates { get; set; } = [];
}

public class CareerTemplate
{
    public string Name { get; set; } = string.Empty;

    public List<DirectoryNode> Directories { get; set; } = [];
}