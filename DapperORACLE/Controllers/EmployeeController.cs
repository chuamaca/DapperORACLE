using DapperORACLE.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperORACLE.Controllers
{
    [Produces("application/json")]
    public class EmployeeController : ControllerBase
    {

        IEmployeeRepository employeeRepository;
        public EmployeeController(IEmployeeRepository _employeeRepository)
        {
            employeeRepository = _employeeRepository;
        }

     

        [Route("api/GetCompaniaListar")]
        public ActionResult PostCompaniaListar()
        {
            var result = employeeRepository.GetCompaniaListar();

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }



    }
}
