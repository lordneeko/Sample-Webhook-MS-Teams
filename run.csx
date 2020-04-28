#r "Newtonsoft.Json"

using System.Net;
//using System.Net.WebRequest;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;


public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    
    string id = req.Query["id"];
    string text = null;
    string queryString = "</at>";
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    
    log.LogInformation("Raw Request: " + requestBody);
    
    // Get the MS Teams Function call and message sent by the user
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    id = id ?? data?.id;
    text = text ?? data?.text;
    Response res = new Response();
    res.type = "message";
    res.text = "No Response";
    
    var satOutput = "";
    
    if (text != null)
    {
        log.LogInformation("id: " + id);
        log.LogInformation("Input Text: " + text); 
        // Need to remove the MS Team's bot tag and extraneous characters from the body of the message
        string message = getTextAfterString(text, queryString).Trim().Replace("&nbsp;", "");
        log.LogInformation("Message: " + message);
        
        // Do the Satellite Query
        var siteQuery = "https://www.n2yo.com/rest/v1/satellite/tle/" + message + "&apiKey=X5YN4E-D4PXUE-WDFWUH-48NO";
        HttpClient client = new HttpClient();
		    HttpResponseMessage response = await client.GetAsync(siteQuery);
		    response.EnsureSuccessStatusCode();
		    satOutput = await response.Content.ReadAsStringAsync();
        // Do the Satellite Query
        
        
        res.text = satOutput;
    }
    return text != null
        ? (ActionResult)new OkObjectResult(res)
        : new BadRequestObjectResult("Please pass an id in the request body");
}

// Pull out the message in the inputString after the searchString
private static string getTextAfterString(string inputString, string searchString)
{
    var index = inputString.IndexOf(searchString);
    if (index != -1 )
        return inputString.Substring(index + searchString.Length);
    else
        return inputString;
} 

// Class to format response to MS Teams. Minimum required JSON fields are type and text
public class Response {
    public string type;
    public string text;
}

// Future classes to de-serialize JSON response from n2yo.com
//public class SatResponse {
//    public string info;
//    public string 

//}
