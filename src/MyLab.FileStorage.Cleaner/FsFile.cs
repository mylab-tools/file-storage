namespace MyLab.FileStorage.Cleaner;

public class FsFile
{
    public string Directory { get; }
    public bool Confirmed { get; set; }
    public DateTime CreateDt { get; set; }
        
    public FsFile(string directory)
    {
        Directory = directory;
    }
}