namespace ButlerCore.Models
{
    public class MovieProperty
    {
        public string? Name { get; set; }
        public string? Value { get; set; }

        public MovieProperty()
        {           
        }

        public MovieProperty(
            string name, 
            string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => $"{Name}: {Value}";

    }
}
