# LearnMarten
My attempt at trying to make sense of Marten

SpikeMarten is a WebAPI Project, which exposes API calls that are used to issue commands, and source events.

BlazorApp illustrates the consumption of those events using the Marten Async Daemon, and updates the UI accordingly, using SignalR.

To get it working, you need to ensure the "Maven" connection strings in appsettings.json of the two projects above point to a valid Postgresql instance.

Then, run the two projects, and open the Quests menu option in the BlazorApp.
While keeping an eye on that BlazorApp, execute the API Methods (using Postman or similar), in some meaningful sequence:
/StartQuest
/JoinQuest
/Arrive
/JoinQuest
/Slay
/LeaveQuest
etc.

Notice how the BlazorUI gets refreshed automagically with each event!

Still have some questions, but this is pretty sweet so far!
