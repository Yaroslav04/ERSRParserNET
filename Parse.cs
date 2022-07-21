using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ERSRParserNET
{
    // TODO добавить реализацию поиска с судами и без них, добавить в класс текст самого решения и номер уголовного дела, добавить фильтры по типу судопроизводства и т.п.
    public class Parse
    {
        public int Pages = 500;
        public List<string> Tegs;
        public ERSRClass ERSRSearchClass;
        private string URL = "https://reyestr.court.gov.ua/";
        public List<string> NativeCourts;

        public Parse()
        {
            Tegs = new List<string>();
            NativeCourts = new List<string>
            {
                "305", "309", "315"
            };
        }

        public async Task<List<ERSRClass>> GetERSRListByTegs(List<string> _tegs, List<string> _courts)
        {
            List<ERSRClass> result = new List<ERSRClass>();
            foreach (var court in _courts)
            {
                foreach (string teg in _tegs)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>
                    {
                      { "PagingInfo.ItemsPerPage", Pages.ToString() }, { "Sort", "1" }, { "SearchExpression", teg},
                      { "CourtName", court }
                    };

                    var subresult = GetERSRFromHTML(await GetHTMLByHeaders(headers));
                    if (subresult.Count > 0)
                    {
                        result.AddRange(subresult);
                    }
                }
            }

            result.Distinct();
            return result;

        }

        async Task<string> GetHTMLByHeaders(Dictionary<string, string> _headers)
        {
            HttpClient client = new HttpClient();

            List<ERSRClass> _list = new List<ERSRClass>();



            var content = new FormUrlEncodedContent(_headers);

            var response = await client.PostAsync(URL, content);

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
                                decision.Id = matches[0].Value.Replace("/Review/", "");
                            }

                            regex = new Regex(@"(>)\w+");
                            matches = regex.Matches(container[4]);
                            if (matches.Count > 0)
                            {
                                decision.DecisionType = matches[0].Value.Replace(">", "");
                            }

                            regex = new Regex(@"\d\d(.)\d\d(.)\d\d\d\d");
                            matches = regex.Matches(container[6]);
                            if (matches.Count > 0)
                            {
                                decision.DecisionDate = Convert.ToDateTime(matches[0].Value);
                            }

                            regex = new Regex(@"\d\d(.)\d\d(.)\d\d\d\d");
                            matches = regex.Matches(container[8]);
                            if (matches.Count > 0)
                            {
                                decision.PublicDate = Convert.ToDateTime(matches[0].Value);
                            }

                            regex = new Regex(@"(>)\w+");
                            matches = regex.Matches(container[10]);
                            if (matches.Count > 0)
                            {
                                decision.JudiciaryType = matches[0].Value.Replace(">", "");
                            }

                            //regex = new Regex(@"(>)\w+(/)\w+(/)\w+");
                            regex = new Regex(@"(>)(\S*)");
                            matches = regex.Matches(container[12]);
                            if (matches.Count > 0)
                            {
                                decision.Case = matches[0].Value.Replace(">", "");
                            }

                            regex = new Regex(@"(>)(\D*)");
                            matches = regex.Matches(container[14]);
                            if (matches.Count > 0)
                            {
                                decision.Court = matches[0].Value.Replace(">", "");
                            }

                            regex = new Regex(@"(>)(\D*)");
                            matches = regex.Matches(container[18]);
                            if (matches.Count > 0)
                            {
                                decision.Judge = matches[0].Value.Replace(">", "");
                            }

                            if (decision.Case != "")
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
}