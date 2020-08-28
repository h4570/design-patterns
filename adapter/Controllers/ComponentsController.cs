using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adapter.Adapters;
using adapter.Adapters.ElectronicComponent;
using adapter.Extensions;
using adapter.Models;
using adapter.Models.WarehouseZ1;
using adapter.Models.WarehouseZ2;
using adapter.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace adapter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComponentsController : ControllerBase
    {

        private readonly Z1ComponentsRepository _z1ComponentsRepo;
        private readonly Z2ComponentsRepository _z2ComponentsRepo;

        public ComponentsController()
        {
            _z1ComponentsRepo = new Z1ComponentsRepository();
            _z2ComponentsRepo = new Z2ComponentsRepository();
        }

        [HttpGet]
        public ActionResult<IEnumerable<IElectronicComponent>> Get()
        {
            var result = new List<IElectronicComponent>();

            var z1Adapter = new Z1ElectronicComponentAdapter();
            var z1Components = _z1ComponentsRepo.GetAll();
            result.AddRange(z1Components.ToElectronicComponents(z1Adapter));

            var z2Adapter = new Z2ElectronicComponentAdapter();
            var z2Components = _z2ComponentsRepo.GetAll();
            result.AddRange(z2Components.ToElectronicComponents(z2Adapter));

            return Ok(result);
        }

    }
}
