using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Vision;

namespace InfuseSomeAi.AzureFunction
{
    public static class VisionService
    {
        [FunctionName("ComputerVision")]
        public static async Task Run([BlobTrigger("images/{name}", Connection = "BlobStorageConnection")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info("\n\n" +
                     "###                                                                         #    ### \n" +
                     " #  #    # ###### #    #  ####  ######     ####   ####  #    # ######      # #    #  \n" +
                     " #  ##   # #      #    # #      #         #      #    # ##  ## #          #   #   #  \n" +
                     " #  # #  # #####  #    #  ####  #####      ####  #    # # ## # #####     #     #  #  \n" +
                     " #  #  # # #      #    #      # #              # #    # #    # #         #######  #  \n" +
                     " #  #   ## #      #    # #    # #         #    # #    # #    # #         #     #  #  \n" +
                     "### #    # #       ####   ####  ######     ####   ####  #    # ######    #     # ### \n");




            /* STEP 1 - Tag and describe images */
            log.Info($"\n\n-- Computer Vision -- ");
            VisionServiceClient service = new VisionServiceClient(ConfigurationManager.AppSettings["visionApiKey"], "https://westeurope.api.cognitive.microsoft.com/vision/v1.0");
            var result = await service.AnalyzeImageAsync(myBlob, new[] { VisualFeature.Tags, VisualFeature.Description });

            string description = result.Description.Captions.First().Text;
            string tags = string.Join(",", result.Tags.Select(a => a.Name));

            log.Info($"Description: {description}");
            log.Info($"Tags: {tags}");







            /* STEP 2  - Face detection */
            log.Info($"\n\n-- Face detection  -- ");
            myBlob.Seek(0, SeekOrigin.Begin);

            FaceServiceClient faceService = new FaceServiceClient(ConfigurationManager.AppSettings["faceApiKey"], "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

            var faceResult = await faceService.DetectAsync(myBlob, true, false, new[] { FaceAttributeType.Age, FaceAttributeType.Gender, FaceAttributeType.Smile });

            foreach (var face in faceResult)
            {
                log.Info($"Face found: Age:{face.FaceAttributes.Age} | Gender: {face.FaceAttributes.Gender} | Smile: {face.FaceAttributes.Smile}");
            }







            /* STEP 5 - Identify people on pictures */
             
            log.Info($"\n\n-- Face Identification -- ");
            string personGroupname = ConfigurationManager.AppSettings["personGroupName"];

            var facesToIdentify = faceResult.Select(a => a.FaceId).ToArray();
            IdentifyResult[] identifyResults = await faceService.IdentifyAsync(personGroupname, facesToIdentify);

            foreach (var identifyResult in identifyResults)
            {
                if (identifyResult.Candidates.Any())
                {
                    var person = faceService.GetPersonAsync(personGroupname, identifyResult.Candidates.First().PersonId);
                    log.Info($"Person Found: {person.Result.Name}");
                }
            }
            
            

            log.Info($"\n\n-- DONE-- \n\n");
        }
    }
}