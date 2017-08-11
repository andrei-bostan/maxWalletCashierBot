#load "BasicForm.csx"

using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

/// This dialog is the main bot dialog, which will call the Form Dialog and handle the results
[Serializable]
public class MainDialog : IDialog<BasicForm>
{
    string URL_Domain = "http://walletapi20170810041706.azurewebsites.net/api/";
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
                var cashiers = GetCashiers();
                await context.PostAsync("Thanks for completing the form you human! Just type anything to restart it." + cashiers.FirstOrDefault().Email);
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

    private List<Cashier> GetCashiers()
    {
        try
        {
            string Url = URL_Domain + "Cashier";

            HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";

            // Get response  
            using (var response = request.GetResponse() as HttWebResponse)
            {
                Stream responseStream = response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    // get the response as text
                    string responseText = reader.ReadToEnd();

                    // convert from text 
                    List<Cashier> results = JsonConvert.DeserializeObject<List<string>>(responseText);
                }
            }

            return results;
        }

        catch (Exception es)
        {
            Console.WriteLine(es);
            Console.ReadLine();
        }

    }
}

public class Cashier
{
    public int Id { get; set; }
    public string Email { get; set; }
}