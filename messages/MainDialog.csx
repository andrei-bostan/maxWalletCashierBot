#load "BasicForm.csx"

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

/// This dialog is the main bot dialog, which will call the Form Dialog and handle the results
[Serializable]
public class MainDialog : IDialog<BasicForm>
{
    public MainDialog()
    {
    }

    public Task StartAsync(IDialogContext context)
    {
        context.Wait(MessageReceivedAsync);
        return Task.CompletedTask;
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        context.Call(BasicForm.BuildFormDialog(FormOptions.PromptInStart), FormComplete);
    }

    private async Task FormComplete(IDialogContext context, IAwaitable<BasicForm> result)
    {
        try
        {
            var form = await result;
            if (form != null)
            {
                
                var cashiersList = Helper.GetCashiers();
                string cashier = cashiersList.FirstOrDefault();
                if(form.Event == "Deposit")
                {
                    Deposit deposit = new Deposit();
                    deposit.RecipientEmailAddress = form.Email;
                    deposit.CashierEmailAddress = cashier;
                    deposit.Sum = form.Sum;
                    //Helper.PostDeposit(deposit);
                }
                await context.PostAsync("Thanks for depositing " + form.Email + "! The cashier at which you deposited is: " + cashier);
            }
            else
            {
                await context.PostAsync("Form returned empty response! Type anything to restart it.");
            }
        }
        catch (OperationCanceledException)
        {
            await context.PostAsync("You canceled the form! Type anything to restart it.");
        }

        context.Wait(MessageReceivedAsync);
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

    //public async static void PostDeposit(Deposit deposit)
    //{
    //    string URL_Domain = "http://walletapi20170810041706.azurewebsites.net/api/";
    //    string Url = URL_Domain + "Deposit";

    //    using (var client = new HttpClient())
    //    {
    //        var values = new Dictionary<string, string>
    //            {
    //                { "RecipientEmailAddress", deposit.RecipientEmailAddress },
    //                { "CashierEmailAddress", deposit.CashierEmailAddress },
    //                { "Sum", deposit.Sum.ToString()}
    //            };

    //        var content = new FormUrlEncodedContent(values);

    //        var response = await client.PostAsync(Url, content);

    //        var responseString = await response.Content.ReadAsStringAsync();
    //    }
    //}
}

public class Cashier
{
    public int Id { get; set; }
    public string EmailAddress { get; set; }
}

public class Deposit
{
    public string RecipientEmailAddress { get; set; }
    public string CashierEmailAddress { get; set; }
    public int Sum { get; set; }
}