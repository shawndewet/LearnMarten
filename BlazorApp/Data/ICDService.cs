using LOTRShared.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BlazorApp.Data
{
    public class ICDService
    {
        private readonly IDocumentStore _store;
        //bool queryInProgress = false;
        //List<ICDRecord> matchedRecords = new();
        string codePattern = @"^[A-Z]\d{2}\.\d{1,2}$";
        
        public ICDService(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<List<ICDRecord>> SearchICDRecords(string term)
        {
            term = term.Trim();
            if (term.Length < 2)
                return new List<ICDRecord>();

            //if (queryInProgress)
            //    return matchedRecords; //return last match

            //try
            //{
               // queryInProgress = true;

                using (var session = _store.LightweightSession())
                {
                    if (term.Any(char.IsDigit))
                    {
                        //user is entering a number...so we are dealing with a Code (as opposed to a description)
                        if (Regex.IsMatch(term, codePattern))
                        {
                            //user has entered a full code (as opposed to a partial code)
                            return (List<ICDRecord>)await session
                                .Query<ICDRecord>()
                                .Where(x => x.Code == term)
                                .ToListAsync();
                        }
                        else
                        {
                            return (List<ICDRecord>)await session
                                .Query<ICDRecord>()
                                .Where(x => x.Code.StartsWith(term))
                                .ToListAsync();
                        }
                    }
                    else
                        return (List<ICDRecord>)await session
                        .Query<ICDRecord>()
                        .Where(x => x.Description.NgramSearch(term))
                        .ToListAsync();
                }

                //return matchedRecords;
            //}
            //finally
            //{
            //    queryInProgress = false;
            //}

        }

    }
}