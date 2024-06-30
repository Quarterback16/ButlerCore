namespace ButlerCore.Helpers
{
    public static class FileSystemHelper
    {
        public static void DeleteDirectory(
            string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, recursive: true);
                    Directory.Delete(directoryPath);
                    Console.WriteLine($"Directory '{directoryPath}' has been deleted.");
                }
                else
                {
                    Console.WriteLine($"Directory '{directoryPath}' does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting directory: {ex.Message}");
            }
        }
    }

}
