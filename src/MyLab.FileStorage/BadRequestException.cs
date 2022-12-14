namespace MyLab.FileStorage
{
    public class BadRequestException : Exception
    {
        public BadRequestException() : base("Bad request")
        {
            
        }

        public BadRequestException(string message) : base(message)
        {

        }
    }
}
