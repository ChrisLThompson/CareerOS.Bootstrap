namespace CareerOS.Bootstrap.Models;

public class DirectoryNode
{
    public string Name { get; set; } = string.Empty;

    public List<DirectoryNode> Children { get; set; } = [];
}