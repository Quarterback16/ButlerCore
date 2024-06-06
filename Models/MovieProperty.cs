
namespace ButlerCore.Models
{
    public class MovieProperty
    {
        public string? Name { get; set; }
        public string? Value { get; set; }

        public MovieProperty()
        {           
        }

        public MovieProperty(string name)
        {
            Name = name;
        }

        public MovieProperty(
            string name, 
            string propertyLine)
        {
            Name = name;
            if (propertyLine.Trim() == $"{name}:")
                Value = string.Empty;
            else
                Value = ValueFrom(name,propertyLine)?.Trim();
        }

        private string? ValueFrom(
            string name, 
            string propertyLine)
        {
            if (propertyLine.Length < name.Length + 1)
                Console.WriteLine($"Error on {name} > {propertyLine}");
            return propertyLine.Substring(name.Length + 1);
        }

        public override string ToString() => $"{Name}: {Value}";

    }
}
