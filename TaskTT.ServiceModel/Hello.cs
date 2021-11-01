using ServiceStack;

namespace TaskTT.ServiceModel
{
    [Route("/parse")]
    [Route("/parse/{PageParse}")]
    public class Page : IReturn<HelloResponse>
    {
        public string PageParse { get; set; }
    }
    public class HelloResponse
    {
        public string Result { get; set; }
    }


}
