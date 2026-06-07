namespace ButlerCore.Helpers
{
    public static class FolderHelper
    {
        public static string DropboxFolder() =>
        
            Environment.MachineName.Equals("MAHOMES")
                ? "c:/users/quart/dropbox/"
                : "d:/dropbox/";
        
    }
}
