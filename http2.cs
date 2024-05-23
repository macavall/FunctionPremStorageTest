using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System;

namespace DataFactoryFA
{
    public class http2
    {
        private readonly ILogger<http2> _logger;
        // private static readonly ShareClient share = new ShareClient(ConnectionString, ShareName);
        private readonly ShareClient share;
        private readonly IFileClient fileClient;

        public http2(ILogger<http2> logger, IFileClient _fileClient)
        {
            _logger = logger;
            fileClient = _fileClient;
            share = fileClient.GetFileClient();
        }

        [Function("http2")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            await DeleteAll();

            return new OkObjectResult("Welcome to Azure Functions!");
        }

        async Task DeleteAll()
        {
            Console.WriteLine("Starting deletion of all files and directories...");

            // Create a ShareClient object
            // ShareClient share = new ShareClient(ConnectionString, ShareName);

            // Ensure the file share exists
            if (await share.ExistsAsync())
            {
                // Get the root directory client
                ShareDirectoryClient rootDirectory = share.GetRootDirectoryClient();

                // Recursively delete all files and directories
                await DeleteDirectoryContentsAsync(rootDirectory);

                Console.WriteLine("All files and directories have been deleted.");
            }
            else
            {
                Console.WriteLine($"File share '{fileClient.GetShareName()}' does not exist.");
            }
        }

        private static async Task DeleteDirectoryContentsAsync(ShareDirectoryClient directoryClient)
        {
            // List all files and subdirectories in the current directory
            await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (item.IsDirectory)
                {
                    // Recursively delete the subdirectory
                    ShareDirectoryClient subdirectoryClient = directoryClient.GetSubdirectoryClient(item.Name);
                    await DeleteDirectoryContentsAsync(subdirectoryClient);

                    // Delete the subdirectory itself
                    await subdirectoryClient.DeleteIfExistsAsync();
                    Console.WriteLine($"Deleted directory: {subdirectoryClient.Path}");
                }
                else
                {
                    // Delete the file
                    ShareFileClient fileClient = directoryClient.GetFileClient(item.Name);
                    await fileClient.DeleteIfExistsAsync();
                    Console.WriteLine($"Deleted file: {fileClient.Path}");
                }
            }
        }
    }
}
