using ServiceStack;

namespace TaskTT.ServiceModel
{
    [Route("/search/{TypeSearch}/{AtributeSearch}")]
    public class Search : IReturn<SearchRespounce>
    {
        public string AtributeSearch { get; set; }
        public string TypeSearch { get; set; }
    }
    public class SearchRespounce
    {
        public string Result { get; set; }
    }


}
