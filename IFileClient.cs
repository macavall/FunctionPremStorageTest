using Azure.Storage.Files.Shares;

public interface IFileClient
{
    public ShareClient fileClient { get; set; }

    public ShareClient GetFileClient();

    public string GetShareName();
}