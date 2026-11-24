using Microsoft.AspNetCore.Mvc;
using template_method.Imports;

namespace template_method.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportController : ControllerBase
    {

        private readonly IReadOnlyDictionary<string, SupplierImport> _imports;
        private readonly ProductRepository _repository;

        public ImportController(IEnumerable<SupplierImport> imports, ProductRepository repository)
        {
            _imports = imports.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);
            _repository = repository;
        }

        [HttpGet("suppliers")]
        public ActionResult GetSuppliers() => Ok(_imports.Values.Select(c => new { c.Key, c.Supplier, c.SampleFile }));

        [HttpGet("products")]
        public ActionResult<IReadOnlyList<ImportedProduct>> GetProducts() => Ok(_repository.All());

        /// <summary>
        /// Raw file in the body, e.g. curl -X POST --data-binary @file.csv -H "Content-Type: text/plain" .../import/kowalski-csv
        /// </summary>
        [HttpPost("{supplier}")]
        public async Task<ActionResult<ImportReport>> Import(string supplier)
        {
            if (!_imports.TryGetValue(supplier, out var import))
                return NotFound($"Unknown supplier '{supplier}'. Available: {string.Join(", ", _imports.Keys)}");

            using var reader = new StreamReader(Request.Body);
            var content = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("File is empty");

            try
            {
                return Ok(import.Run(content));
            }
            catch (FormatException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Imports sample files of all suppliers - the same Run() for CSV, JSON and XML.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult Demo()
        {
            var reports = _imports.Values.Select(c => c.Run(c.SampleFile)).ToList();
            return Ok(new { Reports = reports, Products = _repository.All() });
        }

    }
}
