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

        /// <exception cref="T:System.NotImplementedException"></exception>
        [HttpGet("create")]
        public ActionResult<Croissant> CreateChocolateCroissant()
        {
            var customization = new CroissantCustomization()
            {
                Chocolate = CroissantChocolateAmount.DoubleChocolate
            };
            var croissant = CroissantFactory
                .GetFactory(CroissantType.Chocolate)
                .GetCroissant(_machineService, customization);
            croissant.Make();
            return Ok(croissant);
        }

    }
}
