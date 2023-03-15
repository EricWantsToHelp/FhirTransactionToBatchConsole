using Microsoft.Health.Fhir.Synthea;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FhirTransactionToBatchConsole
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            foreach (string path in args)
            {
                if (File.Exists(path))
                {
                    // This path is a file
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }
        }

        //Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            // Keep it simple and safe. Stick to one level.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            //// Recurse into subdirectories of this directory.
            //string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            //foreach (string subdirectory in subdirectoryEntries)
            //    ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            Console.WriteLine("Processing file '{0}'.", path);

            // Read the file to string
            string requestBody = System.IO.File.ReadAllText(path);

            JObject bundle = new JObject();
            JArray entries;
            try
            {
                bundle = JObject.Parse(requestBody);
            }
            catch (JsonReaderException)
            {
                Console.WriteLine("ERROR: Input file is not a valid JSON document - " + Path.GetFileName(path));
            }

            try
            {
                SyntheaReferenceResolver.ConvertUUIDs(bundle);
                SyntheaPostToPut.PostToPut(bundle);
            }
            catch
            {
                Console.WriteLine("ERROR: Failed to resolve references in doc " + Path.GetFileName(path) + ". Check if it's a valid patient file.");
            }

            // Write the bundle to a new file
            string newPath = Path.GetDirectoryName(path) + @"\Resolved - " + Path.GetFileName(path);
            File.WriteAllText(newPath, bundle.ToString());

            Console.WriteLine("Finished processing new file '{0}'.", newPath);
        }
    }
}