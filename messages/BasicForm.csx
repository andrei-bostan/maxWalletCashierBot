using System;
using Microsoft.Bot.Builder.FormFlow;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public enum EventOptions { Deposit = 1, Withdraw };

// For more information about this template visit http://aka.ms/azurebots-csharp-form
[Serializable]
public class BasicForm
{
    [Prompt("Please select your activity {||}")]
    public EventOptions Event { get; set; }

    [Prompt("How much RON would you like to deposit? Please type a number:")]
    public int Sum { get; set; }

    //public string Cashiers;

    public static IForm<BasicForm> BuildForm()
    {
        // Builds an IForm<T> based on BasicForm
        return new FormBuilder<BasicForm>()
        .Field(nameof(BasicForm.Event))
        .Field(nameof(BasicForm.Sum))
        //.Field(new FieldReflector<BasicForm>(nameof(BasicForm.Cashiers))
        //            .SetType(null)
        //            .SetPrompt(PerLinePromptAttribute("Please select the cashier: {||}"))
        //            .SetDefine((state, field) =>
        //            {
        //                var cashiers = Helper.GetCashiers();
        //                foreach (var cashier in cashiers)
        //                {
        //                    field
        //                        .AddDescription(cashier, cashier)
        //                        .AddTerms(cashier, cashier);
        //                }

        //                return Task.FromResult(true);
        //            }))
        .AddRemainingFields()
        .Build()
        ;
    }

    public static IFormDialog<BasicForm> BuildFormDialog(FormOptions options = FormOptions.PromptInStart)
    {
        // Generated a new FormDialog<T> based on IForm<BasicForm>
        return FormDialog.FromForm(BuildForm, options);
    }
}

public static class Helper
{
    public static List<string> GetCashiers()
    {
        string URL_Domain = "http://walletapi20170810041706.azurewebsites.net/api/";
        var results = new List<string>();
        try
        {
            string Url = URL_Domain + "Cashier";

            HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            // Get response  
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                Stream responseStream = response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    // get the response as text
                    string responseText = reader.ReadToEnd();

                    // convert from text 
                    var cahierResults = JsonConvert.DeserializeObject<List<Cashier>>(responseText);
                    results = cahierResults.Select(c => c.EmailAddress).ToList();

                }
            }
        }

        catch (Exception es)
        {
        }

        return results;
    }
}

public class Cashier
{
    public int Id { get; set; }
    public string EmailAddress { get; set; }
}
