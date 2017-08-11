using System;
using Microsoft.Bot.Builder.FormFlow;

public enum EventOptions { Deposit = 1, Withdraw };
public enum ConfirmationOptions { Yes = 1, No };

// For more information about this template visit http://aka.ms/azurebots-csharp-form
[Serializable]
public class BasicForm
{
    [Prompt("Hi! Would you like to start a deposit or withdrawal?")]
    public ConfirmationOptions Confirmation { get; set; }

    [Prompt("Please select your activity {||}")]
    public EventOptions Event { get; set; }

    [Prompt("How much RON would you like to deposit? Please type a number:")]
    public int Sum { get; set; }

    public static IForm<BasicForm> BuildForm()
    {
        // Builds an IForm<T> based on BasicForm
        return new FormBuilder<BasicForm>().Build();
    }

    public static IFormDialog<BasicForm> BuildFormDialog(FormOptions options = FormOptions.PromptInStart)
    {
        // Generated a new FormDialog<T> based on IForm<BasicForm>
        return FormDialog.FromForm(BuildForm, options);
    }
}
