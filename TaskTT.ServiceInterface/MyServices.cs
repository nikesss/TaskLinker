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
using ServiceStack.OrmLite;
using TaskTT.ServiceModel;
using TaskTT.ServiceModel.Types;

namespace TaskTT.ServiceInterface
{

    public class ParsePage : Service
    {
        public object Any(Search request)
        {

            if (request.AtributeSearch != null && request.TypeSearch == "byarticle")
            {
                var dblist = Db.From<ParsedPage>()
                .Where<ParsedPage>(x => x.TextArticle.Contains($"{request.AtributeSearch}"));
                
                return  Db.Select(dblist);
            }
            else if (request.AtributeSearch != null && request.TypeSearch == "pullenti")
            {

                var sql = Db.From<Entitis>()
                    .Where(x => x.ValueEntitis.Contains(request.AtributeSearch)).SelectDistinct(x=>x.ParsedPageId);
                var sqlList = Db.SqlList<int>(sql);
                var dblist = Db.SelectByIds<ParsedPage>(sqlList);

                return dblist;
            }
            else
            {
                return new SearchRespounce { Result = "Not Found" };
            }

        }
        public async Task Any(ParsingdPage request)
        {
            var pathPage = "https://www.mlyn.by/novosti/";
            await DemoSimpleCrawler(pathPage);
        }
        public object Any(ShowArticles request)
        {

            
            return Db.Select<ParsedPage>().OrderByDescending(x=>x.DateArticle);


        }
        public async Task DemoSimpleCrawler(string myUri)
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 5000,
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

                if (blockArticle == null)
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
