using ServiceStack;
using System.Collections.Generic;

namespace TaskTT.ServiceModel
{
    [Route("/search/{TypeSearch}/{AtributeSearch}/{CurentPage}/{TotalArticlePerPage}")]
    public class Search : IReturn<SearchRespounce>
    {
        public string AtributeSearch { get; set; }
        public string TypeSearch { get; set; }
        public int CurentPage { get; set; }
        public int TotalArticlePerPage { get; set; }
    }
    public class SearchRespounce
    {
        public int TotalArticles { get; set; }

        public List<ParsedPage> DbList { get; set; }
    }


}
