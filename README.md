# Yaya Wallet Webhook

### This is the Asp.Net Core Web API project

To run this project,

- Install the .Net 8 sdk and install it
- Clone the project and open it
- Run the project from terminal by typing `dotnet run`

Be sure you're in `C:\Users\...\Yaya.Webhook.API>`

### For testing the project

I used the REST Client of vs code extension, so you can use it to if you wish by installing it ( `vscode:extension/humao.rest-client`). I create a file called request.http and you can use it too and change what we want to change

The project and all the source code lines are self-descriptive as I used a readable naming. I added some comments to explain the codes.

### Structure

1. appsettings.json file consists of Configuration values like SecretKey, list of allowed IPs and time tolerance
2. Program.cs file the entrance and contain all the necessary initialization.
3. After then the request get into the WebhookController.cs file of POST method
