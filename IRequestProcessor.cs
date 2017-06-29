using System.Web;

public interface IRequestProcessor
{
    string key { get; }

    void Process(HttpContext context);
}

