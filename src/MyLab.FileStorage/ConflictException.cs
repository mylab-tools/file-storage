namespace MyLab.FileStorage
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {

        }

        public ConflictException() : base("Conflict exception")
        {
            
        }
    }
}
