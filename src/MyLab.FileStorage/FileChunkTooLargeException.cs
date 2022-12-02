namespace MyLab.FileStorage;

public class FileChunkTooLargeException : Exception
{
    public FileChunkTooLargeException() : base("File chunk is too large")
    {
            
    }
}