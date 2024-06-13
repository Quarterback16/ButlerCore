namespace ButlerCore.Models
{
    public class MediaTag
    {
        public string? Value { get; set; }

        public MediaTag()
        {
            Value = null;
        }

        public MediaTag(string value)
        {
            Value = value;
        }

        public override string ToString() => string.IsNullOrEmpty(Value) ? string.Empty : Value;
    }
}
