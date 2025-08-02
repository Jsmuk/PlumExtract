namespace PlumExtract.Domain.Interfaces;

public interface IBlobStore
{
    IBlobContainer GetContainer(string containerName = "");
}