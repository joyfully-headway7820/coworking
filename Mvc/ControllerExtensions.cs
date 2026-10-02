using Microsoft.AspNetCore.Mvc;

namespace coworking.Mvc;

public static class ControllerExtensions
{
    public static ViewResult Missing(this Controller controller)
    {
        controller.Response.StatusCode = StatusCodes.Status404NotFound;
        return controller.View("NotFound");
    }
}
