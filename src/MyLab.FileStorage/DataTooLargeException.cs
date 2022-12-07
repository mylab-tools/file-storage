namespace MyLab.FileStorage;

public class DataTooLargeException : Exception
{
    public DataTooLargeException() : base("Data is too large")
    {

    }
}