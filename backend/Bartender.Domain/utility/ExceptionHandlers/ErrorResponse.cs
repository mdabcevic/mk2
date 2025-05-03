using Bartender.Domain.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class ErrorResponse
{
    public string Error = string.Empty;
    public object? Data;
    public int StatusCode;

    public ErrorResponse() { }
    public ErrorResponse(string error, int statusCode, object data = null)
    {
        Error = error;
        StatusCode = statusCode;
        Data = data;
    }
}
