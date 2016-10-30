﻿using System.Net;
using RankOne.Attributes;
using RankOne.Models;

namespace RankOne.Analyzers.Performance
{
    [AnalyzerCategory(SummaryName = "Performance", Alias = "gzipanalyzer")]
    public class GZipAnalyzer : BaseAnalyzer
    {
        public override AnalyzeResult Analyse(PageData pageData)
        {
            string encoding = null;
            var request = (HttpWebRequest)WebRequest.Create(pageData.Url);
            request.Method = "GET";
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response != null)
                {
                    encoding = response.ContentEncoding;
                }
            }

            var result = new AnalyzeResult
            {
                Alias = "gzipanalyzer"
            };
            if (encoding == "gzip")
            {
                result.AddResultRule("gzipanalyzer_gzip_enabled", ResultType.Success);
            }
            else
            {
                result.AddResultRule("gzipanalyzer_gzip_disabled", ResultType.Hint);
            }

            return result;
        }
    }
}
