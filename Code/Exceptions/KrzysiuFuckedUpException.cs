namespace SER.Code.Helpers.Exceptions;
public class KrzysiuFuckedUpException : DeveloperFuckedUpException
{
    public KrzysiuFuckedUpException() : base("krzysiu")
    {
    }

    public KrzysiuFuckedUpException(string msg) : base("krzysiu", msg)
    {
    }
}
