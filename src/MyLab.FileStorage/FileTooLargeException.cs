namespace MyLab.FileStorage;

public class FileTooLargeException : Exception
{
    public FileTooLargeException() : base("File is too large")
    {
            
    }
}