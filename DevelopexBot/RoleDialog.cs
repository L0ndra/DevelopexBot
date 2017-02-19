using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace DevelopexBot
{
    [Serializable]
    public class RoleDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message.Text == "-add")
            {
                var users = (await UserDal.GetAllUsers()).Where(u => !u.IsAdmin).Select(u => u.Id);
                PromptDialog.Choice(context, AddNewAdminAsync, users, "Choose new admin");
            }
            else
            {
                if (message.Text == "-remove")
                {
                    var users = (await UserDal.GetAllUsers()).Where(u => u.IsAdmin).Select(u => u.Id);
                    PromptDialog.Choice(context, RemoveNewAdminAsync, users, "Choose admin to remove");
                }
                else
                {
                    await SendMessages(context, message.Text);
                    await context.PostAsync("Messages sended");
                }
            }
        }

        public async Task AddNewAdminAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var userId = await argument;
            var user = await UserDal.GetUserAsync(userId);
            user.IsAdmin = true;
            await UserDal.AddNewUserToTable(user);
            await context.PostAsync("Admin added");
        }

        public async Task RemoveNewAdminAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var userId = await argument;
            var user = await UserDal.GetUserAsync(userId);
            user.IsAdmin = false;
            await UserDal.AddNewUserToTable(user);
            await context.PostAsync("Admin removed");
        }

        public async Task SendMessages(IDialogContext context, string message)
        {
            var users = await UserDal.GetAllUsers();
            foreach (var user in users)
            {
                Activity activity = JsonConvert.DeserializeObject<Activity>(user.Activity);
                var reply = activity.CreateReply("");
                reply.Text = message;
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
        }
    }
}