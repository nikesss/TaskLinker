using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp;
using Pullenti.Ner;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TaskTT.ServiceModel;
using TaskTT.ServiceModel.Types;

namespace TaskTT.ServiceInterface
{

    public class ParsePage : Service
    {
        public IDbConnectionFactory DbFactory { get; set; }

        public object Any(Search request)
        {

            if (request.AtributeSearch != null && request.TypeSearch == "byarticle")
            {
                using (var db = DbFactory.OpenDbConnection())
                {
                    var sql = db.From<ParsedPage>()
                        .Where<ParsedPage>(x => x.TextArticle.Contains($"{request.AtributeSearch}"));
                    var sqlList = db.SqlList<int>(sql);
                    var articles = db.Select<ParsedPage>(sql)
                        .OrderByDescending(x => x.DateArticle)
                        .Skip(request.CurentPage * request.TotalArticlePerPage)
                        .Take(request.TotalArticlePerPage).ToList();

                    return new SearchRespounce { TotalArticles = sqlList.Count, DbList = articles };
                }
                
            }
            else if (request.AtributeSearch != null && request.TypeSearch == "pullenti")
            {
                using(var db = DbFactory.OpenDbConnection())
                {
                    var sql = db.From<Entitis>()
                        .Where(x => x.ValueEntitis.Contains(request.AtributeSearch)).SelectDistinct(x => x.ParsedPageId);
                    var sqlList = db.SqlList<int>(sql);
                    var dblist = db.SelectByIds<ParsedPage>(sqlList)
                        .OrderByDescending(x=>x.DateArticle)
                        .Skip(request.CurentPage * request.TotalArticlePerPage)
                        .Take(request.TotalArticlePerPage).ToList();

                    return new SearchRespounce { TotalArticles = sqlList.Count, DbList = dblist };
                }
                
            }
            else
            {

                return new SearchRespounce { TotalArticles = 0, DbList = null };
            }

        }
        public object Any(ShowArticles request)
        {
            var sqlList = Db.Select<ParsedPage>();
            var articles = Db.Select<ParsedPage>()
                .OrderByDescending(x => x.DateArticle)
                .Skip(request.CurrentPage * request.TotalArticlePerPage)
                .Take(request.TotalArticlePerPage).ToList();

            return new ShowArticlesRespounce { TotalArticles = sqlList.Count, DbList = articles };



        }
        public async Task Any(ParsingPage request)
        {
            var pathPage = "https://www.mlyn.by/novosti/";
            await DemoSimpleCrawler(pathPage);
            
            
        }
        
        public async Task DemoSimpleCrawler(string myUri)
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 3000,
                MinCrawlDelayPerDomainMilliSeconds = 3000
            };

            var crawler = new PoliteWebCrawler(config);
            crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;
            var crawlResult = await crawler.CrawlAsync(new Uri(myUri));





        }

        public void AngelSharp(CrawledPage crp)
        {
            var angleSharpHtmlDocument = crp.AngleSharpHtmlDocument;

            try
            {

                var blockArticle = angleSharpHtmlDocument.Body.QuerySelector("[class='post clearfix']");

                if (blockArticle == null )
                    return;

                var tittleArticle = blockArticle.GetElementsByClassName("post-title").FirstNonDefault().TextContent;
                var dateArticle = blockArticle.GetElementsByClassName("meta post-meta").FirstNonDefault().Children.FirstNonDefault().TextContent;
                var textArticle = blockArticle.GetElementsByClassName("post-body").FirstNonDefault().TextContent;
                var urlArticle = crp.HttpRequestMessage.RequestUri.OriginalString;
                var htmlArticle = crp.Content.Text;

                var parsedPage = new ParsedPage
                {

                    UrlArticle = urlArticle,
                    HtmlArticle = htmlArticle,
                    TitleArticle = ScrubHtml(tittleArticle),
                    DateArticle = DateTime.Parse(dateArticle),
                    TextArticle = ScrubHtml(textArticle)
                };
                Db.Open();
                var longID = Db.Insert(parsedPage, selectIdentity: true);

                Processor processor = ProcessorService.CreateProcessor();
                SourceOfAnalysis sofa = new SourceOfAnalysis(ScrubHtml(textArticle));
                AnalysisResult resaultPulleniParse = processor.Process(sofa);

                foreach (var k in resaultPulleniParse.Entities)
                {
                    if (k.TypeName == "GEO" || k.TypeName == "ORGANIZATION" || k.TypeName == "PERSON")
                    {
                        var entitisValue = new Entitis
                        {
                            TypeEntitis = k.TypeName,
                            ValueEntitis = k.ToString(),
                            ParsedPageId = (int)longID
                        };
                        Db.Insert(entitisValue);
                    }
                }
                Db.Close();
            }
            catch{}
        }
        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {

            CrawledPage crawledPage = e.CrawledPage;
            if (crawledPage.HttpRequestMessage != null)
            {
                Console.WriteLine(crawledPage.HttpRequestMessage.RequestUri.OriginalString + " is parsed");
                AngelSharp(crawledPage);
            }

        }
        public static string ScrubHtml(string value)
        {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }


    }
}
