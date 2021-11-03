using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTT.ServiceModel
{
    [Route("/show")]
    public class ShowArticles : IReturn<ShowArticlesRespounce>
    {
        public string Show { get; set; }
    }
    public class ShowArticlesRespounce
    {
        public string Result { get; set; }
    }
}
