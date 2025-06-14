﻿using System.Text.Json.Serialization;

namespace BartenderBackend.ExceptionHandlers;

public class ErrorResponse
{
    public string Error { get; set; } = "Unknown error occurred";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }
    public int StatusCode { get; set; }

    public ErrorResponse() { }
    public ErrorResponse(string error, int statusCode, object? data = null)
    {
        Error = string.IsNullOrEmpty(error) ? "An unexpected error occurred" : error;
        StatusCode = statusCode;
        Data = data;
    }
}
