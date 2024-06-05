namespace ButlerCore.Models
{
    public class MovieTag
    {
        public string? Value { get; set; }

        public MovieTag()
        {
            Value = null;
        }

        public MovieTag(string value)
        {
            Value = value;
        }

        public override string ToString() => string.IsNullOrEmpty(Value) ? string.Empty : Value;
    }
}
