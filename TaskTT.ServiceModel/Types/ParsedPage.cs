using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTT.ServiceModel
{
    public class ParsedPage
    {
        [PrimaryKey, AutoIncrement]
        public  int Id { get; set; }

        [Index(Unique = true)]
        public string UrlArticle { get; set; }
        public string HtmlArticle { get; set; }
        public  string TitleArticle { get; set; }
        public  DateTime DateArticle { get; set; }
        public  string TextArticle { get; set; }

    }
}
