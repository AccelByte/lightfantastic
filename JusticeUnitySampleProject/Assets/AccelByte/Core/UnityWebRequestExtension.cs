// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using AccelByte.Models;
using UnityEngine.Networking;
using Utf8Json;

namespace AccelByte.Core
{
    public static class UnityWebRequestExtension
    {
        public static Result TryParseResponse(this UnityWebRequest request)
        {
            if (request == null)
            {
                return Result.CreateError(ErrorCode.NetworkError);
            }

            switch (request.responseCode)
            {
            case (long)HttpStatusCode.OK:
            case (long)HttpStatusCode.Created:
            case (long)HttpStatusCode.Accepted:
            case (long)HttpStatusCode.NoContent:
            case (long)HttpStatusCode.Ambiguous:
            case (long)HttpStatusCode.Moved:
            case (long)HttpStatusCode.Redirect:
            case (long)HttpStatusCode.SeeOther:
            case (long)HttpStatusCode.NotModified:
            case (long)HttpStatusCode.UseProxy:
            case (long)HttpStatusCode.Unused:
            case (long)HttpStatusCode.RedirectKeepVerb:

                return Result.CreateOk();
            case (long)HttpStatusCode.BadRequest:
            case (long)HttpStatusCode.Unauthorized:
            case (long)HttpStatusCode.PaymentRequired:
            case (long)HttpStatusCode.Forbidden:
            case (long)HttpStatusCode.NotFound:
            case (long)HttpStatusCode.MethodNotAllowed:
            case (long)HttpStatusCode.NotAcceptable:
            case (long)HttpStatusCode.ProxyAuthenticationRequired:
            case (long)HttpStatusCode.RequestTimeout:
            case (long)HttpStatusCode.Conflict:
            case (long)HttpStatusCode.Gone:
            case (long)HttpStatusCode.LengthRequired:
            case (long)HttpStatusCode.PreconditionFailed:
            case (long)HttpStatusCode.RequestEntityTooLarge:
            case (long)HttpStatusCode.RequestUriTooLong:
            case (long)HttpStatusCode.UnsupportedMediaType:
            case (long)HttpStatusCode.RequestedRangeNotSatisfiable:
            case (long)HttpStatusCode.ExpectationFailed:

                if (string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    return Result.CreateError((ErrorCode)request.responseCode);
                }

                try
                {
                    var error = JsonSerializer.Deserialize<ServiceError>(request.downloadHandler.text);

                    if (error.numericErrorCode == 0)
                    {
                        return Result.CreateError((ErrorCode)request.responseCode);
                    }

                    return Result.CreateError((ErrorCode)error.numericErrorCode, error.errorMessage);
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    var err = JsonSerializer.Deserialize<OAuthError>(request.downloadHandler.text);
                    string message = err.error + ": " + err.error_description;

                    return Result.CreateError((ErrorCode)request.responseCode, message);
                }
                catch (Exception)
                {
                    // ignored
                }

                return Result.CreateError((ErrorCode)request.responseCode);
            default:

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    return Result.CreateError(
                        (ErrorCode)request.responseCode,
                        "Unknown Service Error: " + request.downloadHandler.text);
                }
                else
                {
                    return Result.CreateError((ErrorCode)request.responseCode);
                }
            }
        }

        public static Result<T> TryParseResponseJson<T>(this UnityWebRequest request)
        {
            if (request == null)
            {
                return Result<T>.CreateError(ErrorCode.NetworkError, "There is no response.");
            }

            string message;
            string responseText = request.downloadHandler.text;

            switch (request.responseCode)
            {
            case (long)HttpStatusCode.OK:
            case (long)HttpStatusCode.Created:

                try
                {
                    var createResponse = JsonSerializer.Deserialize<T>(responseText);

                    return Result<T>.CreateOk(createResponse);
                }
                catch (Exception ex)
                {
                    return Result<T>.CreateError(ErrorCode.ErrorFromException, ex.Message);
                }
            case (long)HttpStatusCode.Accepted:
            case (long)HttpStatusCode.NoContent:
            case (long)HttpStatusCode.ResetContent:
            case (long)HttpStatusCode.PartialContent:
            case (long)HttpStatusCode.Ambiguous:
            case (long)HttpStatusCode.Moved:
            case (long)HttpStatusCode.Redirect:
            case (long)HttpStatusCode.SeeOther:
            case (long)HttpStatusCode.NotModified:
            case (long)HttpStatusCode.UseProxy:
            case (long)HttpStatusCode.Unused:
            case (long)HttpStatusCode.RedirectKeepVerb:
                message = "JSON response body expected but instead found HTTP Response with Status " + 
                          request.responseCode;
                return Result<T>.CreateError(ErrorCode.InvalidResponse, message);
            case (long)HttpStatusCode.BadRequest:
            case (long)HttpStatusCode.Unauthorized:
            case (long)HttpStatusCode.PaymentRequired:
            case (long)HttpStatusCode.Forbidden:
            case (long)HttpStatusCode.NotFound:
            case (long)HttpStatusCode.MethodNotAllowed:
            case (long)HttpStatusCode.NotAcceptable:
            case (long)HttpStatusCode.ProxyAuthenticationRequired:
            case (long)HttpStatusCode.RequestTimeout:
            case (long)HttpStatusCode.Conflict:
            case (long)HttpStatusCode.Gone:
            case (long)HttpStatusCode.LengthRequired:
            case (long)HttpStatusCode.PreconditionFailed:
            case (long)HttpStatusCode.RequestEntityTooLarge:
            case (long)HttpStatusCode.RequestUriTooLong:
            case (long)HttpStatusCode.UnsupportedMediaType:
            case (long)HttpStatusCode.RequestedRangeNotSatisfiable:
            case (long)HttpStatusCode.ExpectationFailed:

                if (string.IsNullOrEmpty(responseText))
                {
                    return Result<T>.CreateError((ErrorCode) request.responseCode);
                }

                try
                {
                    var error = JsonSerializer.Deserialize<ServiceError>(responseText);

                    if (error.numericErrorCode == 0)
                    {
                        return Result<T>.CreateError((ErrorCode) request.responseCode);
                    }

                    return Result<T>.CreateError((ErrorCode) error.numericErrorCode, error.errorMessage);
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    var err = JsonSerializer.Deserialize<OAuthError>(responseText);
                    message = err.error + ": " + err.error_description;

                    return Result<T>.CreateError((ErrorCode) request.responseCode, message);
                }
                catch (Exception)
                {
                    // ignored
                }

                return Result<T>.CreateError((ErrorCode) request.responseCode);
            default:

                if (!string.IsNullOrEmpty(responseText))
                {
                    return Result<T>.CreateError(
                        (ErrorCode) request.responseCode,
                        "Unknown Service Error: " + responseText);
                }
                else
                {
                    return Result<T>.CreateError((ErrorCode) request.responseCode);
                }
            }
        }

        public static byte[] GetBodyRaw(this HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[1024];
                    int read;

                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memoryStream.Write(buffer, 0, read);
                    }

                    memoryStream.Flush();

                    return memoryStream.ToArray();
                }
            }
        }
    }
}
