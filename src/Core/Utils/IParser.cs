namespace MySharpChat.Core.Utils
{
    public interface IParser<T>
    {
        public T? Parse(string? text);
        public bool TryParse(string? text, out T? parsedObject);
    }
}
