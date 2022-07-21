using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace ERSRParserNET
{
    public class Parse
    {
        private int pages;
        private int sort; //2 по убыванию // 0 ревалантность
        public Parse()
        {
            pages = 1000;
            sort = 2;
        }
        public Parse(int _page, int _sort)
        {
            pages = _page;
            sort = _sort;
        }

     
        private string url = "https://reyestr.court.gov.ua/";

        private List<Dictionary<string, string>> CreateHeaders(List<InputClass> _inputList)
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

            foreach (var _input in _inputList)
            {
                foreach (var court in _input.Courts)
                {

                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("PagingInfo.ItemsPerPage", pages.ToString());
                    headers.Add("Sort", sort.ToString());
                    headers.Add("CourtName", court);
                    headers.Add("CSType", "2");
                    //headers.Add( "CaseCat1", "40438"); //справи з 2019 року

                    if (_input.Input.Count > 0)
                    {
                        foreach (var item in _input.Input)
                        {
                            headers.Add(item.Key, item.Value);
                        }
                    }
                    result.Add(headers);
                }
            }

            return result;
        }

        public async Task<List<ERSRClass>> GetERSRListByTegs(List<InputClass> _inputList)
        {
            List<Dictionary<string, string>> _headers = CreateHeaders(_inputList);
            List<ERSRClass> result = new List<ERSRClass>();

            foreach (var headers in _headers)
            {
                var subresult = await GetERSRFromHTML(await GetHTML(headers));
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

            List<ERSRClass> _list = new List<ERSRClass>();

            var content = new FormUrlEncodedContent(_headers);

            var response = await client.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        private async Task<List<ERSRClass>> GetERSRFromHTML(string _html)
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
                                var criminalCases = await GetCriminalNumberFromCasePage(decision.Id);
                                if (criminalCases.Count > 0)
                                {
                                    decision.CriminalCase = criminalCases;
                                }
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

        private async Task<List<string>> GetCriminalNumberFromCasePage(string _id)
        {
            List<string> result = new List<string>();
            string url = $"https://reyestr.court.gov.ua/Review/{_id}";
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            Regex regex = new Regex(@"\d(20)\d\d\d\d\d\d\d\d\d\d\d\d\d\d");
            MatchCollection matches = regex.Matches(responseString);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    result.Add(match.Value);
                }
            }
            return result;
        }
    }

    public class InputClass
    {
        public Dictionary<string, string> Input;
        public List<string> Courts { get; set; }

        public InputClass()
        {
            Input = new Dictionary<string, string>();
            Courts = new List<string>
            {
                "305", "309", "315"
            };
        }

        // "305", "309", "315"
        // "1054"

    }

}