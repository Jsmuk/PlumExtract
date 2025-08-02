namespace PlumExtract.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class OutputFormatterAttribute(string type) : Attribute
{
    public string Type { get; set; } = type;
}