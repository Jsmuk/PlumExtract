namespace PlumExtract.Domain.Interfaces;

public interface IOutputFormatterFactory
{
    IOutputFormatter Create(string type);
}