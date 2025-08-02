namespace PlumExtract.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class BlobProviderAttribute(string type) : Attribute
{
    public string Type { get; set; } = type;
}