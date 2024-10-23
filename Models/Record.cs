namespace Spider_QAMS.Models
{
    public class Record
    {
        public int RecordId { get; set; } = 0;
        public int RecordType { get; set; }
        public string RecordText { get; set; } = string.Empty;
    }
    public class TextValueOption
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}
