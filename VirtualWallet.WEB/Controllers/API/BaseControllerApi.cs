﻿using Microsoft.AspNetCore.Mvc;
using VirtualWallet.BUSINESS.Results;
using VirtualWallet.DATA.Models;

namespace VirtualWallet.WEB.Controllers.API
{
    [ApiController]
    public abstract class BaseControllerApi : ControllerBase
    {
        protected User CurrentUser => HttpContext.Items["CurrentUser"] as User;

        protected IActionResult HandleAuthorization()
        {
            if (HttpContext.Items.ContainsKey("AuthorizationResult"))
            {
                var result = HttpContext.Items["AuthorizationResult"] as Result;

                if (result != null && !result.IsSuccess)
                {
                    return new ObjectResult(new { Error = result.Error })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            return null; 
        }
    }
}
