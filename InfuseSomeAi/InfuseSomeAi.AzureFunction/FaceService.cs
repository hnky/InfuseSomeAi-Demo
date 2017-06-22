using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Face;

namespace InfuseSomeAi.AzureFunction
{
    public static class FaceService
    {
        [FunctionName("FaceAPI")]
        public static async Task Run([BlobTrigger("faces/{name}", Connection = "BlobStorageConnection")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info("------------");
            log.Info($"FaceService Function -  Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");


            /* STEP 3 - Create facegroup  */

            FaceServiceClient faceService = new FaceServiceClient(ConfigurationManager.AppSettings["faceApiKey"], "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

            string personGroupname = ConfigurationManager.AppSettings["personGroupName"];

            log.Info($"-- Person group --");
            try
            {
                await faceService.CreatePersonGroupAsync(personGroupname, personGroupname);
                log.Info($"Created");
            }
            catch (FaceAPIException ex)
            {
                log.Info(ex.ErrorMessage);
                log.Info(ex.ErrorCode);
            }
           
            





            /* STEP 4 - Add a person to a facegroup */

            log.Info($"\n\n-- Face Add person -- ");

            // Create a person
            var person = await faceService.CreatePersonAsync(personGroupname, name.Split('.').First());

            // Add a face to the person
            var persistantFace = await faceService.AddPersonFaceAsync(personGroupname, person.PersonId, myBlob);

            // Train the group
            await faceService.TrainPersonGroupAsync(personGroupname);

            log.Info($"-- Person added: {name.Split('.').First()} / {person.PersonId} / {persistantFace.PersistedFaceId} -- \n\n");

            


            log.Info($"\n\n-- DONE-- \n\n");
        }
    }
}