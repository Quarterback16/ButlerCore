using ButlerCore.Models;

namespace ButlerCore.Helpers
{
    public static class MediaHelper
    {
        public static List<MediaProperty> KnownProperties()
        {
            return new List<MediaProperty>()
            {
                new MediaProperty("How"),
                new MediaProperty("With"),
                new MediaProperty("when"),
                new MediaProperty("Year"),
                new MediaProperty("genre"),
                new MediaProperty("author"),
                new MediaProperty("rating"),
                new MediaProperty("Keeper"),
                new MediaProperty("Priority"),
                new MediaProperty("Completion"),
            };
        }
    }
}
