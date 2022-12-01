namespace MyLab.FileStorage
{
    public class BadChecksumException : Exception
    {
        public BadChecksumException() : base("Bad checksum")
        {
            
        }
    }
}
