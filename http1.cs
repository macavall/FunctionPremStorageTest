using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataFactoryFA
{
    public class http1
    {
        private int fileSizeMB = Convert.ToInt32(Environment.GetEnvironmentVariable("FileSizeInBytes") ?? "10") * 1024 * 1024; // 1 MB
        private int directoryDepth = Convert.ToInt32(Environment.GetEnvironmentVariable("directoryDepth") ?? "5");
        private readonly ShareClient share;

        private readonly ILogger<http1> _logger;

        public http1(ILogger<http1> logger, IFileClient _fileClient)
        {
            _logger = logger;
            share = _fileClient.GetFileClient();
        }

        [Function("http1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            await CreateDirectories();

            return new OkObjectResult("Welcome to Azure Functions!");
        }

        async Task CreateDirectories()
        {
            Console.WriteLine("Starting directory and file creation...");

            // Create the file share if it doesn't exist
            await share.CreateIfNotExistsAsync();

            // Create the 3 top-level directories
            for (int i = 1; i <= directoryDepth; i++)
            {
                string directoryName = $"directory{i}";
                ShareDirectoryClient directory = share.GetDirectoryClient(directoryName);

                // Create the top-level directory
                await directory.CreateIfNotExistsAsync();

                // Create nested directories and files
                await CreateNestedDirectoriesAndFiles(directory, directoryDepth);

                Console.WriteLine($"Created {directoryName} with nested directories and files.");
            }

            Console.WriteLine("Directory and file creation complete.");
        }

        async Task CreateNestedDirectoriesAndFiles(ShareDirectoryClient parentDirectory, int depth)
        {
            if (depth <= 0) return;

            string nestedDirectoryName = $"nested{depth}";
            ShareDirectoryClient nestedDirectory = parentDirectory.GetSubdirectoryClient(nestedDirectoryName);

            // Create the nested directory
            await nestedDirectory.CreateIfNotExistsAsync();

            // Create a 1 MB text file in the current directory
            string fileName = "sample.txt";
            ShareFileClient file = nestedDirectory.GetFileClient(fileName);

            // Create the file with the specified size
            await file.CreateAsync(fileSizeMB);
            byte[] data = Encoding.UTF8.GetBytes(new string('A', fileSizeMB));
            using (MemoryStream stream = new MemoryStream(data))
            {
                await file.UploadAsync(stream);
            }

            Console.WriteLine($"Created {fileName} in {nestedDirectoryName}.");

            // Recursively create the next level of nested directories and files
            await CreateNestedDirectoriesAndFiles(nestedDirectory, depth - 1);
        }
    }
}
