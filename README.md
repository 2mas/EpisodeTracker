# README #

A Tv-show tracker that uses TheTVDB.com API v2. Search and add series to your trackinglist and get notified when new episodes are aired for your favourite series.

Right now only "valid" episodes are being processed. There are special-episodes and episodes without a correctly formatted air-date that will be ignored from processing.

Feel free to contribute!

### How to run ###

To run this you need

1. Your own api-keys at TheTVDB.com
2. Modify smtp.config and episodeTracker.config with your setup

episodeTracker.config should contain, for example:
```
<episodeTracker>
    <notificationSettings notificationType="Email/None" notificationEmails="comma-separated list of emails" />
    <apiSettings apiUrl="https://api.thetvdb.com/" apiKey="" apiUser="" apiUserKey="" />
</episodeTracker>
```

smtp.config should contain your smtp-config if Email is chosen as notificationType, for example in testing:
```
<smtp deliveryMethod="SpecifiedPickupDirectory" from="sender@episodetracker">
    <specifiedPickupDirectory pickupDirectoryLocation="C:\tmp\"/>
</smtp>
```

### Examples ###

A small example-application as a commandline-project is provided with a json-file storage.

The cli could be wrapped in a windows-service (or just the windows scheduler perhaps) to call the update-method frequently without having to manually do much except mark episodes as seen and track new series.

(At this point, if any starting arguments are given to the CLI executable, only the update-method is run without any output.)
