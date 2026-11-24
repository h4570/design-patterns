using Microsoft.AspNetCore.Mvc;
using proxy.Documents;

namespace proxy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentsController : ControllerBase
    {

        // Controller depends on the interface only - it has no idea that it gets a proxy
        private readonly IDocumentStorage _documents;

        public DocumentsController(IDocumentStorage documents)
        {
            _documents = documents;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<DocumentInfo>> List() => Execute(() => _documents.List());

        [HttpGet("{id:int}")]
        public ActionResult<Document> Get(int id) => Execute(() => _documents.Get(id));

        [HttpDelete("{id:int}")]
        public ActionResult<string> Delete(int id) => Execute(() =>
        {
            _documents.Delete(id);
            return $"Document {id} deleted";
        });

        /// <summary>
        /// Shows whether the expensive real storage was created already (virtual proxy).
        /// </summary>
        [HttpGet("storage-status")]
        public ActionResult<object> StorageStatus([FromServices] Lazy<CloudDocumentStorage> storage) =>
            Ok(new { RealStorageCreated = storage.IsValueCreated });

        private ActionResult<T> Execute<T>(Func<T> action)
        {
            try
            {
                return Ok(action());
            }
            catch (NotAuthenticatedException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status401Unauthorized); }
            catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
            catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        }

    }
}
