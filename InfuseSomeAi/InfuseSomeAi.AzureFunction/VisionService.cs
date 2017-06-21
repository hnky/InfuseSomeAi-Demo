using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace InfuseSomeAi.AzureFunction
{
    public static class VisionService
    {
        [FunctionName("ComputerVision")]        
        public static void Run([BlobTrigger("images/{name}", Connection = "BlobStorageConnection")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}