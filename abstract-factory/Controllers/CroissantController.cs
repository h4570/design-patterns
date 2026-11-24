using abstract_factory.CroissantAbstractFactory;
using abstract_factory.Services;
using Microsoft.AspNetCore.Mvc;

namespace abstract_factory.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CroissantController : ControllerBase
    {

        private readonly MachineService _machineService;

        public CroissantController()
        {
            _machineService = new MachineService();
        }

        [HttpGet("create")]
        public ActionResult<object> CreateChocolateCroissant()
        {
            var customization = new CroissantCustomization()
            {
                Chocolate = CroissantChocolateAmount.DoubleChocolate
            };
            var croissant = CroissantFactory
                .GetFactory(CroissantType.Chocolate)
                .GetCroissant(_machineService, customization);
            croissant.Make();
            // (object) - serialize runtime type, so ChocolateCroissant properties are visible too
            return Ok(new { Croissant = (object)croissant, MachineOperations = _machineService.Operations });
        }

    }
}
