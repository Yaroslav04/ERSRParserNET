using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace ERSRParserNET
{
    public class ParseERSRPage
    {
        private int pages;
        private int sort; //1 по убыванию // 0 ревалантность
        private List<HeaderClass> inputHeaders;
        private List<string> inputCourts;
        private string url = "https://reyestr.court.gov.ua/";
        public ParseERSRPage()
        {
            inputCourts = new List<string>();
            inputHeaders = new List<HeaderClass>();
            pages = 1000;
            sort = 1;
        }
        public ParseERSRPage(int _page, int _sort)
        {
            inputCourts = new List<string>();
            inputHeaders = new List<HeaderClass>();
            pages = _page;
            sort = _sort;
        }

        public void AddHeader(string _header, string _value)
        {
            inputHeaders.Add(new HeaderClass
            {
                Header = _header,
                Content = _value
            });
        }

        public void AddSearchHeader(string _value)
        {
            inputHeaders.Add(new HeaderClass
            {
                Header = "SearchExpression",
                Content = _value
            });
        }

        public void AddCaseHeader(string _value)
        {
            inputHeaders.Add(new HeaderClass
            {
                Header = "CaseNumber",
                Content = _value
            });
        }

        public void AddCourt(string _court)
        {
            inputCourts.Add(_court);
        }

        public void SetLocalCourts()
        {
            inputCourts = new List<string> { "305", "309", "315" };
        }

        public void SetLocalFullCourts()
        {
            inputCourts = new List<string> { "305", "309", "315", "1054" };
        }

        private List<Dictionary<string, string>> GetHeaders()
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

            if (inputCourts.Count > 0)
            {
                
                foreach (var court in inputCourts)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("PagingInfo.ItemsPerPage", pages.ToString());
                    headers.Add("Sort", sort.ToString());
                    headers.Add("CSType", "2");
                    headers.Add("CourtName", court);
                    if (inputHeaders.Count > 0)
                    {
                        foreach (var input in inputHeaders)
                        {
                            headers.Add(input.Header, input.Content);
                        }
                    }
                    result.Add(headers);
                }
            }
            else
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("PagingInfo.ItemsPerPage", pages.ToString());
                headers.Add("Sort", sort.ToString());
                headers.Add("CSType", "2");
                if (inputHeaders.Count > 0)
                {
                    foreach (var input in inputHeaders)
                    {
                        headers.Add(input.Header, input.Content);
                    }
                }
                result.Add(headers);
            }

            return result;
        }

        public async Task<List<ERSRClass>> GetERSRPageList()
        {
            List<ERSRClass> result = new List<ERSRClass>();
            foreach(var header in GetHeaders())
            {
                var subresult = GetERSRFromHTML(await GetHTML(header));
                if (subresult.Count > 0)
                {
                    result.AddRange(subresult);
                }
            }   

            result = result.Distinct().ToList();
            return result;
        }

        async Task<string> GetHTML(Dictionary<string, string> _headers)
        {
            HttpClient client = new HttpClient();

            var content = new FormUrlEncodedContent(_headers);

            var response = await client.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        private List<ERSRClass> GetERSRFromHTML(string _html)
        {
            var lines = _html.Split('\n');

            List<string> container = new List<string>();
            List<ERSRClass> list = new List<ERSRClass>();

            bool containerSensor = false;

            foreach (string line in lines)
            {
                if (line.Contains("<tr>"))
                {
                    containerSensor = true;
                }

                if (line.Contains("</tr>"))
                {
                    container.Add(line);
                    if (container.Count > 0)
                    {
                        bool sw = false;
                        foreach (var _c in container)
                        {
                            if (_c.Contains("/Review/"))
                            {
                                sw = true;
                            }
                        }

                        if (sw)
                        {
                            ERSRClass decision = new ERSRClass();

                            Regex regex = new Regex(@"(/Review/)\d*");
                            MatchCollection matches = regex.Matches(container[2]);
                            if (matches.Count > 0)
                            {
                                decision.Id = matches[0].Value.Replace("/Review/", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                                decision.URL = $"https://reyestr.court.gov.ua/Review/{matches[0].Value.Replace("/Review/", "").Replace("\t", "").Replace("\n", "").Replace("\r", "")}";
                            }

                            regex = new Regex(@"(>)\w+");
                            matches = regex.Matches(container[4]);
                            if (matches.Count > 0)
                            {
                                decision.DecisionType = matches[0].Value.Replace(">", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                            }

                            regex = new Regex(@"\d\d(.)\d\d(.)\d\d\d\d");
                            matches = regex.Matches(container[6]);
                            if (matches.Count > 0)
                            {
                                decision.DecisionDate = Convert.ToDateTime(matches[0].Value.Replace("\t", "").Replace("\n", "").Replace("\r", ""));
                            }

                            regex = new Regex(@"\d\d(.)\d\d(.)\d\d\d\d");
                            matches = regex.Matches(container[8]);
                            if (matches.Count > 0)
                            {
                                decision.PublicDate = Convert.ToDateTime(matches[0].Value.Replace("\t", "").Replace("\n", "").Replace("\r", ""));
                            }

                            regex = new Regex(@"(>)\w+");
                            matches = regex.Matches(container[10]);
                            if (matches.Count > 0)
                            {
                                decision.JudiciaryType = matches[0].Value.Replace(">", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                            }

                            //regex = new Regex(@"(>)\w+(/)\w+(/)\w+");
                            regex = new Regex(@"(>)(\S*)");
                            matches = regex.Matches(container[12]);
                            if (matches.Count > 0)
                            {
                                decision.Case = matches[0].Value.Replace(">", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                            }

                            regex = new Regex(@"(>)(\D*)");
                            matches = regex.Matches(container[14]);
                            if (matches.Count > 0)
                            {
                                decision.Court = matches[0].Value.Replace(">", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                            }

                            regex = new Regex(@"(>)(\D*)");
                            matches = regex.Matches(container[18]);
                            if (matches.Count > 0)
                            {
                                decision.Judge = matches[0].Value.Replace(">", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                            }


                            if (decision.Id != "")
                            {
                                list.Add(decision);
                            }
                        }
                    }
                    container.Clear();
                    containerSensor = false;
                }

                if (containerSensor == true)
                {
                    container.Add(line);
                }
            }

            return list;
        }
    }

    public class HeaderClass
    {
        public string Header { get; set; }
        public string Content { get; set; }
        public HeaderClass()
        {
            Header = "";
            Content = "";
        }

        public HeaderClass(string _header, string _content)
        {
            Header = _header;
            Content = _content;
        }
    }

}