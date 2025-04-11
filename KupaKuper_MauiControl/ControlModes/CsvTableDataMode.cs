namespace KupaKuper_MauiControl.ControlModes
{
    public class CsvTableDataMode
    {
        public string[] Headers { get; set; } = new[] { "1", "2" };
        public List<string[]> Rows { get; set; } = new List<string[]>();
    }
} 