#load "BasicForm.csx"

#r "Newtonsoft.Json"
using System.Collections;
using Newtonsoft.Json;

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
using System.Text;

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
                if (form.Event == EventOptions.Deposit)
                {
                    Deposit deposit = new Deposit();
                    deposit.RecipientEmailAddress = form.Email;
                    deposit.CashierEmailAddress = cashier;
                    deposit.Sum = form.Sum;
                    WalletDP(deposit);
                    await context.PostAsync("Thanks for depositing " + form.Email + "! The cashier at which you deposited is: " + cashier);
                }
                else if (form.Event == EventOptions.Withdraw)
                {
                    Withdraw wd = new Withdraw();
                    wd.BeneficiaryEmailAddress = form.Email;
                    wd.CashierEmailAddress = cashier;
                    wd.Sum = form.Sum;
                    var ret = await WalletWD(wd);
                    if (ret == "1")
                        await context.PostAsync("Thanks for withdraw " + form.Email + "! The cashier at which you withdraw is: " + cashier);
                    else
                        await context.PostAsync("Cannot withdraw: " + ret);
                }
                else
                {
                    await context.PostAsync("Thanks for nothing");
                }
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


public class Withdraw
{
    public string BeneficiaryEmailAddress { get; set; }
    public string CashierEmailAddress { get; set; }
    public int Sum { get; set; }
}





public static async void WalletDP(Deposit deposit)
{
    //try
    //{
    var address = "http://walletapi20170810041706.azurewebsites.net/api/deposit";
    using (var client = new HttpClient())
    {
        client.BaseAddress = new Uri(address);
        var notificationDictionary = new Dictionary<string, string>
                    {
                        { "RecipientEmailAddress", deposit.RecipientEmailAddress },
                        { "CashierEmailAddress", deposit.CashierEmailAddress },
                        { "Sum", deposit.Sum.ToString()}
                    };

        var formContent = new StringContent(JsonConvert.SerializeObject(notificationDictionary), Encoding.UTF8, "application/json");
        var result = await client.PostAsync(address, formContent);
        string resultContent = await result.Content.ReadAsStringAsync();
    }

    //}
    //catch (Exception ex)
    //{
    //}
}

public static async Task<String> WalletWD(Withdraw wd)
{
    //try
    //{
    var address = "http://walletapi20170810041706.azurewebsites.net/api/withdraw";
    using (var client = new HttpClient())
    {
        client.BaseAddress = new Uri(address);
        var notificationDictionary = new Dictionary<string, string>
                    {
                        { "BeneficiaryEmailAddress", wd.BeneficiaryEmailAddress },
                        { "CashierEmailAddress", wd.CashierEmailAddress },
                        { "Sum", wd.Sum.ToString()}
                    };

        var formContent = new StringContent(JsonConvert.SerializeObject(notificationDictionary), Encoding.UTF8, "application/json");
        var result = await client.PostAsync(address, formContent);
        if (result.IsSuccessStatusCode)
        {
            return "1";
            //string resultContent = await result.Content.ReadAsStringAsync();
        }
        else
        {
            var msg = JsonConvert.DeserializeObject<Ret>(await result.Content.ReadAsStringAsync());

            return msg.Message;
        }
    }

    //}
    //catch (Exception ex)
    //{
    //}
}

public class Ret
{
    public string Message { get; set; }
}
