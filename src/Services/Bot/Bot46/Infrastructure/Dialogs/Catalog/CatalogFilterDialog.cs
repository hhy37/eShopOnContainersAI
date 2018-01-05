using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveCards;
using Bot46.API.Infrastructure.Models;
using Bot46.API.Infrastructure.Modules;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class CatalogFilterDialog : IDialog<CatalogFilter>
    {
        protected int count = 1;
        private static readonly ICatalogService serviceCatalog = ServiceResolver.Get<ICatalogService>();
        private static readonly IComputerVisionService serviceComputerVision = ServiceResolver.Get<IComputerVisionService>();

        public CatalogFilterDialog() {
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowFilter(context);

            context.Wait(AdaptiveMessageReceivedAsync);
        }

        private async Task ShowFilter(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Text = "Choose your selecttion or upload a image to search by image.";
            await context.PostAsync(reply);
            var replyFilter = context.MakeMessage();
            var attachement = await AdaptiveCatalogFilter();
            replyFilter.Attachments = new List<Attachment>() { attachement };
            await context.PostAsync(replyFilter);
        }

        public async Task<Attachment> AdaptiveCatalogFilter()
        {
            AdaptiveCard card = new AdaptiveCard();
            // Add text to the card.
            card.Body.Add(new AdaptiveTextBlock(){
                Text = "Catalog Filter",
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder
            });
           
            // Add list of brands to the card. 
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Brand",
                Weight = AdaptiveTextWeight.Bolder
            });
         
            var brands = await serviceCatalog.GetBrands();
            var brandChoices = new List<AdaptiveChoice>();
            foreach ( var brand in brands){
                brandChoices.Add( new AdaptiveChoice() { Title = brand.Text, Value = brand.Id });
            }

            card.Body.Add(new AdaptiveChoiceSetInput(){
                Id = "Brand",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices  = brandChoices,
                Value = "-1"
            });
          

            // Add list of types to the card. 
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Type",
                Weight = AdaptiveTextWeight.Bolder
            });         
            var types = await serviceCatalog.GetTypes();
            var typeChoices = new List<AdaptiveChoice>();
            foreach (var type in types){
                typeChoices.Add( new AdaptiveChoice() { Title = type.Text, Value = type.Id });
            }

            card.Body.Add(new AdaptiveChoiceSetInput(){
                Id = "Type",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices  = typeChoices,
                Value = "-1"
            });

            card.Actions.Add(new AdaptiveSubmitAction(){
                Title = "Search",
                DataJson = $"{{ 'ActionType': '{BotActionTypes.CatalogFilter}' }}"
            });
          
            // Create the attachment.
            Attachment attachment = new Attachment(){
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;
        }


        public virtual async Task AdaptiveMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Value != null)
            {
                // Got an Action Submit
                dynamic value = message.Value;
                switch (value.ActionType.ToString())
                {
                    case BotActionTypes.CatalogFilter:
                        CatalogFilter filter = CatalogFilter.Map(value);
                        context.Done(filter);
                        break;
                }
                return;
            }
            else
            {
                var content = await HandleAttachments(context, message);
                if(content != null)
                {
                    var tags = await serviceComputerVision.ClassifyImageAsync(content);
                    CatalogFilter filter = new CatalogFilter() { Tags = tags };
                    context.Done(filter);
                    return;
                }
            }
            await context.PostAsync("Please choose your selection or upload a image.");
            await ShowFilter(context);
            context.Wait(AdaptiveMessageReceivedAsync);
        }

        public async Task<byte[]> HandleAttachments(IDialogContext context, IMessageActivity message){
            byte[] content = null;
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];
                var client = new ConnectorClient(new Uri(context.Activity.ServiceUrl), new MicrosoftAppCredentials());
                content = await client.HttpClient.GetByteArrayAsync(attachment.ContentUrl);
            }
            return content;
        }
    }
}