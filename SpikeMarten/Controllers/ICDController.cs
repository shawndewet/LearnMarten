using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using LOTRShared.Domain;
using LOTRShared.Commands;
using System.Globalization;

namespace SpikeMarten.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ICDController : ControllerBase
    {
        private readonly IQuerySession _session;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ICDController(IQuerySession session, IWebHostEnvironment webHostEnvironment)
        {
            _session = session;
            _hostingEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<ICDModel> CreateICDEntry([FromBody] ICDModel model,
            [FromServices] IDocumentStore store)
        {
            // Open a session for querying, loading, and
            // updating documents
            using (var session = store.LightweightSession())
            {
                var rec = new ICDRecord
                {
                    Id = model.Code,
                    Code = model.Code,
                    Description = model.Description
                };
                session.Store(rec);

                await session.SaveChangesAsync();
                return model;
            }
        }

        [HttpPost("Upload")]
        public async Task<int> UploadFromCSV([FromServices] IDocumentStore store)
        {
            IEnumerable<ICDRecord> records;
            using (var reader = new StreamReader(Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "content", "ICD10Codes.csv")))
            using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CsvMap>();
                records = csv.GetRecords<ICDRecord>();
                await store.BulkInsertAsync(records.ToList());
            }

            return await _session.Query<ICDRecord>().CountAsync();
        }

        [HttpGet()]
        public async Task<List<ICDRecord>> GetRecords()
        {
            return (List<ICDRecord>)await _session
                .Query<ICDRecord>()
                .ToListAsync();
        }


        [HttpGet("fulltextsearch")]
        public async Task<List<ICDRecord>> Search([FromQuery] string term)
        {
            return (List<ICDRecord>) await _session
                .Query<ICDRecord>()
                .Where(x => x.Description.Search(term))
                .ToListAsync();
        }


        [HttpGet("plaintextsearch")]
        public async Task<List<ICDRecord>> PlainTextSearch([FromQuery] string term)
        {
            return (List<ICDRecord>)await _session
                .Query<ICDRecord>()
                .Where(x => x.Description.PlainTextSearch(term))
                .ToListAsync();
        }


        [HttpGet("phrasesearch")]
        public async Task<List<ICDRecord>> PhraseSearch([FromQuery] string term)
        {
            return (List<ICDRecord>)await _session
                .Query<ICDRecord>()
                .Where(x => x.Description.PhraseSearch(term))
                .ToListAsync();
        }

        [HttpGet("webstylesearch")]
        public async Task<List<ICDRecord>> WebStyleSearch([FromQuery] string term)
        {
            return (List<ICDRecord>)await _session
                .Query<ICDRecord>()
                .Where(x => x.Description.WebStyleSearch(term))
                .ToListAsync();
        }

        [HttpGet("ngramsearch")]
        public async Task<List<ICDRecord>> NGramSearch([FromQuery] string term)
        {
            return (List<ICDRecord>)await _session
                .Query<ICDRecord>()
                .Where(x => x.Description.NgramSearch(term))
                .ToListAsync();

        }

    }

    public class CsvMap : CsvHelper.Configuration.ClassMap<ICDRecord>
    {
        public CsvMap()
        {
            //ICKICD,ICXICD,ICXLNG
            Map(m => m.Id).Name("ICKICD");
            Map(m => m.Code).Name("ICKICD");
            Map(m => m.Description).Name("ICXICD");
        }
    }
}
