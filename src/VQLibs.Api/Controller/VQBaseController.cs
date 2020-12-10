using Microsoft.AspNetCore.Mvc;

namespace VQLibs.Api.Controller
{
    public abstract class VQBaseController : ControllerBase
    {
        protected IActionResult DefaultResponse(object data = null, bool success = true)
        {
            if (success)
            {
                return data != null
                    ? (IActionResult)Ok(data)
                    : Ok();
            }
            else
            {
                return data != null
                    ? (IActionResult)BadRequest(data)
                    : BadRequest();
            }
        }
    }
}
