using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTT.ServiceModel
{
    [Route("/show/{CurrentPage}/{TotalArticlePerPage}")]
    public class ShowArticles : IReturn<ShowArticlesRespounce>
    {
        public int CurrentPage { get; set; }
        public int TotalArticlePerPage { get; set; }
    }
    public class ShowArticlesRespounce
    {
        public int TotalArticles { get; set; }
        public List<ParsedPage> DbList { get; set; }
    }
}
