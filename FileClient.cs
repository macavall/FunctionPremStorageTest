using Azure.Storage.Files.Shares;
using System;

public class FileClient : IFileClient
{
    private string ConnectionString = Environment.GetEnvironmentVariable("fileShareConnString") ?? "No_Connection_String_Found";
    private string ShareName = Environment.GetEnvironmentVariable("testfileshare") ?? "No_File_Share_Name_Found";
    public ShareClient fileClient { get; set; }

    public FileClient()
    {
        fileClient = new ShareClient(ConnectionString, ShareName);
    }

    public ShareClient GetFileClient()
    {
        return fileClient;
    }

    public string GetShareName()
    {
        return ShareName;
    }
}