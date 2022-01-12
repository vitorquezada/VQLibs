using System.Net;
using Microsoft.AspNetCore.Mvc;
using VQLib.Business.Model;

namespace VQLib.Api.Controller
{
    public abstract class VQBaseController : ControllerBase
    {
        protected IActionResult DefaultResponse<T>(VQBaseResponse<T> data)
        {
            return StatusCode(
                data.HasErrorValidations
                    ? (int)HttpStatusCode.BadRequest
                    : (int)HttpStatusCode.OK,
                data);
        }
    }
}